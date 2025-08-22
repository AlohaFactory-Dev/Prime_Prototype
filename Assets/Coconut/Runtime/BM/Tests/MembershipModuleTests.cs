using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zenject;

namespace Aloha.Coconut.Tests
{
    public class MembershipModuleTests : ZenjectUnitTestFixture
    {
        private readonly PropertyTypeGroup _membershipPropertyTypeGroup = (PropertyTypeGroup)1;
        private readonly string _redDotPath = "test";
        
        public override void Setup()
        {
            base.Setup();

            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);

            PropertyType.Load();
            PropertyType.AddType(PropertyTypeGroup.Default, 100, "test");
            PropertyType.AddType(_membershipPropertyTypeGroup, 1, "membership_1");
            PropertyType.AddType(_membershipPropertyTypeGroup, 2, "membership_2");
            PropertyType.AddType(_membershipPropertyTypeGroup, 3, "membership_3");

            Mock<IMembershipDatabase> membershipDatabaseMock = new();
            membershipDatabaseMock.Setup(d => d.GetMembershipDataList())
                .Returns(new List<MembershipData>
                {
                    new(1, "test1", new List<Property> { new("Diamond", 100) },
                        new List<MembershipPrivilege>
                        {
                            new("test_privilege1", "test_privilege1")
                        }),
                    new(2, "test2", new List<Property> { new("Diamond", 100) },
                        new List<MembershipPrivilege>
                        {
                            new("test_privilege2", "test_privilege2")
                        }),
                    new(3, "test3", new List<Property> { new("Diamond", 100) },
                        new List<MembershipPrivilege>
                        {
                            new("test_privilege3", "test_privilege3")
                        })
                });
            membershipDatabaseMock.Setup(d => d.GetMembershipTypeGroup())
                .Returns(_membershipPropertyTypeGroup);
            membershipDatabaseMock.Setup(d => d.GetRedDotPath())
                .Returns(_redDotPath);
            Container.Bind<IMembershipDatabase>().FromInstance(membershipDatabaseMock.Object).AsSingle().NonLazy();
            
            Mock<IPackageRewardsManager> packageRewardsManagerMock = new();
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test1", false))
                .Returns(new List<Property>
                {
                    new("membership_1", 30),
                    new("test", 100)
                });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test2", false))
                .Returns(new List<Property>
                {
                    new("membership_2", 50),
                    new("test", 100)
                });
            packageRewardsManagerMock.Setup(m => m.GetPackageRewards("test3", false))
                .Returns(new List<Property>
                {
                    new("membership_3", 70),
                    new("test", 100)
                });

            SaveDataManager saveDataManager = new SaveDataManager();
            saveDataManager.LinkFileDataSaver(false);
            Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle().NonLazy();
            Container.Bind<PropertyManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MockIAPManager>().AsSingle().NonLazy();
            Container.BindInstance(packageRewardsManagerMock.Object).AsSingle();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            MembershipModuleInstaller.Install(Container);
        }

        [Test]
        public async Task PurchaseMembershipTest()
        {
            var membershipManager = Container.Resolve<MembershipManager>();
            var membershipList = membershipManager.MembershipList;

            var membership1 = membershipList.Find(membership => membership.Id == 1);
            var membership2 = membershipList.Find(membership => membership.Id == 2);
            var membership3 = membershipList.Find(membership => membership.Id == 3);

            // 비활성화 상태인지 확인
            Assert.IsFalse(membership1.IsActive);
            Assert.IsFalse(membership2.IsActive);
            Assert.IsFalse(membership3.IsActive);

            Container.Resolve<MockIAPManager>().IsSuccess = true;
            await membership1.Purchase();
            await membership2.Purchase();
            await membership3.Purchase();

            // 활성화 되었는지 확인
            Assert.IsTrue(membership1.IsActive);
            Assert.IsTrue(membership2.IsActive);
            Assert.IsTrue(membership3.IsActive);
            // 만료일이 제대로 설정되었는지 확인
            Assert.AreEqual(30, (membership1.EndDate.Date - Clock.GameDateNow.Date).Days + 1);
            Assert.AreEqual(50, (membership2.EndDate.Date - Clock.GameDateNow.Date).Days + 1);
            Assert.AreEqual(70, (membership3.EndDate.Date - Clock.GameDateNow.Date).Days + 1);

            var propertyManager = Container.Resolve<PropertyManager>();
            Assert.AreEqual(300, (int)propertyManager.GetBalance(PropertyType.Get("test")));

            membership1.ClaimDailyRewards(PlayerAction.TEST);
            membership2.ClaimDailyRewards(PlayerAction.TEST);
            membership3.ClaimDailyRewards(PlayerAction.TEST);

            Assert.AreEqual(300, (int)propertyManager.GetBalance(PropertyType.Get("Diamond")));

            Assert.IsTrue(membership1.IsClaimedToday);
            Assert.IsTrue(membership2.IsClaimedToday);
            Assert.IsTrue(membership3.IsClaimedToday);

            Clock.AddDebugOffset(TimeSpan.FromDays(1));

            Assert.IsFalse(membership1.IsClaimedToday);
            Assert.IsFalse(membership2.IsClaimedToday);
            Assert.IsFalse(membership3.IsClaimedToday);

            Clock.AddDebugOffset(TimeSpan.FromDays(200));

            Assert.IsFalse(membership1.IsActive);
            Assert.IsFalse(membership2.IsActive);
            Assert.IsFalse(membership3.IsActive);
        }

        [Test]
        public void ObtainMembershipTest()
        {
            var membershipManager = Container.Resolve<MembershipManager>();
            var membershipList = membershipManager.MembershipList;

            var membership3 = membershipList.Find(membership => membership.Id == 3);

            var propertyManager = Container.Resolve<PropertyManager>();
            propertyManager.Obtain(new Property("membership_3", 5), PlayerAction.TEST);

            Assert.AreEqual(4, (membership3.EndDate.Date - Clock.GameDateNow.Date).Days);
        }
        
        [Test]
        public void ObtainMembershipDoubleTest()
        {
            var membershipManager = Container.Resolve<MembershipManager>();
            var membershipList = membershipManager.MembershipList;

            var membership3 = membershipList.Find(membership => membership.Id == 3);

            var propertyManager = Container.Resolve<PropertyManager>();
            propertyManager.Obtain(new Property("membership_3", 5), PlayerAction.TEST);
            var firstEndDate = membership3.EndDate;
            
            propertyManager.Obtain(new Property("membership_3", 5), PlayerAction.TEST);
            var secondEndDate = membership3.EndDate;
            
            Assert.AreEqual(5, (secondEndDate.Date - firstEndDate.Date).Days);
        }

        [Test]
        public void DailyRewardsRedDotTest()
        {
            var membershipManager = Container.Resolve<MembershipManager>();
            var membershipList = membershipManager.MembershipList;

            var membership = membershipList.Find(membership => membership.Id == 1);
            
            Container.Resolve<MockIAPManager>().IsSuccess = true;
            membership.Purchase().Forget();
            
            Assert.IsTrue(RedDot.GetNotified($"{_redDotPath}/{membership.Id}/DailyRewards"));
            
            membership.ClaimDailyRewards(PlayerAction.TEST);
            
            Assert.IsFalse(RedDot.GetNotified($"{_redDotPath}/{membership.Id}/DailyRewards"));
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