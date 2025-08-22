using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UniRx;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class RushEventModuleTests : ZenjectUnitTestFixture
    {
        private readonly string _redDotPath = "test";
        private readonly string _testActionName = "test";
        private readonly Subject<(string, int)> _fakeActionProgressSubject = new();
        
        public override void Setup()
        {
            base.Setup();

            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 11, "testPropertyType");
            PropertyType.AddType(PropertyTypeGroup.Default, 12, "testReward");

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.Initialize();
            Clock.DebugSetNow(now);

            Mock<IEventScheduleDatabase> eventScheduleDatabaseMock = new();
            eventScheduleDatabaseMock.Setup(d => d.GetEventScheduleDatas())
                .Returns(new List<EventScheduleData>
                {
                    new(11, "rush", 11, false, 20241201, 20241207),
                    new(21, "rush", 21, false, 20241201, 20241207),
                });

            Container.BindInstance(eventScheduleDatabaseMock.Object).AsSingle().NonLazy();

            Mock<IRushEventDatabase> rushEventDatabaseMock = new();
            rushEventDatabaseMock.Setup(d => d.GetRushEventData(It.IsAny<int>()))
                .Returns((int eventId) =>
                {
                    int id = eventId;
                    PropertyType targetPropertyType = PropertyType.Get("testPropertyType");

                    int totalRound = 4;
                    List<Property> roundRewards = new()
                    {
                        new Property(PropertyType.Get("testReward"), 100),
                        new Property(PropertyType.Get("testReward"), 200),
                        new Property(PropertyType.Get("testReward"), 300)
                    };
                    List<RushEventMissionData> missionDatas = new List<RushEventMissionData>
                    {
                        new()
                        {
                            objective = 100, rewards = new List<Property> { new(PropertyType.Get("testReward"), 10) }
                        },
                        new()
                        {
                            objective = 200, rewards = new List<Property> { new(PropertyType.Get("testReward"), 20) }
                        },
                        new()
                        {
                            objective = 300, rewards = new List<Property> { new(PropertyType.Get("testReward"), 30) }
                        }
                    };
                    RushEventMissionGroupData missionGroupData = new RushEventMissionGroupData
                    {
                        totalRound = totalRound, roundRewards = roundRewards, missionDatas = missionDatas
                    };

                    int defaultProductId = eventId * 1000;
                    List<RushEventPackageData> packageDatas = new List<RushEventPackageData>
                    {
                        new() { id = defaultProductId + 1, packageId = "test1", isFree = true, limit = 1 },
                        new() { id = defaultProductId + 2, packageId = "test2", isFree = false, limit = 1 },
                        new() { id = defaultProductId + 3, packageId = "test3", isFree = false, limit = 1 }
                    };
                    RushEventPackageGroupData packageGroupData = new RushEventPackageGroupData
                        { packageDatas = packageDatas };

                    return new RushEventData
                    {
                        targetAction = _testActionName, missionGroupData = missionGroupData,
                        packageGroupData = packageGroupData
                    };
                });
            rushEventDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_redDotPath);

            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> { new(PropertyType.Get("testReward"), 1000) });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> { new(PropertyType.Get("testReward"), 2000) });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property> { new(PropertyType.Get("testReward"), 3000) });
            
            Mock<IRushEventProgressHandler> rushEventProgressHandlerMock = new();
            rushEventProgressHandlerMock.Setup(m => m.OnProgressAdded)
                .Returns(_fakeActionProgressSubject);

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);

            Container.BindInstance(rushEventDatabaseMock.Object).AsSingle();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle();
            Container.BindInstance(rushEventProgressHandlerMock.Object).AsSingle();
            Container.BindInstance(saveDataManager).AsSingle();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EventScheduleManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.Bind<Product.Factory>().AsSingle().NonLazy();
            Container.Bind<LimitedProduct.Factory>().AsSingle().NonLazy();

            RushEventModuleInstaller.Install(Container);
        }

        [Test]
        public void InitializeTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);

            Assert.AreEqual(3, rushEvent.MissionGroup.Missions.Count);
            Assert.AreEqual(4, rushEvent.MissionGroup.TotalRound);
            Assert.AreEqual(1, rushEvent.MissionGroup.CurrentRound);

            Assert.IsTrue(rushEvent.PackageGroup.LimitedProducts.Count == 3);
            Assert.IsNotNull(rushEvent.PackageGroup.LimitedProducts[0]);
            Assert.IsNotNull(rushEvent.PackageGroup.LimitedProducts[1]);
            Assert.IsNotNull(rushEvent.PackageGroup.LimitedProducts[2]);
        }

        [Test]
        public void RushEventMissionTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);

            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[0].Progress);
            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[1].Progress);
            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[2].Progress);
            
            _fakeActionProgressSubject.OnNext((_testActionName, 300));

            Assert.IsTrue(rushEvent.MissionGroup.Missions[0].Progress >= rushEvent.MissionGroup.Missions[0].Objective);
            Assert.IsTrue(rushEvent.MissionGroup.Missions[1].Progress >= rushEvent.MissionGroup.Missions[1].Objective);
            Assert.IsTrue(rushEvent.MissionGroup.Missions[2].Progress >= rushEvent.MissionGroup.Missions[2].Objective);

            rushEvent.MissionGroup.Missions[0].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[1].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[2].Claim(PlayerAction.TEST);

            Assert.IsTrue(rushEvent.MissionGroup.IsRoundRewardsClaimable);

            rushEvent.MissionGroup.ClaimRoundRewards(PlayerAction.TEST);

            Assert.AreEqual(2, rushEvent.MissionGroup.CurrentRound);
            
            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[0].Progress);
            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[1].Progress);
            Assert.AreEqual(0, rushEvent.MissionGroup.Missions[2].Progress);

            _fakeActionProgressSubject.OnNext((_testActionName, 100000));

            rushEvent.MissionGroup.Missions[0].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[1].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[2].Claim(PlayerAction.TEST);
            
            rushEvent.MissionGroup.ClaimRoundRewards(PlayerAction.TEST);
            
            // 라운드 넘어갔을 때 미션 Progress가 정상적으로 차감되는지 확인
            Assert.AreEqual(99700, rushEvent.MissionGroup.Missions[0].Progress);
            Assert.AreEqual(99700, rushEvent.MissionGroup.Missions[1].Progress);
            Assert.AreEqual(99700, rushEvent.MissionGroup.Missions[2].Progress);
        }

        [Test]
        public void RushEventPackageTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);

            Container.Resolve<MockIAPManager>().IsSuccess = true;
            rushEvent.PackageGroup.LimitedProducts[0].Purchase(PlayerAction.TEST);
            rushEvent.PackageGroup.LimitedProducts[1].Purchase(PlayerAction.TEST);
            rushEvent.PackageGroup.LimitedProducts[2].Purchase(PlayerAction.TEST);
            
            Assert.AreEqual(0, rushEvent.PackageGroup.LimitedProducts[0].Remaining);
            Assert.AreEqual(0, rushEvent.PackageGroup.LimitedProducts[1].Remaining);
            Assert.AreEqual(0, rushEvent.PackageGroup.LimitedProducts[2].Remaining);
        }

        [Test]
        public void RushEventEndTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            
            Assert.AreEqual(2, rushEventManager.ActiveEvents.Count);
            
            Clock.AddDebugOffset(TimeSpan.FromDays(7));
            
            Assert.AreEqual(0, rushEventManager.ActiveEvents.Count);
        }

        [Test]
        public void RushEventMissionRedDotTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);

            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[0].RedDotPath));
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[1].RedDotPath));
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[2].RedDotPath));
            
            _fakeActionProgressSubject.OnNext((_testActionName, 300));
            
            Assert.IsTrue(RedDot.GetNotified(rushEvent.MissionGroup.Missions[0].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(rushEvent.MissionGroup.Missions[1].RedDotPath));
            Assert.IsTrue(RedDot.GetNotified(rushEvent.MissionGroup.Missions[2].RedDotPath));
            
            rushEvent.MissionGroup.Missions[0].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[1].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[2].Claim(PlayerAction.TEST);
            
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[0].RedDotPath));
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[1].RedDotPath));
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.Missions[2].RedDotPath));
        }

        [Test]
        public void RushEventPackageRedDotTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);

            var freeProduct = rushEvent.PackageGroup.LimitedProducts.Find(p => p.Product.Price is FreePrice);

            Assert.IsTrue(RedDot.GetNotified(freeProduct.RedDotPath));
            
            Container.Resolve<MockIAPManager>().IsSuccess = true;
            freeProduct.Purchase(PlayerAction.TEST);
            
            Assert.IsFalse(RedDot.GetNotified(freeProduct.RedDotPath));
        }

        [Test]
        public void RushEventRoundRewardsRedDotTest()
        {
            var rushEventManager = Container.Resolve<RushEventManager>();
            var rushEvent = rushEventManager.ActiveEvents.Find(ev => ev.EventScheduleId == 11);
            Assert.NotNull(rushEvent);
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.RoundRewardsRedDotPath));
            
            _fakeActionProgressSubject.OnNext((_testActionName, 300));
            
            rushEvent.MissionGroup.Missions[0].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[1].Claim(PlayerAction.TEST);
            rushEvent.MissionGroup.Missions[2].Claim(PlayerAction.TEST);
            
            Assert.IsTrue(RedDot.GetNotified(rushEvent.MissionGroup.RoundRewardsRedDotPath));
            
            rushEvent.MissionGroup.ClaimRoundRewards(PlayerAction.TEST);
            
            Assert.IsFalse(RedDot.GetNotified(rushEvent.MissionGroup.RoundRewardsRedDotPath));
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