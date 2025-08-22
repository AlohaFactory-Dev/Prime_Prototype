using UnityEngine;

namespace Aloha.Coconut.Player
{
    [CreateAssetMenu(menuName = "Coconut/Config/PlayerProfileConfig")]
    public class PlayerProfileConfig : CoconutConfig
    {
        public PropertyTypeGroup profileImageGroup;
        public PropertyTypeGroup profileFrameGroup;
    }
}
