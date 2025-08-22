using System.Collections.Generic;
using Aloha.Coconut;
using NUnit.Framework;
using UnityEngine;
using Zenject;

public class PropertyExchangeTests : ZenjectUnitTestFixture
{
    private static PropertyType TestPropertyType => PropertyType.Get("Test");
    private static PropertyType Test2PropertyType => PropertyType.Get("Test2");
    private static PropertyType ExchangeTestPropertyType => PropertyType.Get("ExchangeTest"); //TestPropertyType PER_EXCHANGE로 교환 
    private static PropertyType ExchangeTest2PropertyType => PropertyType.Get("ExchangeTest2"); //TestPropertyType PER_EXCHANGE * 2개로 교환
    private static PropertyTypeGroup ExchangeTestGroup => (PropertyTypeGroup)12345;
    private static PropertyTypeGroup ExchangeTestGroup2 => (PropertyTypeGroup)12346;
    private const int PER_EXCHANGE = 100;
    
    private PropertyManager PropertyManager => Container.Resolve<PropertyManager>();
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        PlayerAction.Load();
    }
    
    public override void Setup()
    {
        base.Setup();
        
        PropertyType.Clear();
        PropertyType.AddType(PropertyTypeGroup.Default, int.MaxValue, "Test");
        PropertyType.AddType(PropertyTypeGroup.Default, int.MaxValue - 1, "Test2");
        PropertyType.AddType(ExchangeTestGroup, 1, "ExchangeTest");
        PropertyType.AddType(ExchangeTestGroup2, 1, "ExchangeTest2");
        
        var saveDataManager = new SaveDataManager();
        saveDataManager.LinkFileDataSaver();

        Container.BindInstance(saveDataManager).AsSingle();
        Container.Bind<PropertyManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle();
        Container.BindInterfacesTo<TestPropertyExchanger>().AsSingle();
        Container.BindInterfacesTo<TestPropertyExchanger2>().AsSingle();
    }

    [Test]
    public void OneExchangeTest()
    {
        var result = PropertyManager.Obtain(TestExchangeProperty(1), PlayerAction.TEST);
        
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0].type == TestPropertyType);
        Assert.IsTrue((int)result[0].amount == PER_EXCHANGE);
        Assert.AreEqual(PER_EXCHANGE, (int)PropertyManager.GetBalance(TestPropertyType));
    }

    [Test]
    public void MultiExchangeTest()
    {
        var tryGetList = new List<Property>() { TestExchangeProperty(1), TestExchangeProperty(1) };
        var result = PropertyManager.Obtain(tryGetList, PlayerAction.TEST);
        
        Assert.IsTrue(result.Count == 2);
        Assert.AreEqual(PER_EXCHANGE * 2, (int)PropertyManager.GetBalance(TestPropertyType));
    }

    [Test]
    public void OneExchangeTestCountChanged()
    {
        var result = PropertyManager.Obtain(TestExchangeProperty2(1), PlayerAction.TEST);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(PER_EXCHANGE * 2, (int)PropertyManager.GetBalance(TestPropertyType));
    }

    [Test]
    public void DependantExchangeTest()
    {
        Container.BindInterfacesTo<TestPropertyDependantExchanger>().AsSingle();
        
        var result = PropertyManager.Obtain(TestProperty(100), PlayerAction.TEST);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(TestPropertyDependantExchanger.TEST_PROPERTY_MAX, (int)PropertyManager.GetBalance(TestPropertyType));
        Assert.AreEqual(100 - TestPropertyDependantExchanger.TEST_PROPERTY_MAX, (int)PropertyManager.GetBalance(Test2PropertyType));
    }

    [Test]
    public void DependantExchangeTest2()
    {
        Container.BindInterfacesTo<TestPropertyDependantExchanger>().AsSingle();
        
        var result = PropertyManager.Obtain(new List<Property>() {TestProperty(2), TestProperty(2), TestProperty(2)}, PlayerAction.TEST);
        Assert.AreEqual(TestPropertyDependantExchanger.TEST_PROPERTY_MAX, (int)PropertyManager.GetBalance(TestPropertyType));
        Assert.AreEqual(6 - TestPropertyDependantExchanger.TEST_PROPERTY_MAX, (int)PropertyManager.GetBalance(Test2PropertyType));
    }

    private class TestPropertyExchanger : IPropertyExchanger
    {
        public List<PropertyTypeGroup> HandlingGroups => new List<PropertyTypeGroup> {ExchangeTestGroup};
        
        public List<Property> Exchange(Property property)
        {
            var exchanged = new List<Property>();
            exchanged.Add(TestProperty(PER_EXCHANGE * (int)property.amount));
            return exchanged;
        }
    }
    
    private class TestPropertyExchanger2 : IPropertyExchanger
    {
        public List<PropertyTypeGroup> HandlingGroups => new List<PropertyTypeGroup> {ExchangeTestGroup2};
        
        public List<Property> Exchange(Property property)
        {
            var exchanged = new List<Property>();
            exchanged.Add(TestProperty(PER_EXCHANGE * (int)property.amount));
            exchanged.Add(TestProperty(PER_EXCHANGE * (int)property.amount));
            return exchanged;
        }
    }
    
    // Test를 5개 이상 보유하면 Test2로 교환
    private class TestPropertyDependantExchanger : IPropertyExchanger
    {
        public const int TEST_PROPERTY_MAX = 5;
        
        private readonly DefaultPropertyHandler _defaultPropertyHandler;
        public List<PropertyTypeGroup> HandlingGroups => new List<PropertyTypeGroup> {PropertyTypeGroup.Default};

        public TestPropertyDependantExchanger(DefaultPropertyHandler defaultPropertyHandler)
        {
            _defaultPropertyHandler = defaultPropertyHandler;
        }
        
        public List<Property> Exchange(Property property)
        {
            if (property.type == TestPropertyType)
            {
                var availableCount = Mathf.Min(TEST_PROPERTY_MAX - (int)_defaultPropertyHandler.GetBalance(TestPropertyType), (int)property.amount) ;
                var overCount = (int)property.amount - availableCount;
                    
                var exchanged = new List<Property>();
                if(availableCount > 0) exchanged.Add(TestProperty(availableCount));
                if(overCount > 0) exchanged.Add(Test2Property(overCount));
                
                return exchanged;
            }
            
            return new List<Property> {property};
        }
    }

    private static Property TestProperty(int amount)
    {
        return new Property(TestPropertyType, amount);
    }
    
    private static Property Test2Property(int amount)
    {
        return new Property(Test2PropertyType, amount);
    }
    
    private static Property TestExchangeProperty(int amount)
    {
        return new Property(ExchangeTestPropertyType, amount);
    }
    
    private static Property TestExchangeProperty2(int amount)
    {
        return new Property(ExchangeTest2PropertyType, amount);
    }

    public override void Teardown()
    {
        PropertyType.DeleteType("Test");
        PropertyType.DeleteType("ExchangeTest");
        PropertyType.DeleteType("ExchangeTest2");
        base.Teardown();
    }
}