using Newtonsoft.Json;

namespace Aloha.Coconut.Player
{
    // PvP 등에서 다른 플레이어의 정보를 표시할 때도 사용할 수 있음
    public class PlayerProfile
    {
        [JsonIgnore] public int Level => levelSaveData.level;
        
        public string Nickname { get; internal set; }
        public int ProfileImageId { get; internal set; }
        public int ProfileFrameId { get; internal set; }
        public string UID { get; internal set; }
        public readonly GrowthLevel.SaveData levelSaveData;
        
        [JsonConstructor]
        public PlayerProfile(string nickname, string uid, int profileImageId, int profileFrameId, GrowthLevel.SaveData levelSaveData)
        {
            Nickname = nickname;
            ProfileImageId = profileImageId;
            ProfileFrameId = profileFrameId;
            UID = uid;
            
            this.levelSaveData = levelSaveData;
        }
    }
}
