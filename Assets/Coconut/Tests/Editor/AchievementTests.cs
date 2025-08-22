using System.Collections.Generic;
using System.Numerics;
using Aloha.Coconut.Achievements;
using Aloha.Coconut.Missions;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.PeriodicQuests.Editor.Tests
{
    public class AchievementTests : ZenjectUnitTestFixture
    {
        private readonly int _testPropertyId = int.MinValue;
        private BigInteger _testPropertyAmount = 100;
        private PropertyType TestPropertyType => PropertyType.Get(PropertyTypeGroup.Default, _testPropertyId);
        
        public override void Setup()
        {
            base.Setup();
            
            PlayerAction.Load();
            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, _testPropertyId, "Test");

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();
            saveDataManager.Reset();
            Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle();
            
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<MissionFactory>().AsSingle().NonLazy();
            
            Mock<IAchievementDataProvider> mockDataProvider = new Mock<IAchievementDataProvider>();
            mockDataProvider.Setup(x => x.GetAchievementDatas())
                .Returns(new List<AchievementData>
                {
                    CreateAchievementData(1, 0),
                    CreateAchievementData(1, 1),
                    CreateAchievementData(1, 2),
                    CreateAchievementData(2, 0),
                    CreateAchievementData(2, 1),
                    CreateAchievementData(2, 2),
                    CreateAchievementData(3, 0),
                    CreateAchievementData(3, 1),
                    CreateAchievementData(3, 2),
                });

            Container.Bind<IAchievementDataProvider>().FromInstance(mockDataProvider.Object).AsSingle().NonLazy();
            Container.Bind<AchievementManager>().AsSingle().NonLazy();
        }

        [Test]
        public void TestGroupComplete()
        {
            var achievementManager = Container.Resolve<AchievementManager>();
            var propertyManager = Container.Resolve<PropertyManager>();
            var group = achievementManager.GetGroups().Find(g => g.GroupId == 1);
            
            Assert.IsNotNull(group.CurrentMission);
            
            var mission1 = group.CurrentMission;
            group.Complete(PlayerAction.TEST);
            Assert.AreEqual(_testPropertyAmount, propertyManager.GetBalance(TestPropertyType));
            
            var mission2 = group.CurrentMission;
            Assert.AreNotEqual(mission1, mission2);
            group.Complete(PlayerAction.TEST);
            Assert.AreEqual(_testPropertyAmount * 2, propertyManager.GetBalance(TestPropertyType));
            
            group.Complete(PlayerAction.TEST);
            Assert.AreEqual(_testPropertyAmount * 3, propertyManager.GetBalance(TestPropertyType));
            
            // 그룹 내의 모든 미션을 완료한 후에는 더이상 Complete이 불가능하고, 마지막 미션이 RewardsClaimed 상태를 유지한다
            Assert.IsTrue(group.CurrentMission.IsRewardsClaimed);
            group.Complete(PlayerAction.TEST);
            Assert.AreEqual(_testPropertyAmount * 3, propertyManager.GetBalance(TestPropertyType));
            Assert.IsTrue(group.CurrentMission.IsRewardsClaimed);
        }

        [Test]
        public void TestSave()
        {
            var achievementManager = Container.Resolve<AchievementManager>();
            var group = achievementManager.GetGroups().Find(g => g.GroupId == 1);
            group.Complete(PlayerAction.TEST);
            var missionId = group.CurrentMission.Id;
            Container.Resolve<SaveDataManager>().Save();

            var subContainer = Container.CreateSubContainer();
            var newSaveDataManager = new SaveDataManager();
            newSaveDataManager.LinkFileDataSaver();
            subContainer.Bind<SaveDataManager>().FromInstance(newSaveDataManager).AsSingle();
            subContainer.Bind<AchievementManager>().AsSingle().NonLazy();
            
            var newAchievementManager = subContainer.Resolve<AchievementManager>();
            var newGroup = newAchievementManager.GetGroups().Find(g => g.GroupId == 1);
            Assert.AreEqual(missionId, newGroup.CurrentMission.Id);
        }

        public override void Teardown()
        {
            EventBus.InitializeOnEnterPlayMode();
            base.Teardown();
        }

        private AchievementData CreateAchievementData(int group, int order)
        {
            return new AchievementData
            {
                group = group,
                order = order,
                type = TestMissionType.TYPE_1,
                var = 0,
                objective = 100,
                rewardTypeAlias = "Test",
                rewardAmount = _testPropertyAmount
            };
        }
    }
}