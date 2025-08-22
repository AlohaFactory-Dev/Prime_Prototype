using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UniRx;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class RegularPassModuleTests : ZenjectUnitTestFixture
    {
        private readonly PropertyTypeGroup _regularPassPropertyTypeGroup = (PropertyTypeGroup)1;

        private readonly Dictionary<string, ReactiveProperty<int>> _progresses = new();
        private readonly string _redDotPath = "test";

        public override void Setup()
        {
            base.Setup();

            PropertyType.Load();
            PropertyType.AddType(_regularPassPropertyTypeGroup, 1, "LevelPass_1");
            PropertyType.AddType(_regularPassPropertyTypeGroup, 2, "LevelPass_2");
            PropertyType.AddType(_regularPassPropertyTypeGroup, 3, "LevelPass_3");

            Mock<IRegularPassDatabase> regularPassDatabaseMock = new();
            regularPassDatabaseMock.Setup(d => d.GetEveryRegularPassData())
                .Returns(new List<RegularPassData>
                {
                    new()
                    {
                        id = 1,
                        type = "level",
                        from = 1,
                        to = 10,
                        iapId = "test1",
                        nodeDatas = GetTestNodeDataList(10)
                    },
                    new()
                    {
                        id = 2,
                        type = "level",
                        from = 11,
                        to = 20,
                        iapId = "test2",
                        nodeDatas = GetTestNodeDataList(10)
                    },
                    new()
                    {
                        id = 3,
                        type = "level",
                        from = 21,
                        to = 30,
                        iapId = "test3",
                        nodeDatas = GetTestNodeDataList(10)
                    }
                });
            
            regularPassDatabaseMock.Setup(d => d.GetRegularPassTypeGroup())
                .Returns(_regularPassPropertyTypeGroup);
            regularPassDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_redDotPath);

            Mock<IRegularPassProgressProvider> regularPassProgressProviderMock = new();
            _progresses.Clear();
            _progresses["level"] = new ReactiveProperty<int>();
            regularPassProgressProviderMock.Setup(d => d.GetReactiveProgress("level"))
                .Returns(() => _progresses["level"]);
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> {new("LevelPass_1", 1)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> {new("LevelPass_2", 1)});
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property> {new("LevelPass_3", 1)});

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);

            Container.BindInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle().NonLazy();
            Container.BindInstance(regularPassDatabaseMock.Object).AsSingle().NonLazy();
            Container.BindInstance(regularPassProgressProviderMock.Object).AsSingle().NonLazy();
            RegularPassModuleInstaller.Install(Container);
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
                    reward2Alias = PropertyTypeAlias.Diamond.ToString(),
                    reward2Amount = 100
                });
            }

            return result;
        }

        [Test]
        public void InitializeTest()
        {
            var regularPassManager = Container.Resolve<RegularPassManager>();
            Assert.IsNotNull(regularPassManager.Passes);
            Assert.IsTrue(regularPassManager.Passes.Count > 0);
        }

        [Test]
        public void RegularPassActiveTest()
        {
            var regularPassManager = Container.Resolve<RegularPassManager>();
            RegularPass levelPass1 = null;
            RegularPass levelPass2 = null;
            RegularPass levelPass3 = null;

            foreach (var pass in regularPassManager.Passes)
            {
                if (pass.Type != "level") continue;
                if (pass.Id == 1)
                {
                    levelPass1 = pass;
                }
                else if (pass.Id == 2)
                {
                    levelPass2 = pass;
                }
                else if (pass.Id == 3)
                {
                    levelPass3 = pass;
                }
            }

            Assert.IsNotNull(levelPass1);
            Assert.IsNotNull(levelPass2);
            Assert.IsNotNull(levelPass3);

            Assert.IsTrue(levelPass1.IsActive);
            Assert.IsFalse(levelPass2.IsActive);
            Assert.IsFalse(levelPass3.IsActive);

            _progresses["level"].Value = 10;

            Assert.IsTrue(levelPass1.IsCompleted);
            Assert.IsTrue(levelPass2.IsActive);
            Assert.IsFalse(levelPass3.IsActive);

            _progresses["level"].Value = 20;
            
            Assert.IsTrue(levelPass2.IsCompleted);
            Assert.IsTrue(levelPass3.IsActive);
        }

        [Test]
        public void RegularPassRewardTest()
        {
            var regularPassManager = Container.Resolve<RegularPassManager>();
            var propertyManager = Container.Resolve<PropertyManager>();
            var iapManager = Container.Resolve<MockIAPManager>();

            RegularPass levelPass1 = null;
            RegularPass levelPass2 = null;
            RegularPass levelPass3 = null;

            foreach (var pass in regularPassManager.Passes)
            {
                if (pass.Type != "level") continue;
                if (pass.Id == 1)
                {
                    levelPass1 = pass;
                }
                else if (pass.Id == 2)
                {
                    levelPass2 = pass;
                }
                else if (pass.Id == 3)
                {
                    levelPass3 = pass;
                }
            }

            Assert.IsNotNull(levelPass1);
            Assert.IsNotNull(levelPass2);
            Assert.IsNotNull(levelPass3);
            
            _progresses["level"].Value = 30;

            iapManager.IsSuccess = true;
            iapManager.FakePurchase("test1");
            iapManager.FakePurchase("test2");
            iapManager.FakePurchase("test3");

            levelPass1.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            levelPass2.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            levelPass3.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            Assert.AreEqual(3000, (int)propertyManager.GetBalance(PropertyTypeAlias.Coin));
            Assert.AreEqual(3000, (int)propertyManager.GetBalance(PropertyTypeAlias.Diamond));
        }

        [Test]
        public void PassNodesRedDotTest()
        {
            var regularPassManager = Container.Resolve<RegularPassManager>();
            RegularPass levelPass1 = null;
            
            foreach (var pass in regularPassManager.Passes)
            {
                if (pass.Type != "level") continue;
                if (pass.Id == 1)
                {
                    levelPass1 = pass;
                }
            }
            
            Assert.IsNotNull(levelPass1);
            
            foreach (var node in levelPass1.Pass.Nodes)
            {
                Assert.IsFalse(RedDot.GetNotified(node.FreeRewardRedDotPath));
            }
            
            _progresses["level"].Value = 10;
            
            foreach (var node in levelPass1.Pass.Nodes)
            {
                Assert.IsTrue(RedDot.GetNotified(node.FreeRewardRedDotPath));
                Assert.IsFalse(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
            }
            
            levelPass1.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            foreach (var node in levelPass1.Pass.Nodes)
            {
                Assert.IsFalse(RedDot.GetNotified(node.FreeRewardRedDotPath));
            }
            
            var iapManager = Container.Resolve<MockIAPManager>();
            iapManager.IsSuccess = true;
            iapManager.FakePurchase("test1");
            
            foreach (var node in levelPass1.Pass.Nodes)
            {
                Assert.IsTrue(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
            }
            
            levelPass1.ClaimAll(PlayerAction.TEST, PlayerAction.TEST);
            
            foreach (var node in levelPass1.Pass.Nodes)
            {
                Assert.IsFalse(RedDot.GetNotified(node.AdvancedRewardRedDotPath));
            }
        }

        public override void Teardown()
        {
            PropertyType.Clear();
            Container.Resolve<SaveDataManager>().Reset();
            base.Teardown();
        }
    }
}