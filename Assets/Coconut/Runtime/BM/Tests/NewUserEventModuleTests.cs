using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Aloha.Coconut.IAP;
using Aloha.Coconut.Missions;
using Cysharp.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class NewUserEventModuleTests : ZenjectUnitTestFixture
    {
        private PropertyTypeGroup _newUserEventExpTypeGroup = (PropertyTypeGroup)1;
        private string _testRedDotPath = "test";

        [Mission(MissionType.Default)]
        private class TestMission1 : Mission
        {
            public TestMission1(MissionData missionData, PropertyManager propertyManager, SaveData saveData = null) :
                base(missionData, propertyManager, saveData) { }

            public override void Start()
            {
                SetProgress(0);
            }

            public void ForceSetProgress(BigInteger progress)
            {
                SetProgress(progress);
            }
        }

        public override void Setup()
        {
            base.Setup();
            
            RedDot.Initialize();
            PropertyType.Load();
            PropertyType.AddType(_newUserEventExpTypeGroup, 1, "NewUserEventExp");

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.Initialize();
            Clock.DebugSetNow(now);
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> {new("Diamond", 100)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> {new("Diamond", 100)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property> {new("Diamond", 100)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test4", false))
                .Returns(new List<Property> {new("Diamond", 100)});

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);

            Container.BindInstance(saveDataManager).AsSingle();
            Container.Bind<Product.Factory>().AsSingle();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.Bind<LimitedProduct.Factory>().AsSingle().NonLazy();

            Mock<INewUserEventDatabase> newUserEventDatabaseMock = new();
            newUserEventDatabaseMock.Setup(d => d.GetPassNodeDataList())
                .Returns(GetTestNodeDataList(10));
            newUserEventDatabaseMock.Setup(d => d.GetMissionGroupDataList())
                .Returns(GetTestMissionGroupDataList(7));
            newUserEventDatabaseMock.Setup(d => d.GetPackageGroupDataList())
                .Returns(GetTestPackageGroupDataList(7));
            newUserEventDatabaseMock.Setup(d => d.GetExpTypeGroup())
                .Returns(_newUserEventExpTypeGroup);
            newUserEventDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_testRedDotPath);
            Container.BindInstance(newUserEventDatabaseMock.Object).AsSingle();

            MissionTypeData.Load();

            NewUserEventModuleInstaller.Install(Container);
        }

        private List<PassNodeData> GetTestNodeDataList(int nodeCount)
        {
            List<PassNodeData> result = new List<PassNodeData>();
            for (int i = 1; i < nodeCount + 1; i++)
            {
                result.Add(new PassNodeData
                {
                    passLevel = i + 1,
                    reward1Alias = PropertyTypeAlias.Diamond.ToString(),
                    reward1Amount = 100
                });
            }

            return result;
        }

        private List<NewUserMissionGroupData> GetTestMissionGroupDataList(int count)
        {
            List<NewUserMissionGroupData> result = new List<NewUserMissionGroupData>();
            var expPropertyType = PropertyType.Get(_newUserEventExpTypeGroup, 1);
            for (int i = 0; i < count; i++)
            {
                result.Add(new NewUserMissionGroupData
                {
                    day = i + 1,
                    missionDataList = new List<MissionData>
                    {
                        new()
                        {
                            id = 1,
                            type = MissionType.Default,
                            var = 0,
                            objective = 1,
                            rewards = new List<Property>
                            {
                                new(expPropertyType, 1),
                                new("Coin", 100)
                            }
                        }
                    }
                });
            }

            return result;
        }

        private List<NewUserPackageGroupData> GetTestPackageGroupDataList(int count)
        {
            List<NewUserPackageGroupData> result = new List<NewUserPackageGroupData>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new NewUserPackageGroupData
                {
                    day = i + 1,
                    packageDataList = new List<NewUserPackageData>
                    {
                        new() { isFree = true, iapId = "test1", limit = 1 },
                        new() { isFree = false, iapId = "test2", limit = 2 },
                        new() { isFree = false, iapId = "test3", limit = 2 },
                        new() { isFree = false, iapId = "test4", limit = 2 },
                    }
                });
            }

            return result;
        }

        [Test]
        public void InitializeTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            Assert.IsNotNull(newUserEvent.ProgressPass);
            Assert.IsTrue(newUserEvent.IsActivated);

            Assert.IsTrue(newUserEvent.MissionGroupList.Count > 0);
            Assert.IsNotNull(newUserEvent.MissionGroupList[0].MissionList);
            Assert.IsTrue(newUserEvent.MissionGroupList[0].MissionList.Count > 0);
            Assert.IsTrue(newUserEvent.PackageGroupList.Count > 0);
            Assert.IsNotNull(newUserEvent.PackageGroupList[0].ProductList);
            Assert.IsTrue(newUserEvent.PackageGroupList[0].ProductList.Count > 0);
        }

        [Test]
        public void NewUserEventStartAndEndTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            var missionGroups = newUserEvent.MissionGroupList;

            CheckIsDailyMissionStarted(missionGroups, 1);

            Clock.AddDebugOffset(TimeSpan.FromDays(6));

            CheckIsDailyMissionStarted(missionGroups, 7);

            Clock.AddDebugOffset(TimeSpan.FromDays(4));

            Assert.IsFalse(newUserEvent.IsActivated);
        }

        private void CheckIsDailyMissionStarted(List<NewUserMissionGroup> missionGroups, int currentDay)
        {
            foreach (var missionGroup in missionGroups)
            {
                if (missionGroup.Day <= currentDay)
                {
                    Assert.IsTrue(missionGroup.IsMissionStarted);
                }
                else
                {
                    Assert.IsFalse(missionGroup.IsMissionStarted);
                }
            }
        }

        [Test]
        public void NewUserEventMissionTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            var propertyManager = Container.Resolve<PropertyManager>();
            var missionGroups = newUserEvent.MissionGroupList;
            var missionGroup = missionGroups.Find(m => m.Day == 1);

            Assert.IsTrue(missionGroup.IsMissionStarted);

            var mission = missionGroup.MissionList[0];

            ((TestMission1)mission).ForceSetProgress(1);
            Assert.IsTrue(mission.IsCompleted);
            Assert.IsTrue(mission.IsRewardsClaimable);

            mission.Claim(PlayerAction.TEST);

            Assert.IsTrue(mission.IsRewardsClaimed);

            var level = newUserEvent.ProgressPass.CurrentLevel;
            var coin = propertyManager.GetBalance(PropertyType.Get("Coin"));
            // 레벨은 1부터 시작이므로 2레벨이어야 함
            Assert.AreEqual(2, level);
            Assert.AreEqual(100, (int)coin);
        }

        [Test]
        public void NewUserEventPassTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            var propertyManager = Container.Resolve<PropertyManager>();
            var pass = newUserEvent.ProgressPass;

            // 레벨 2부터 보상 시작 ~ 11레벨 까지
            pass.SetLevel(11);
            pass.ClaimFreeRewards(PlayerAction.TEST);
            Assert.AreEqual(1000, (int)propertyManager.GetBalance(PropertyType.Get("Diamond")));
        }

        [Test]
        public void NewUserEventPackageTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            var propertyManager = Container.Resolve<PropertyManager>();
            var packageGroups = newUserEvent.PackageGroupList;

            Clock.AddDebugOffset(TimeSpan.FromDays(1));

            var packageGroup = packageGroups.Find(p => p.Day == 1);
            Assert.IsNotNull(packageGroup);

            var limitedProduct1 = packageGroup.ProductList[1];
            Container.Resolve<MockIAPManager>().IsSuccess = true;
            limitedProduct1.Purchase(PlayerAction.TEST);
            limitedProduct1.Purchase(PlayerAction.TEST);

            Assert.IsTrue(limitedProduct1.Remaining == 0);

            var diamond = propertyManager.GetBalance(PropertyType.Get("Diamond"));
            Assert.AreEqual(200, (int)diamond);
        }

        [Test]
        public void PackageRedDotTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            Assert.IsTrue(RedDot.GetNotified($"{_testRedDotPath}/Package/1/test1"));

            LimitedProduct freeProduct = newUserEvent.PackageGroupList.Find(p => p.Day == 1)
                .ProductList.Find(p => p.Product.Price is FreePrice);
            
            freeProduct.PurchaseAsync(PlayerAction.TEST).Forget();
            Assert.IsFalse(RedDot.GetNotified($"{_testRedDotPath}/Package/1/test1"));
        }


        [Test]
        public void MissionRedDotTest()
        {
            var newUserEvent = Container.Resolve<NewUserEvent>();
            Assert.False(RedDot.GetNotified($"{_testRedDotPath}/Mission/1/1"));
            
            var mission = newUserEvent.MissionGroupList.Find(m => m.Day == 1).MissionList[0];
            ((TestMission1)mission).ForceSetProgress(1);
            Assert.IsTrue(RedDot.GetNotified($"{_testRedDotPath}/Mission/1/1"));
            
            mission.Claim(PlayerAction.TEST);
            Assert.IsFalse(RedDot.GetNotified($"{_testRedDotPath}/Mission/1/1"));
        }
        

        public override void Teardown()
        {
            GameConfig.Clear();
            PropertyType.Clear();
            Clock.ResetDebugOffset();
            Container.Resolve<SaveDataManager>().Reset();
            base.Teardown();
        }
    }
}