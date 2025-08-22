using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class BeginnerPackagesModuleTests : ZenjectUnitTestFixture
    {
        private readonly PropertyTypeGroup _beginnerPackagePropertyTypeGroup = (PropertyTypeGroup)1;

        private string _testRedDotPath = "test";

        public override void Setup()
        {
            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 100, "Test");
            PropertyType.AddType(_beginnerPackagePropertyTypeGroup, 1, "BeginnerPackage1");
            PropertyType.AddType(_beginnerPackagePropertyTypeGroup, 2, "BeginnerPackage2");
            PropertyType.AddType(_beginnerPackagePropertyTypeGroup, 3, "BeginnerPackage3");

            base.Setup();

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);

            Mock<IBeginnerPackageDatabase> beginnerPackageDatabaseMock = new();
            beginnerPackageDatabaseMock.Setup(d => d.GetBeginnerPackageDataList())
                .Returns(GetTestPackageDataList());
            beginnerPackageDatabaseMock.Setup(d => d.GetBeginnerPackageTypeGroup())
                .Returns(_beginnerPackagePropertyTypeGroup);
            beginnerPackageDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_testRedDotPath);

            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property> { new("BeginnerPackage1", 1) });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property> { new("BeginnerPackage2", 1) });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property> { new("BeginnerPackage3", 1) });

            var saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();

            Container.BindInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.BindInstance(beginnerPackageDatabaseMock.Object).AsSingle();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle();
            BeginnerPackagesModuleInstaller.Install(Container);
        }

        private List<BeginnerPackageData> GetTestPackageDataList()
        {
            return new List<BeginnerPackageData>
            {
                new(1, "test1", GetTestComponentDataList()),
                new(2, "test2", GetTestComponentDataList()),
                new(3, "test3", GetTestComponentDataList())
            };
        }

        private List<BeginnerPackageComponentData> GetTestComponentDataList()
        {
            return new List<BeginnerPackageComponentData>
            {
                new(1, new List<Property>
                {
                    new("Test", 100),
                    new("Test", 200),
                    new("Test", 300)
                }),
                new(2, new List<Property>
                {
                    new("Test", 400),
                    new("Test", 500),
                    new("Test", 600)
                }),
                new(3, new List<Property>
                {
                    new("Test", 700),
                    new("Test", 800),
                    new("Test", 900)
                })
            };
        }

        [Test]
        public void InitializeTest()
        {
            BeginnerPackagesManager beginnerPackagesManager = Container.Resolve<BeginnerPackagesManager>();
            Assert.IsTrue(beginnerPackagesManager.BeginnerPackageList.Count == 3);

            foreach (var package in beginnerPackagesManager.BeginnerPackageList)
            {
                Assert.IsFalse(package.IsPurchased);
                Assert.IsTrue(package.Components.Count == 3);
                Assert.IsTrue(package.Components.TrueForAll(c => !c.IsClaimable));
            }
        }

        [Test]
        public async Task PurchaseAndDayPassTest()
        {
            BeginnerPackagesManager beginnerPackagesManager = Container.Resolve<BeginnerPackagesManager>();
            var packages = beginnerPackagesManager.BeginnerPackageList;

            foreach (var package in packages)
            {
                Container.Resolve<MockIAPManager>().IsSuccess = true;
                await package.Purchase();

                Clock.AddDebugOffset(TimeSpan.FromDays(1));

                Assert.IsTrue(package.IsPurchased);
                // 1일차, 2일차는 획득 가능해야 함
                Assert.IsTrue(package.Components.Find(c => c.Day == 1)?.IsClaimable);
                Assert.IsTrue(package.Components.Find(c => c.Day == 2)?.IsClaimable);
                Assert.IsFalse(package.Components.Find(c => c.Day == 3)?.IsClaimable);

                // 2일차 보상을 받음
                var day2Component = package.Components.Find(c => c.Day == 2);
                var obtainedRewards = day2Component?.Claim(PlayerAction.TEST);
                Assert.IsNotNull(obtainedRewards);
                Assert.IsTrue(day2Component.IsClaimed);
            }

            PropertyManager propertyManager = Container.Resolve<PropertyManager>();
            Assert.AreEqual(4500, (int)propertyManager.GetBalance(PropertyType.Get("Test")));
        }

        [Test]
        public async Task IsActiveTest()
        {
            BeginnerPackagesManager beginnerPackagesManager = Container.Resolve<BeginnerPackagesManager>();
            var packages = beginnerPackagesManager.BeginnerPackageList;

            Assert.IsTrue(beginnerPackagesManager.IsActive.Value);

            Container.Resolve<MockIAPManager>().IsSuccess = true;
            foreach (var package in packages)
            {
                await package.Purchase();
            }

            Clock.AddDebugOffset(TimeSpan.FromDays(2));

            foreach (var package in packages)
            {
                foreach (var component in package.Components)
                {
                    component.Claim(PlayerAction.TEST);
                }
            }

            Assert.IsFalse(beginnerPackagesManager.IsActive.Value);
        }

        [Test]
        public void DailyRewardsRedDotTest()
        {
            var beginnerPackagesManager = Container.Resolve<BeginnerPackagesManager>();
            var database = Container.Resolve<IBeginnerPackageDatabase>();

            var beginnerPackage1 = beginnerPackagesManager.BeginnerPackageList[0];
            var redDotPath = $"{database.GetRedDotPath()}/Package/1/1";
            Assert.IsFalse(RedDot.GetNotified(redDotPath));

            Container.Resolve<MockIAPManager>().IsSuccess = true;
            beginnerPackage1.Purchase().Forget();

            Assert.IsTrue(RedDot.GetNotified(redDotPath));

            var day1Component = beginnerPackage1.Components.Find(c => c.Day == 1);
            day1Component.Claim(PlayerAction.TEST);

            Assert.IsFalse(RedDot.GetNotified(redDotPath));
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