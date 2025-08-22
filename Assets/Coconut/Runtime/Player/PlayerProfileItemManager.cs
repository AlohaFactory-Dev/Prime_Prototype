using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aloha.Coconut.Player
{
    public class PlayerProfileItemManager : IPropertyHandler
    {
        private PlayerProfileConfig _config;
        private SaveData _saveData;
        
        private List<PlayerProfileImage> _images;
        private List<PlayerProfileFrame> _frames;

        public PlayerProfileItemManager(SaveDataManager saveDataManager)
        {
            _saveData = saveDataManager.Get<SaveData>("player_profile_item_manager");
            _config = CoconutConfig.Get<PlayerProfileConfig>();
            HandlingGroups = new List<PropertyTypeGroup> { _config.profileImageGroup, _config.profileFrameGroup };

            _images = PropertyType.GetAll(_config.profileImageGroup).Select(p => new PlayerProfileImage(p)).ToList();
            foreach (var image in _images)
            {
                image.IsUnlocked = _saveData.unlockedImages.Contains(image.Id);
            }
            
            _frames = PropertyType.GetAll(_config.profileFrameGroup).Select(p => new PlayerProfileFrame(p)).ToList();
            foreach (var frame in _frames)
            {
                frame.IsUnlocked = _saveData.unlockedFrames.Contains(frame.Id);
            }
        }

        public void UnlockImage(int imageId)
        {
            _saveData.unlockedImages.Add(imageId);
            _images.Find(i => i.Id == imageId).IsUnlocked = true;
        }
        
        public void UnlockFrame(int frameId)
        {
            _saveData.unlockedFrames.Add(frameId);
            _frames.Find(f => f.Id == frameId).IsUnlocked = true;
        }

        private class SaveData
        {
            public HashSet<int> unlockedImages;
            public HashSet<int> unlockedFrames;
        }
        
        #region IPropertyHandler
        public List<PropertyTypeGroup> HandlingGroups { get; }

        void IPropertyHandler.Obtain(Property property)
        {
            if(property.type.group == _config.profileImageGroup) UnlockImage(property.type.id);
            else if(property.type.group == _config.profileFrameGroup) UnlockFrame(property.type.id); 
        }

        void IPropertyHandler.Use(Property property)
        {
            throw new System.NotImplementedException();
        }

        void IPropertyHandler.Set(Property property)
        {
            if(property.type.group == _config.profileImageGroup) UnlockImage(property.type.id);
            else if(property.type.group == _config.profileFrameGroup) UnlockFrame(property.type.id);
        }

        BigInteger IPropertyHandler.GetBalance(PropertyType property)
        {
            if(property.group == _config.profileImageGroup) return _saveData.unlockedImages.Contains(property.id) ? 1 : 0;
            if(property.group == _config.profileFrameGroup) return _saveData.unlockedFrames.Contains(property.id) ? 1 : 0;
            return 0;
        }
        #endregion
    }
}
