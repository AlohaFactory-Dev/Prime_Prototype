using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aloha.Coconut.IAP;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class HardCurrencyModuleTests : ZenjectUnitTestFixture
    {
        public override void Setup()
        {
            base.Setup();

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);

            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 100, "test");

            Mock<IHardCurrencyProductGroupDatabase> productGroupDatabaseMock = new();
            productGroupDatabaseMock.Setup(d => d.GetProductGroupDataList())
                .Returns(new List<HardCurrencyProductGroupData>
                {
                    new(1, "test1", "test1_1"),
                    new(2, "test2", "test2_1"),
                    new(3, "test3", "test3_1")
                });
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> {new("test", 100)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> {new("test", 200)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property> {new("test", 300)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1_1", false))
                .Returns(new List<Property> {new("test", 200)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2_1", false))
                .Returns(new List<Property> {new("test", 400)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3_1", false))
                .Returns(new List<Property> {new("test", 600)});

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);

            Container.BindInstance(saveDataManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle().NonLazy();
            Container.BindInstance(productGroupDatabaseMock.Object).AsSingle().NonLazy();

            HardCurrencyModuleInstaller.Install(Container);
        }

        [Test]
        public async Task PurchaseAndResetTest()
        {
            var hardCurrencyProductsManager = Container.Resolve<HardCurrencyProductsManager>();
            var hardCurrencyProductGroups = hardCurrencyProductsManager.HardCurrencyProductGroups;
            var targetProduct = hardCurrencyProductGroups.Find(group => group.Product.IAPId == "test3_1");

            var propertyManager = Container.Resolve<PropertyManager>();
            var iapManager = Container.Resolve<MockIAPManager>();

            Assert.IsNotNull(targetProduct);

            iapManager.IsSuccess = true;
            await targetProduct.Purchase();

            Assert.IsTrue(targetProduct.IsDoublePurchased);
            Assert.AreEqual("test3", targetProduct.Product.IAPId);

            var balance = propertyManager.GetBalance(PropertyType.Get("test"));
            Assert.AreEqual(600, (int)balance);

            // 주간 리셋
            Clock.AddDebugOffset(TimeSpan.FromDays(7));

            Assert.IsFalse(targetProduct.IsDoublePurchased);
            Assert.AreEqual("test3_1", targetProduct.Product.IAPId);
        }

        public override void Teardown()
        {
            PropertyType.Clear();
            Clock.ResetDebugOffset();
            Container.Resolve<SaveDataManager>().Reset();
            base.Teardown();
        }
    }
}