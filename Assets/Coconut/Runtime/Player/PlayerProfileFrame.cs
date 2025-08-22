namespace Aloha.Coconut.Player
{
    public class PlayerProfileFrame
    {
        public int Id => _imagePropertyType.id;
        public string NameKey => _imagePropertyType.nameKey;
        public string DescKey => _imagePropertyType.descriptionKey;
        public string IconPath => _imagePropertyType.iconPath;
        
        public bool IsUnlocked { get; internal set; }
        
        private readonly PropertyType _imagePropertyType;

        public PlayerProfileFrame(PropertyType imagePropertyType)
        {
            _imagePropertyType = imagePropertyType;
        }
    }
}