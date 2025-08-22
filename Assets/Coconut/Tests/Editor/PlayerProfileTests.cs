using Aloha.Coconut.Player;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Aloha.Coconut.Tests.Editor
{
    public class PlayerProfileTests
    {
        [Test]
        public void JsonTest()
        {
            var playerProfile = new PlayerProfile("hi", "uid", 0, 0, new GrowthLevel.SaveData());
            var json = JsonConvert.SerializeObject(playerProfile);
            var newProfile = JsonConvert.DeserializeObject<PlayerProfile>(json);
            
            Assert.AreEqual(playerProfile.Nickname, newProfile.Nickname);
            Assert.AreEqual(playerProfile.ProfileImageId, newProfile.ProfileImageId);
            Assert.AreEqual(playerProfile.ProfileFrameId, newProfile.ProfileFrameId);
            Assert.AreEqual(playerProfile.Level, newProfile.Level);
        }
    }
}