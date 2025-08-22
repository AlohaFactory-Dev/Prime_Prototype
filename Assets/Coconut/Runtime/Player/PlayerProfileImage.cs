namespace Aloha.Coconut.Player
{
    public class PlayerProfileImage
    {
        public int Id => _imagePropertyType.id;
        public string NameKey => _imagePropertyType.nameKey;
        public string DescKey => _imagePropertyType.descriptionKey;
        public string IconPath => _imagePropertyType.iconPath;
        
        public bool IsUnlocked { get; internal set; }
        
        private readonly PropertyType _imagePropertyType;

        public PlayerProfileImage(PropertyType imagePropertyType)
        {
            _imagePropertyType = imagePropertyType;
        }
    }
}
