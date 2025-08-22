using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Aloha.Coconut.Attendances;
using NUnit.Framework;
using UnityEngine;
using Zenject;

public class AttendanceTests : ZenjectUnitTestFixture
{
    private PropertyManager PropertyManager => Container.Resolve<PropertyManager>();
    
    public override void Setup()
    {
        base.Setup();
        PropertyType.AddType(PropertyTypeGroup.Default, int.MaxValue, "test");
        var saveDataManager = new SaveDataManager();
        saveDataManager.LinkFileDataSaver(false);
        
        Container.Bind<PropertyManager>().AsSingle();
        Container.BindInterfacesTo<DefaultPropertyHandler>().AsSingle();
        Container.Bind<SaveDataManager>().FromInstance(saveDataManager).AsSingle();
        Container.Bind<PeriodicResetHandler>().AsSingle();
        Container.Bind<SimpleValues>().AsSingle();
    }

    private Attendance Create(string id, List<AttendanceNode> nodes, DiContainer container)
    {
        return new Attendance(id, nodes, container.Resolve<SaveDataManager>().Get<Attendance.SaveData>(id),
            container.Resolve<PeriodicResetHandler>(), container.Resolve<PropertyManager>());
    }

    private List<AttendanceNode> GetStandardNodes()
    {
        return new List<AttendanceNode>()
        {
            new (1, new List<Property>() {new ("test", 1)}),
            new (2, new List<Property>() {new ("test", 1)}),
            new (3, new List<Property>() {new ("test", 1)}),
            new (4, new List<Property>() {new ("test", 1)}),
            new (5, new List<Property>() {new ("test", 1)}),
            new (6, new List<Property>() {new ("test", 1)}),
            new (7, new List<Property>() {new ("test", 1)}),
        };
    }

    [Test]
    public void FirstDayTest()
    {
        var attendance = Create("test", GetStandardNodes(), Container);
        
        Assert.AreEqual(1, attendance.DayCount);
        Assert.AreEqual(0, attendance.LastClaimedDay);

        attendance.Claim(PlayerAction.TEST);
        Assert.AreEqual(1, attendance.LastClaimedDay);
        Assert.AreEqual(1, (int)PropertyManager.GetBalance(PropertyType.Get("test")));
    }

    [Test]
    public void SecondDayTest()
    {
        var attendance = Create("test", GetStandardNodes(), Container);
        
        Assert.AreEqual(1, attendance.DayCount);
        Assert.AreEqual(0, attendance.LastClaimedDay);

        attendance.Claim(PlayerAction.TEST);
        Assert.AreEqual(1, attendance.LastClaimedDay);
        
        Clock.AddDebugOffset(TimeSpan.FromDays(1));
        Assert.AreEqual(2, attendance.DayCount);
        Assert.AreEqual(1, attendance.LastClaimedDay);
        
        attendance.Claim(PlayerAction.TEST);
        Assert.AreEqual(2, attendance.LastClaimedDay);
        Assert.AreEqual(2, (int)PropertyManager.GetBalance(PropertyType.Get("test")));
    }

    [Test]
    public void DayPassedDuringOffline()
    {
        var attendance = Create("test", GetStandardNodes(), Container);
        attendance.Claim(PlayerAction.TEST);

        Clock.Initialize();
        Clock.AddDebugOffset(TimeSpan.FromDays(1));
        
        var newContainer = Container.CreateSubContainer();
        newContainer.Bind<PeriodicResetHandler>().AsSingle();
        
        var newAttendance = Create("test", GetStandardNodes(), newContainer);
        Assert.AreEqual(2, newAttendance.DayCount);
        Assert.AreEqual(1, newAttendance.LastClaimedDay);
        
        newAttendance.Claim(PlayerAction.TEST);
        Assert.AreEqual(2, newAttendance.LastClaimedDay);
        Assert.AreEqual(2, (int)PropertyManager.GetBalance(PropertyType.Get("test")));
    }

    [Test]
    public void CompleteTest()
    {
        var attendance = Create("test", GetStandardNodes(), Container);
        for (var i = 0; i < attendance.Nodes[^1].day; i++)
        {
            Clock.AddDebugOffset(TimeSpan.FromDays(1));
            attendance.Claim(PlayerAction.TEST);
        }
        
        Assert.AreEqual(attendance.Nodes.Count, (int)PropertyManager.GetBalance(PropertyType.Get("test")));
        Debug.Log(attendance.LastClaimedDay);
        Debug.Log(attendance.Nodes[^1].day);
        Assert.IsTrue(attendance.IsCompleted);
    }

    public override void Teardown()
    {
        Container.Resolve<SaveDataManager>().Reset();
        PropertyType.DeleteType("test");
        base.Teardown();
    }
}
