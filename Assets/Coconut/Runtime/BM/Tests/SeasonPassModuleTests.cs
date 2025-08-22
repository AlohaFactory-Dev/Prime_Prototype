using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UniRx;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class SeasonPassModuleTests : ZenjectUnitTestFixture
    {
        private readonly PropertyTypeGroup _seasonPassPropertyTypeGroup = (PropertyTypeGroup)1;
        private readonly string _redDotPath = "test";

        public override void Setup()
        {
            base.Setup();

            PropertyType.Load();
            PropertyType.AddType(_seasonPassPropertyTypeGroup, 1, "SeasonPass_Advance");
            PropertyType.AddType(_seasonPassPropertyTypeGroup, 2, "SeasonPass_Premium");

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);

            Mock<ISeasonPassDatabase> seasonPassDatabaseMock = new();
            seasonPassDatabaseMock.Setup(d => d.GetCurrentSeasonPassData(Clock.GameDateNow))
                .Returns(new SeasonPassData
                {
                    id = 1,
                    expPerLevel = 100,
                    endDate = 20241231,
                    nodeDatas = GetTestNodeDataList(10)
                });
            
            seasonPassDatabaseMock.Setup(d => d.GetCurrentSeasonPassData(new GameDate(2025, 1, 1)))
                .Returns(new SeasonPassData
                {
                    id = 2,
                    expPerLevel = 100,
                    endDate = 20250131,
                    nodeDatas = GetTestNodeDataList(10)
                });
            
            seasonPassDatabaseMock.Setup(d => d.GetSeasonPassTypeGroup())
                .Returns(_seasonPassPropertyTypeGroup);
            seasonPassDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_redDotPath);

            Mock<ISeasonPassExpAdder> seasonPassExpAdderMock = new();
            seasonPassExpAdderMock.Setup(d => d.OnGetSeasonPassExp)
                .Returns(new Subject<int>());
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> {new("SeasonPass_Advance", 1)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> {new("SeasonPass_Premium", 1)});

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);

            Container.BindInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle().NonLazy();
            Container.BindInstance(seasonPassDatabaseMock.Object).AsSingle().NonLazy();
            Container.BindInstance(seasonPassExpAdderMock.Object).AsSingle().NonLazy();
            SeasonPassModuleInstaller.Install(Container);
        }

        private List<PassNodeData> GetTestNodeDataList(int nodeCount)
        {
            List<PassNodeData> result = new List<PassNodeData>();
            for (int i = 0; i < nodeCount; i++)
            {
                result.Add(new PassNodeData
                {
                    passLevel = i + 1,
                    reward1Alias = PropertyTypeAlias.Coin.ToString(),
                    reward1Amount = 100,
                    reward2Alias = PropertyTypeAlias.Stamina.ToString(),
                    reward2Amount = 100,
                    reward3Alias = PropertyTypeAlias.Diamond.ToString(),
                    reward3Amount = 100
                });
            }

            return result;
        }

        [Test]
        public void InitializeTest()
        {
            var seasonPass = Container.Resolve<SeasonPass>();
            Assert.IsNotNull(seasonPass.CurrentPass);
            Assert.IsTrue(seasonPass.CurrentPass.Nodes.Count > 0);
            Assert.IsFalse(seasonPass.CurrentPass.IsAdvancedActivated);
        }

        [Test]
        public void SeasonPassRewardTest()
        {
            var seasonPass = Container.Resolve<SeasonPass>();
            var expAdder = Container.Resolve<ISeasonPassExpAdder>();

            Assert.AreEqual(1, seasonPass.CurrentPass.CurrentLevel);

            expAdder.OnGetSeasonPassExp.OnNext(500);
            Assert.AreEqual(6, seasonPass.CurrentPass.CurrentLevel);

            seasonPass.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);

            var propertyManager = Container.Resolve<PropertyManager>();
            var coin = (int)propertyManager.GetBalance(PropertyTypeAlias.Coin);
            var stamina = (int)propertyManager.GetBalance(PropertyTypeAlias.Stamina);
            var diamond = (int)propertyManager.GetBalance(PropertyTypeAlias.Diamond);
            Assert.AreEqual(600, coin);
            Assert.AreEqual(0, stamina);
            Assert.AreEqual(0, diamond);

            var iapManager = Container.Resolve<MockIAPManager>();
            iapManager.IsSuccess = true;
            iapManager.FakePurchase("test1");

            Assert.IsTrue(seasonPass.CurrentPass.IsAdvancedActivated);
            seasonPass.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);

            coin = (int)propertyManager.GetBalance(PropertyTypeAlias.Coin);
            stamina = (int)propertyManager.GetBalance(PropertyTypeAlias.Stamina);
            diamond = (int)propertyManager.GetBalance(PropertyTypeAlias.Diamond);

            Assert.AreEqual(600, coin);
            Assert.AreEqual(600, stamina);
            Assert.AreEqual(0, diamond);

            iapManager.FakePurchase("test2");

            seasonPass.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            diamond = (int)propertyManager.GetBalance(PropertyTypeAlias.Diamond);

            Assert.AreEqual(600, diamond);
        }

        [Test]
        public void SeasonPassDatePassTest()
        {
            var seasonPass = Container.Resolve<SeasonPass>();
            var expAdder = Container.Resolve<ISeasonPassExpAdder>();

            expAdder.OnGetSeasonPassExp.OnNext(1000);
            Assert.AreEqual(10, seasonPass.CurrentPass.CurrentLevel);

            var iapManager = Container.Resolve<MockIAPManager>();
            iapManager.IsSuccess = true;
            iapManager.FakePurchase("test1");
            iapManager.FakePurchase("test2");
            
            seasonPass.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            Clock.AddDebugOffset(TimeSpan.FromDays(31));
            
            Assert.AreEqual(1, seasonPass.CurrentPass.CurrentLevel);
            Assert.IsFalse(seasonPass.CurrentPass.IsAdvancedActivated);
            Assert.IsFalse(seasonPass.CurrentPass.IsPremiumActivated);
            Assert.IsTrue(seasonPass.EndDate == new GameDate(2025, 01, 31));
        }

        [Test]
        public void SeasonPassRedDotTest()
        {
            var seasonPass = Container.Resolve<SeasonPass>();
            var expAdder = Container.Resolve<ISeasonPassExpAdder>();
            
            var iapManager = Container.Resolve<MockIAPManager>();
            iapManager.IsSuccess = true;
            iapManager.FakePurchase("test1");
            iapManager.FakePurchase("test2");
            
            expAdder.OnGetSeasonPassExp.OnNext(300);

            foreach (var node in seasonPass.CurrentPass.Nodes)
            {
                if (node.PassLevel < 5)
                {
                    Assert.IsTrue(RedDot.GetNotified(node.FreeRewardRedDotPath));
                    Assert.IsTrue(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
                    Assert.IsTrue(RedDot.GetNotified(node.PremiumRewardRedDotPath));
                }
                else
                {
                    Assert.IsFalse(RedDot.GetNotified(node.FreeRewardRedDotPath));
                    Assert.IsFalse(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
                    Assert.IsFalse(RedDot.GetNotified(node.PremiumRewardRedDotPath));
                }
            }
            
            seasonPass.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            foreach (var node in seasonPass.CurrentPass.Nodes)
            {
                Assert.IsFalse(RedDot.GetNotified(node.FreeRewardRedDotPath));
                Assert.IsFalse(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
                Assert.IsFalse(RedDot.GetNotified(node.PremiumRewardRedDotPath));
            }
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