using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class PurchaseRewardsModuleTests : ZenjectUnitTestFixture
    {
        private readonly string _redDotPath = "test";
        
        public override void Setup()
        {
            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 100, "test");
            
            base.Setup();
            
            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);
            
            Mock<IEventScheduleDatabase> eventScheduleDatabaseMock = new();
            eventScheduleDatabaseMock.Setup(d => d.GetEventScheduleDatas())
                .Returns(new List<EventScheduleData>());
            eventScheduleDatabaseMock.Setup(d => d.GetEventScheduleDatas())
                .Returns(new List<EventScheduleData>
                {
                    new EventScheduleData(11, "purchase_rewards", 11, true, 1, 7),
                    new EventScheduleData(21, "purchase_rewards", 21, true, 1, 7),
                });
            
            Mock<IPurchaseRewardsDatabase> purchaseRewardsDatabaseMock = new();
            purchaseRewardsDatabaseMock.Setup(d => d.GetPurchaseRewardsType(11)).Returns(PurchaseRewardsType.Amount);
            purchaseRewardsDatabaseMock.Setup(d => d.IsAmountEventAvailable(11, "USD")).Returns(true);
            purchaseRewardsDatabaseMock.Setup(d => d.IsAmountEventAvailable(11, "KRW")).Returns(false);
            purchaseRewardsDatabaseMock.Setup(d => d.GetAmountEventData(11, "USD"))
                .Returns(new PurchaseRewardsEventData
                {
                    type = PurchaseRewardsType.Amount,
                    purchaseRewardsList = new List<PurchaseRewardsData>
                    {
                        new PurchaseRewardsData
                        {
                            objective = 100,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                        new PurchaseRewardsData
                        {
                            objective = 200,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                        new PurchaseRewardsData
                        {
                            objective = 300,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                    }
                });
            
            purchaseRewardsDatabaseMock.Setup(d => d.GetPurchaseRewardsType(21)).Returns(PurchaseRewardsType.Day);
            purchaseRewardsDatabaseMock.Setup(d => d.GetDayEventData(21))
                .Returns(new PurchaseRewardsEventData
                {
                    type = PurchaseRewardsType.Day,
                    purchaseRewardsList = new List<PurchaseRewardsData>
                    {
                        new PurchaseRewardsData
                        {
                            objective = 1,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                        new PurchaseRewardsData
                        {
                            objective = 2,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                        new PurchaseRewardsData
                        {
                            objective = 3,
                            rewards = new List<Property> {new Property("test", 100)}
                        },
                    }
                });
            purchaseRewardsDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_redDotPath);
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test", false))
                .Returns(new List<Property> {new Property("test", 100)});

            Container.Bind<SaveDataManager>().AsSingle();
            Container.Resolve<SaveDataManager>().LinkFileDataSaver(false);
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle();
            Container.Bind<SimpleValues>().AsSingle();
            Container.Bind<IEventScheduleDatabase>().FromInstance(eventScheduleDatabaseMock.Object).AsSingle();
            Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle();
            Container.Bind<PropertyManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle();
            Container.Bind<IPurchaseRewardsDatabase>().FromInstance(purchaseRewardsDatabaseMock.Object).AsSingle();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle();
            EventScheduleModuleInstaller.Install(Container);
            PurchaseRewardsModuleInstaller.Install(Container);
        }

        [Test]
        public void InitializeTest()
        {
            PurchaseRewardsEventManager purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            Assert.IsTrue(purchaseRewardsEventManager.ActiveEvents.Count == 2);
        }

        [Test]
        public void PurchaseDayRewardsTest()
        {
            PurchaseRewardsEventManager purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            MockIAPManager iapManager = (MockIAPManager) Container.Resolve<IIAPManager>();

            iapManager.Price = 100;
            iapManager.IsSuccess = true;
            
            iapManager.FakePurchase("test");
            iapManager.FakePurchase("test");
            
            PurchaseRewardsEvent dayEvent = purchaseRewardsEventManager.ActiveEvents.Find(e => e.Type == PurchaseRewardsType.Day);
            Assert.IsTrue(dayEvent.PurchaseRewardsList[0].IsClaimable);
            Assert.IsFalse(dayEvent.PurchaseRewardsList[1].IsClaimable);
            Assert.IsFalse(dayEvent.PurchaseRewardsList[2].IsClaimable);
        }

        [Test]
        public void PurchaseAmountRewardsTest()
        {
            PurchaseRewardsEventManager purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            MockIAPManager iapManager = (MockIAPManager) Container.Resolve<IIAPManager>();
            PurchaseRewardsEvent amountEvent = purchaseRewardsEventManager.ActiveEvents.Find(e => e.Type == PurchaseRewardsType.Amount);

            iapManager.Price = 100;
            iapManager.IsSuccess = true;
            
            Assert.IsFalse(amountEvent.PurchaseRewardsList[0].IsClaimable);
            Assert.IsFalse(amountEvent.PurchaseRewardsList[1].IsClaimable);
            Assert.IsFalse(amountEvent.PurchaseRewardsList[2].IsClaimable);
            
            iapManager.FakePurchase("test");
            iapManager.FakePurchase("test");
            
            Assert.IsTrue(amountEvent.PurchaseRewardsList[0].IsClaimable);
            Assert.IsTrue(amountEvent.PurchaseRewardsList[1].IsClaimable);
            Assert.IsFalse(amountEvent.PurchaseRewardsList[2].IsClaimable);
        }

        [Test]
        public void EventEndTest()
        {
            PurchaseRewardsEventManager purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            Clock.AddDebugOffset(TimeSpan.FromDays(10));
            
            Assert.IsTrue(purchaseRewardsEventManager.ActiveEvents.Count == 0);
        }

        [Test]
        public void NoCurrencyCodeTest()
        {
            MockIAPManager iapManager = (MockIAPManager) Container.Resolve<IIAPManager>();
            iapManager.CurrencyCode = "KRW";
            
            PurchaseRewardsEventManager purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            Assert.IsTrue(purchaseRewardsEventManager.ActiveEvents.Count == 1);
            Assert.IsTrue(!purchaseRewardsEventManager.ActiveEvents.Exists(e => e.Type == PurchaseRewardsType.Amount));
        }

        [Test]
        public void RedDotTest()
        {
            var purchaseRewardsEventManager = Container.Resolve<PurchaseRewardsEventManager>();
            var activeEvents = purchaseRewardsEventManager.ActiveEvents;

            var amountEvent = activeEvents.Find(e => e.Type == PurchaseRewardsType.Amount);
            
            MockIAPManager iapManager = (MockIAPManager) Container.Resolve<IIAPManager>();
            iapManager.Price = 100;
            iapManager.IsSuccess = true;
            
            iapManager.FakePurchase("test");
            iapManager.FakePurchase("test");
            iapManager.FakePurchase("test");
            
            Assert.IsTrue(RedDot.GetNotified(amountEvent.PurchaseRewardsList[0].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(amountEvent.PurchaseRewardsList[1].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(amountEvent.PurchaseRewardsList[2].RedDotPath));
            
            amountEvent.PurchaseRewardsList[0].Claim(PlayerAction.TEST);

            Assert.IsFalse(RedDot.GetNotified(amountEvent.PurchaseRewardsList[0].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(amountEvent.PurchaseRewardsList[1].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(amountEvent.PurchaseRewardsList[2].RedDotPath));
        }

        public override void Teardown()
        {
            PropertyType.Clear();
            Container.Resolve<SaveDataManager>().Reset();
            Clock.Initialize();
            Clock.ResetDebugOffset();
            base.Teardown();
        }
    }
}
