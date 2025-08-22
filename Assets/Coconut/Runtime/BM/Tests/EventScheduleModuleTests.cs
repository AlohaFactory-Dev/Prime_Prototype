using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UniRx;
using Zenject;
using Assert = UnityEngine.Assertions.Assert;

namespace Aloha.Coconut.Tests
{
    public class EventScheduleModuleTests : ZenjectUnitTestFixture
    {
        public override void Setup()
        {
            base.Setup();
            
            // 현재가 2024년 12월 1일이라고 가정
            DateTime now = new DateTime(2024, 12, 01, 5, 0, 0);
            Clock.DebugSetNow(now);
            
            Mock<IEventScheduleDatabase> eventScheduleDatabaseMock = new();

            eventScheduleDatabaseMock.Setup(d => d.GetEventScheduleDatas())
                .Returns(new List<EventScheduleData>
                {
                    new EventScheduleData(1, "test", 1, false, 20241101, 20241107),
                    new EventScheduleData(2, "test", 1, false, 20241201, 20241207),
                    new EventScheduleData(3, "test", 1, false, 20250101, 20250107),
                    new EventScheduleData(4, "test", 1, true, 1, 3),
                    new EventScheduleData(5, "test", 1, true, 3, 5),
                    new EventScheduleData(6, "test", 1, true, 6, 6),
                });
            
            Container.Bind<SaveDataManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PeriodicResetHandler>().AsSingle().NonLazy();
            Container.Bind<SimpleValues>().AsSingle().NonLazy();
            Container.Bind<IEventScheduleDatabase>().FromInstance(eventScheduleDatabaseMock.Object).AsSingle();
            EventScheduleModuleInstaller.Install(Container);
            
            Container.Resolve<SaveDataManager>().LinkFileDataSaver(false);
        }

        [Test]
        public void ActivatedEventSchedulesTest()
        {
            EventScheduleManager eventScheduleManager = Container.Resolve<EventScheduleManager>();
            List<EventSchedule> activeEventSchedules = eventScheduleManager.GetActiveEventSchedules("test");
            
            Assert.AreEqual(activeEventSchedules.Count, 2);
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 2));
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 4));
        }

        [Test]
        public void ActivatedEventSchedulesTestAfterThreeDays()
        {
            EventScheduleManager eventScheduleManager = Container.Resolve<EventScheduleManager>();
            bool isEvent4Ended = false;
            bool isEvent5Started = false;
            
            eventScheduleManager.OnEventEnded
                .Where(ev => ev.Id == 4).First()
                .Subscribe(_ => isEvent4Ended = true);
            
            eventScheduleManager.OnEventStarted
                .Where(ev => ev.Id == 5).First()
                .Subscribe(_ => isEvent5Started = true);
            
            // 1일차는 위 테스트에서 수행함
            // 2일차
            Clock.AddDebugOffset(TimeSpan.FromDays(1));
            List<EventSchedule> activeEventSchedules = eventScheduleManager.GetActiveEventSchedules("test");
            Assert.AreEqual(activeEventSchedules.Count, 2);
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 2));
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 4));
            
            // 3일차
            Clock.AddDebugOffset(TimeSpan.FromDays(1));
            activeEventSchedules = eventScheduleManager.GetActiveEventSchedules("test");
            Assert.AreEqual(activeEventSchedules.Count, 3);
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 2));
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 4));
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 5));
            Assert.IsTrue(isEvent5Started);
            
            // 4일차
            Clock.AddDebugOffset(TimeSpan.FromDays(1));
            activeEventSchedules = eventScheduleManager.GetActiveEventSchedules("test");
            Assert.AreEqual(activeEventSchedules.Count, 2);
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 2));
            Assert.IsTrue(activeEventSchedules.Exists(e => e.Id == 5));
            Assert.IsTrue(isEvent4Ended);
        }

        public override void Teardown()
        {
            Container.Resolve<SaveDataManager>().Reset();
            base.Teardown();
            Clock.ResetDebugOffset();
        }
    }
}
