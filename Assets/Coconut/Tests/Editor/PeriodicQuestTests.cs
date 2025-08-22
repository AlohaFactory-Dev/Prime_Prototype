using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut.Missions;
using NUnit.Framework;
using Zenject;
using Assert = UnityEngine.Assertions.Assert;

namespace Aloha.Coconut.PeriodicQuests.Editor.Tests
{
    public class PeriodicQuestTests : ZenjectUnitTestFixture
    {
        private const int POINT_PER_TEST_MISSION = 20;
        private const int REWARD_INTERVAL = 20;

        private static PropertyType _propertyType;
        private static Property _reward;
        
        [OneTimeSetUp]
        public void LoadPropertyTypeAndReward()
        {
            PropertyType.Load();
            _propertyType = PropertyType.AddType(PropertyTypeGroup.Default, int.MinValue, "Test");
            _reward = new Property(_propertyType, 100);
        }
        
        public override void Setup()
        {
            base.Setup();
            
            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();
            saveDataManager.Reset();

            Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.Bind<MissionFactory>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicQuest>().FromSubContainerResolve()
                .ByMethod(c => InstallPeriodicQuest(c, ResetPeriod.Daily))
                .AsSingle().NonLazy();
        }

        private void InstallPeriodicQuest(DiContainer subContainer, ResetPeriod resetPeriod)
        {
            subContainer.Bind<ResetPeriod>().FromInstance(resetPeriod).AsSingle();
            subContainer.BindInterfacesTo<TestPeriodicQuestDataProvider>().AsSingle().NonLazy();
            subContainer.BindInterfacesAndSelfTo<PeriodicQuest>().AsSingle().NonLazy();
        }

        [Test]
        public void SimpleDataTests()
        {
            var periodicQuest = Container.Resolve<PeriodicQuest>();
            for (var i = 1; i < periodicQuest.Rewards.Count; i++)
            {
                Assert.IsTrue(periodicQuest.Rewards[i - 1].requiredPoint < periodicQuest.Rewards[i].requiredPoint,
                    $"Rewards are not sorted: {periodicQuest.Rewards[i - 1].requiredPoint} >= {periodicQuest.Rewards[i].requiredPoint}");
            }
            
            Assert.AreEqual(0, periodicQuest.CurrentPoint);
            Assert.IsFalse(periodicQuest.IsQuestOver);
        }

        [Test]
        public void ClaimRewardTest()
        {
            var periodicQuest = Container.Resolve<PeriodicQuest>();
            var missions = periodicQuest.PeriodicMissions;
            
            periodicQuest.Complete(missions[0]);
            periodicQuest.Complete(missions[1]);
            // 미션 두개 완료, 포인트는 40
            Assert.AreEqual(POINT_PER_TEST_MISSION * 2, periodicQuest.CurrentPoint);
            
            // 리워드 2개까지 획득 가능
            Assert.AreEqual(2, periodicQuest.Rewards.Count(r => r.RewardState == PeriodicQuestReward.State.Claimable));
            
            // 한번만 Claim 해도 전부 Claim
            periodicQuest.ClaimRewards(PlayerAction.TEST);
            Assert.AreEqual(0, periodicQuest.Rewards.Count(r => r.RewardState == PeriodicQuestReward.State.Claimable));
            
            // 보상 획득됨
            Assert.AreEqual(_reward.amount * 2, Container.Resolve<PropertyManager>().GetBalance(_propertyType));
        }

        [Test]
        public void ResetTest()
        {
            ClaimRewardTest();
            
            // 하루가 지나면 리셋
            Clock.AddDebugOffset(TimeSpan.FromDays(1));
            
            var periodicQuest = Container.Resolve<PeriodicQuest>();
            Assert.AreEqual(0, periodicQuest.CurrentPoint);
            Assert.AreEqual(0, periodicQuest.Rewards.Count(r => r.RewardState == PeriodicQuestReward.State.Claimable));
            Assert.AreEqual(0, periodicQuest.PeriodicMissions.Count(m => m.IsCompleted));
        }

        public override void Teardown()
        {
            base.Teardown();
        }

        private class TestPeriodicQuestDataProvider : IPeriodicQuestDataProvider
        {
            public List<PeriodicQuestReward> GetRewards()
            {
                var rewards = new List<PeriodicQuestReward>();
                for (var i = 0; i < 5; i++)
                {
                    rewards.Add(new PeriodicQuestReward
                    {
                        requiredPoint = i * REWARD_INTERVAL + REWARD_INTERVAL,
                        rewardTypeGroup = _reward.type.group,
                        rewardTypeId = _reward.type.id,
                        rewardAmount = _reward.amount
                    });
                }

                return rewards;
            }

            public List<PeriodicQuestMissionData> GetMissions()
            {
                var result = new List<PeriodicQuestMissionData>();
                for (var i = 0; i < 10; i++)
                {
                    result.Add(new PeriodicQuestMissionData
                    {
                        id = i,
                        type = TestMissionType.TYPE_1,
                        var = 0,
                        objective = 10,
                        point = POINT_PER_TEST_MISSION
                    });
                }

                return result;
            }
        }
    }
}