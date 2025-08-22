using UnityEngine;

namespace Aloha.Coconut.Player
{
    [CreateAssetMenu(menuName = "Coconut/Config/MyProfileConfig")]
    public class MyProfileConfig : CoconutConfig
    {
        public Property NicknameChangeCost => new Property(nicknameChangeCostType, nicknameChangeCostAmount);
        public int FreeNicknameChangeCount => freeNicknameChangeCount;
        public int NicknameLengthMin => nicknameLengthMin;
        public int NicknameLengthMax => nicknameLengthMax;
        public PlayerActionName NickChangeActionName => nickChangeActionName;
        public string DefaultNicknamePrefix => defaultNicknamePrefix;
        public int DefaultProfileImageId => defaultProfileImageId;
        public int DefaultProfileFrameId => defaultProfileFrameId;
        
        public PropertyTypeAlias LevelExpPropertyType => levelExpPropertyType;
        
        [Header("Default Values")]
        [SerializeField] private string defaultNicknamePrefix = "Player";
        [SerializeField] private int defaultProfileImageId;
        [SerializeField] private int defaultProfileFrameId;

        [Header("Growth Level")] 
        [SerializeField] private PropertyTypeAlias levelExpPropertyType;
        
        [Header("Nickname")]
        [SerializeField] private PropertyTypeAlias nicknameChangeCostType;
        [SerializeField] private int nicknameChangeCostAmount;
        [SerializeField] private int freeNicknameChangeCount = 1;
        [SerializeField] private int nicknameLengthMin = 2;
        [SerializeField] private int nicknameLengthMax = 12;
        [SerializeField] private PlayerActionName nickChangeActionName;
    }
}
