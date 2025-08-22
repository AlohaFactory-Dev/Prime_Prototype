using Sirenix.OdinInspector;
using UnityEngine;

namespace Aloha.Coconut
{
    [CreateAssetMenu(fileName = "CoreConfig", menuName = "Coconut/Config/PlayerActionConfig")]
    public class PlayerActionConfig : CoconutConfig
    {
#if UNITY_EDITOR
        [InfoBox("player_actions 테이블을 기반으로 enum 생성")]
        [Button]
        public void GeneratePlayerActionNameEnums()
        {
            PlayerAction.Load();
            EnumGenerator<PlayerAction, PlayerActionName>.New()
                .SetSources(PlayerAction.GetAll())
                .SetFilePath("Assets/Coconut/Runtime/Core/PlayerAction/PlayerActionName.cs")
                .SetNameSelector(p => p.actionName)
                .SetIntValueSelector(p => p.actionId)
                .SetGeneratorClass(nameof(PlayerActionConfig))
                .Generate();
        }
#endif
    }
}
