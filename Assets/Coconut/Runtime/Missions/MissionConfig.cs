using Sirenix.OdinInspector;
using UnityEngine;

namespace Aloha.Coconut.Missions
{
    [CreateAssetMenu(menuName = "Coconut/Config/MissionConfig")]
    public class MissionConfig : CoconutConfig
    {
#if UNITY_EDITOR
        [Button]
        public void GenerateMissionTypeEnums()
        {
            var missionTypeDatas = TableManager.Get<MissionTypeData>("mission_types");
            
            EnumGenerator<MissionTypeData, MissionType>.New()
                .SetSources(missionTypeDatas)
                .SetNamespace("Aloha.Coconut.Missions")
                .SetFilePath("Assets/Coconut/Runtime/Missions/MissionType.cs")
                .SetNameSelector(m => m.typeName)
                .SetIntValueSelector(m => m.typeId)
                .SetGeneratorClass(nameof(MissionConfig))
                .Generate();
        }
#endif
    }
}