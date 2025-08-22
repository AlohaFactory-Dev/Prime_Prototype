using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class SpecialPackagesModuleTests : ZenjectUnitTestFixture 
    {
        public override void Setup()
        {
            base.Setup();
            
            SaveDataManager saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver();
            Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle();
            Container.Bind<LimitedProduct.Factory>().AsSingle();
            Container.Bind<SpecialPackageManager>().AsSingle();

            Mock<ISpecialPackageDatabase> specialPackageDatabaseMock = new();
            specialPackageDatabaseMock.Setup(d => d.GetSpecialPackageDatas())
                .Returns(new List<SpecialPackageData>
                {
                    new SpecialPackageData { id = 1, iapProductId = "test1", limit = 2, condition = 0 },
                    new SpecialPackageData { id = 2, iapProductId = "test2", limit = 1, condition = 0 },
                    new SpecialPackageData { id = 3, iapProductId = "test3", limit = 1, condition = 2 }
                });
            Container.Bind<ISpecialPackageDatabase>().FromInstance(specialPackageDatabaseMock.Object).AsSingle();

            Container.BindInterfacesAndSelfTo<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MockIAPManager>().AsSingle();
            Container.BindInterfacesTo<MockPackageRewardsManager>().AsSingle();

            Container.Resolve<PropertyManager>();
        }

        [Test]
        public void SoldOutTest()
        {
            SpecialPackageManager specialPackageManager = Container.Resolve<SpecialPackageManager>();

            Purchase(1);
            Assert.IsTrue(!specialPackageManager.SpecialPackages[0].LimitedProduct.IsSoldOut);
            
            Purchase(1);
            Assert.IsTrue(specialPackageManager.SpecialPackages[0].LimitedProduct.IsSoldOut);
        }

        private void Purchase(int id)
        {
            SpecialPackageManager specialPackageManager = Container.Resolve<SpecialPackageManager>();
            Task.Run(() => specialPackageManager.SpecialPackages.FirstOrDefault(p => p.Id == id).PurchaseAsync(PlayerAction.TEST))
                .Wait();
        }

        [Test]
        public void ConditionalPackageTest()
        {
            SpecialPackageManager specialPackageManager = Container.Resolve<SpecialPackageManager>();
            Purchase(2);
            
            Assert.IsTrue(specialPackageManager.SpecialPackages.Any(p => p.Id == 3));

            // 세이브파일을 불러와서 새로 생성한 후에도 잘 활성화되는지 확인
            Container.Resolve<SaveDataManager>().Save();
            Setup();
            specialPackageManager = Container.Resolve<SpecialPackageManager>();
            Assert.IsTrue(specialPackageManager.SpecialPackages.Any(p => p.Id == 3));
        }
        
        public override void Teardown()
        {
            Container.Resolve<SaveDataManager>().DeleteAll();
            base.Teardown();
        }
    }
}
