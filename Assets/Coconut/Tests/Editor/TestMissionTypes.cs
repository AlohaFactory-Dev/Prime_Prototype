using System.Numerics;
using Aloha.Coconut;
using Aloha.Coconut.Missions;

// 자동화테스트용으로 정의된 가짜 미션타입들
public static class TestMissionType
{
    public const MissionType TYPE_1 = (MissionType)int.MinValue;
    public const MissionType TYPE_2 = (MissionType)(int.MinValue + 1);
}

[Mission(TestMissionType.TYPE_1)]
public class TestMissionType1 : Mission
{
    public TestMissionType1(MissionData missionData, PropertyManager propertyManager, Mission.SaveData missionSaveData = null) : base(missionData, propertyManager, missionSaveData)
    {
    }

    public override void Start()
    {
        SetProgress(Objective);
    }
    
    public void ForceSetProgress(BigInteger progress)
    {
        base.SetProgress(progress);
    }
}

[Mission(TestMissionType.TYPE_2)]
public class TestMissionType2 : Mission
{
    public TestMissionType2(MissionData missionData, PropertyManager propertyManager, Mission.SaveData missionSaveData = null) : base(missionData, propertyManager, missionSaveData)
    {
    }

    public override void Start()
    {
        SetProgress(Objective);
    }
}