using System.Collections.Generic;
using NUnit.Framework;
using Zenject;
using Assert = UnityEngine.Assertions.Assert;

namespace Aloha.Coconut.Tests.Editor
{
    public class GrowthLevelTests : ZenjectUnitTestFixture
    {
        private PropertyManager _propertyManager;
        private PropertyType _exp;
        private GrowthLevel.Factory _growthLevelFactory;

        public override void Setup()
        {
            base.Setup();
            
            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 9999, "testExp");
            _exp = PropertyType.Get("testExp");

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();
            
            Container.Bind<SaveDataManager>().FromInstance(saveDataManager);
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.Bind<GrowthLevel.Factory>().AsSingle().NonLazy();
            Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            
            _propertyManager = Container.Resolve<PropertyManager>();
            _growthLevelFactory = Container.Resolve<GrowthLevel.Factory>();
        }

        [Test]
        public void FirstLevelTest()
        {
            var expRequired = new List<int>() { 10, 10, 10, 10, 10 };
            var growthLevel = _growthLevelFactory.Create(_exp, expRequired);
            Assert.AreEqual(1, growthLevel.Level);
        }

        [Test]
        public void SimpleLevelUpTest()
        {
            var expRequired = new List<int>() { 10, 10, 10, 10, 10 };
            var growthLevel = _growthLevelFactory.Create(_exp, expRequired);
            _propertyManager.Obtain(new Property(_exp, 15), PlayerAction.TEST);

            Assert.AreEqual(expRequired.Count + 1, growthLevel.MaxLevel);
            Assert.AreEqual(2, growthLevel.Level);
            Assert.AreEqual(10, growthLevel.ExpRequirement);
            Assert.AreEqual(5, growthLevel.CurrentExp);
        }

        [Test]
        public void MaxLevelTest()
        {
            var expRequired = new List<int>() { 10, 10, 10, 10, 10 };
            var growthLevel = _growthLevelFactory.Create(_exp, expRequired);
            _propertyManager.Obtain(new Property(_exp, 50), PlayerAction.TEST);
            
            Assert.AreEqual(growthLevel.MaxLevel, growthLevel.Level);
        }

        [Test]
        public void DoubleLevelUpTest()
        {
            var expRequired = new List<int>() { 10, 10, 10, 10, 10 };
            var growthLevel = _growthLevelFactory.Create(_exp, expRequired);
            _propertyManager.Obtain(new Property(_exp, 15), PlayerAction.TEST);
            _propertyManager.Obtain(new Property(_exp, 15), PlayerAction.TEST);
            
            Assert.AreEqual(4, growthLevel.Level);
        }
    }
}