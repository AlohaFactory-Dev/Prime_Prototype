using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aloha.Coconut.Missions
{
    public class MissionTypeData
    {
        [CSVColumn] public int typeId;
        [CSVColumn] public string typeName;
        [CSVColumn] public string descriptionKey;
        
        private static Dictionary<MissionType, MissionTypeData> _missionTypeDatas = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            _missionTypeDatas = new Dictionary<MissionType, MissionTypeData>();

            var missionTypeDatas = TableManager.Get<MissionTypeData>("mission_types");
            foreach (var missionTypeData in missionTypeDatas)
            {
                Assert.IsTrue(Enum.TryParse(typeof(MissionType), missionTypeData.typeName, out _), 
                    $"Invalid MissionType: {missionTypeData.typeName}. MissionConfig에서 MissionType을 생성해주세요.");
                
                _missionTypeDatas[(MissionType)missionTypeData.typeId] = missionTypeData;
            }
            
            Debug.Log($"Coconut.MissionTypeData: MissionTypeData {_missionTypeDatas.Count}개 로드되었습니다.");
        }
        
        internal static string GetDescriptionKey(MissionType missionType)
        {
            if (_missionTypeDatas.ContainsKey(missionType)) return _missionTypeDatas[missionType].descriptionKey;
            else return "";
        }
    }
}