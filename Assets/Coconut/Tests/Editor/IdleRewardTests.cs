using System;
using System.Collections.Generic;
using Aloha.Coconut.IdleRewards;
using Moq;
using NUnit.Framework;
using Zenject;
using Assert = UnityEngine.Assertions.Assert;

namespace Aloha.Coconut.Tests.Editor
{
    public class IdleRewardTests : ZenjectUnitTestFixture
    {
        private IdleRewardGenerator.Factory Factory => Container.Resolve<IdleRewardGenerator.Factory>();
        private Mock<IIdleRewardDataProvider> _idleRewardDataProviderMock;
        private IdleRewardGenerator _idleRewardGenerator;
        private IIdleRewardDataProvider IdleRewardDataProvider => _idleRewardDataProviderMock.Object;

        private PropertyType _testProp1;
        private PropertyType _testProp2;
        private PropertyType _testProp3;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PropertyType.Load();
        
            PropertyType.AddType(PropertyTypeGroup.Default, 100, "t1");
            PropertyType.AddType(PropertyTypeGroup.Default, 101, "t2");
            PropertyType.AddType(PropertyTypeGroup.Default, 102, "t3");
            _testProp1 = PropertyType.Get(PropertyTypeGroup.Default, 100);
            _testProp2 = PropertyType.Get(PropertyTypeGroup.Default, 101);
            _testProp3 = PropertyType.Get(PropertyTypeGroup.Default, 102);
        
            PlayerAction.Load();
            EventBus.InitializeOnEnterPlayMode();
        }

        public override void Setup()
        {
            base.Setup();

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();

            var rvAdapter = new Mock<IRVAdapter>().Object;

            Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<IRVAdapter>().FromInstance(rvAdapter).AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.Bind<IdleRewardGenerator.Factory>().AsSingle().NonLazy();

            _idleRewardDataProviderMock = new Mock<IIdleRewardDataProvider>();
            _idleRewardDataProviderMock.Setup(i => i.Id).Returns("TestRewardDataProvider");
            _idleRewardDataProviderMock.Setup(i => i.RedDotPath).Returns("redDot");
            _idleRewardDataProviderMock.Setup(i => i.IdleHoursMax).Returns(24);
            _idleRewardDataProviderMock.Setup(i => i.RewardGenerationSeconds).Returns(1);
            _idleRewardDataProviderMock.Setup(i => i.QuickEarningCost).Returns(new Property(_testProp2, 100));
            _idleRewardDataProviderMock.Setup(i => i.QuickEarningHours).Returns(2);
            _idleRewardDataProviderMock.Setup(i => i.QuickEarningPerDay).Returns(3);
            _idleRewardDataProviderMock.Setup(i => i.RVQuickEarningsPerDay).Returns(2);
            _idleRewardDataProviderMock.Setup(i => i.QuickEarningRVPlacementId).Returns(1);
            _idleRewardDataProviderMock.Setup(i => i.QuickEarningRVPlacementName).Returns("testPlacement");
        
            _idleRewardGenerator = Factory.Create(IdleRewardDataProvider, new IdleRewardGenerator.SaveData());
            _idleRewardGenerator.Run();
        }

        [Test]
        public void SimpleOneTimeGeneration()
        {
            _idleRewardDataProviderMock.Setup(i => i.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp1, 100),
                    new (_testProp2, 100),
                });
        
            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Count == 0);

            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Count == 2);
            Assert.IsTrue(currentRewards.Find(p => p.type == _testProp1).amount == 100);
            Assert.IsTrue(currentRewards.Find(p => p.type == _testProp2).amount == 100);
        }

        [Test]
        public void SimpleOneTimeGenerationAndClaim()
        {
            SimpleOneTimeGeneration();
            _idleRewardGenerator.ClaimRewards(PlayerAction.TEST);

            var propertyManager = Container.Resolve<PropertyManager>();
            Assert.AreEqual(100, propertyManager.GetBalance(_testProp1));
            Assert.AreEqual(100, propertyManager.GetBalance(_testProp2));
        }

        [Test]
        public void TwoTimeGenerationSeparated()
        {
            _idleRewardDataProviderMock.Setup(i => i.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp1, 100),
                    new (_testProp2, 100),
                });
        
            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Count == 0);

            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));

            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Count == 2);
            Assert.AreEqual(200, currentRewards.Find(p => p.type == _testProp1).amount);
            Assert.AreEqual(200, currentRewards.Find(p => p.type == _testProp2).amount);
        }

        [Test]
        public void OverMaxTimeGeneration()
        {
            _idleRewardDataProviderMock.Setup(i => i.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp1, 100),
                    new (_testProp2, 100),
                });
        
            Clock.AddDebugOffset(TimeSpan.FromHours(_idleRewardGenerator.IdleHoursMax));
            Clock.AddDebugOffset(TimeSpan.FromHours(_idleRewardGenerator.IdleHoursMax));

            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.AreEqual(100 * _idleRewardGenerator.RewardGenerationMax,
                currentRewards.Find(p => p.type == _testProp1).amount);
            Assert.AreEqual(100 * _idleRewardGenerator.RewardGenerationMax,
                currentRewards.Find(p => p.type == _testProp2).amount);
        }

        [Test]
        public void OverMaxTimeGenerationAndClaim()
        {
            OverMaxTimeGeneration();
            _idleRewardGenerator.ClaimRewards(PlayerAction.TEST);

            var propertyManager = Container.Resolve<PropertyManager>();
            ;
            Assert.AreEqual(100 * _idleRewardGenerator.RewardGenerationMax, propertyManager.GetBalance(_testProp1));
            Assert.AreEqual(100 * _idleRewardGenerator.RewardGenerationMax, propertyManager.GetBalance(_testProp2));

            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Count == 0);
        }

        [Test]
        public void ClaimAndTimeLeft()
        {
            _idleRewardDataProviderMock.Setup(i => i.RewardGenerationSeconds).Returns(10);
            _idleRewardDataProviderMock.Setup(i => i.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp1, 100),
                    new (_testProp2, 100),
                });
        
            var overtime = 7;
            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds + overtime)); // 17초 지난 상태에서 claim
            _idleRewardGenerator.ClaimRewards(PlayerAction.TEST);
        
            // 7초 정도 남아있어야 함        
            Assert.AreApproximatelyEqual(overtime, (float)_idleRewardGenerator.IdleTime.TotalSeconds, 1);

            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds - overtime)); // 3초 후
            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.AreEqual(100, currentRewards.Find(p => p.type == _testProp1).amount);
            Assert.AreEqual(100, currentRewards.Find(p => p.type == _testProp2).amount);
        }

        [Test]
        public void FloatAcquisition()
        {
            _idleRewardDataProviderMock.Setup(p => p.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp3, 0.5f)
                });

            // amount 0.5로 입력되어있으면, 두번에 한 번씩만 생성되어야 함
            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            var currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(!currentRewards.Exists(p => p.type == _testProp3));

            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(currentRewards.Exists(p => p.type == _testProp3));

            // 세번째에는 1.5만큼 누적되어 있으므로, 1만 리턴
            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.AreEqual(1, currentRewards.Find(p => p.type == _testProp3).amount);
            _idleRewardGenerator.ClaimRewards(PlayerAction.TEST);

            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.IsTrue(!currentRewards.Exists(p => p.type == _testProp3));

            Clock.AddDebugOffset(TimeSpan.FromSeconds(IdleRewardDataProvider.RewardGenerationSeconds) + TimeSpan.FromMilliseconds(10));
            currentRewards = _idleRewardGenerator.GetCurrentRewards();
            Assert.AreEqual(1, currentRewards.Find(p => p.type == _testProp3).amount);
        }

        [Test]
        public void QuickEarnings()
        {
            _idleRewardDataProviderMock.Setup(i => i.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp1, 100),
                    new (_testProp2, 100),
                });
        
            _idleRewardGenerator.ClaimQuickEarningRewards(PlayerAction.TEST);

            var propertyManager = Container.Resolve<PropertyManager>();
            Assert.AreEqual(100 * _idleRewardGenerator.QuickEarningGeneration, propertyManager.GetBalance(_testProp1));
            Assert.AreEqual(100 * _idleRewardGenerator.QuickEarningGeneration, propertyManager.GetBalance(_testProp2));
        }

        [Test]
        public void QuickEarningsRefilled()
        {
            QuickEarnings();
            Clock.AddDebugOffset(TimeSpan.FromDays(1) + TimeSpan.FromMilliseconds(10)); // 하루 지남

            Assert.AreEqual(_idleRewardGenerator.QuickEarningPerDay, _idleRewardGenerator.QuickEarningLeft);
        }

        [Test]
        public void QuickEarningsFloatAcquisition()
        {
            _idleRewardDataProviderMock.Setup(i => i.RewardGenerationSeconds).Returns(600);
            _idleRewardDataProviderMock.Setup(p => p.GetIdleRewardsPerGeneration())
                .Returns(new List<IdleReward>
                {
                    new (_testProp3, 0.5f)
                });

            var quickEarningsRewards = _idleRewardGenerator.GetQuickEarningRewards();
            var quickEarningGeneration = _idleRewardGenerator.QuickEarningGeneration;
            Assert.AreEqual((int)(0.5f * quickEarningGeneration), quickEarningsRewards.Find(p => p.type == _testProp3).amount);

            _idleRewardGenerator.ClaimQuickEarningRewards(PlayerAction.TEST);
            var propertyManager = Container.Resolve<PropertyManager>();
            Assert.AreEqual((int)(0.5f * quickEarningGeneration), (int)propertyManager.GetBalance(_testProp3));
        }

        public override void Teardown()
        {
            Clock.ResetDebugOffset();
            Container.Resolve<SaveDataManager>().Reset();
            base.Teardown();
        }
    }
}