using Aloha.Coconut;

namespace CoconutMilk.Equipments
{
    public class Equipment
    {
        public int UID => _saveData.uid;
        public EquipmentType Type { get; }
        public bool IsEquipped { get; set; }
        private readonly SaveData _saveData;

        private Equipment(SaveData saveData, EquipmentType type)
        {
            _saveData = saveData;
            Type = type;
        }

        public bool IsIngredientOf(Equipment other)
        {
            if (other.Type.IsMaxRarity || other == this) return false;
            var otherRarityData = other.Type.RarityData;
            if (Type.RarityData.requiredSource == EquipmentRequiredSource.SameEquipment)
            {
                return Type.RarityData.requiredRarity == otherRarityData.rarity && Type.Set == other.Type.Set && Type.Part == other.Type.Part;
            }
            else
            {
                return Type.RarityData.requiredRarity == otherRarityData.rarity && Type.Part == other.Type.Part;
            }
        }

        public PropertyType ToPropertyType()
        {
            return PropertyType.Get(EquipmentConfigs.PropertyTypeGroup, Type.TypeId);
        }

        public class SaveData
        {
            public int typeId;
            public int uid;

            public SaveData(int typeId, int uid)
            {
                this.typeId = typeId;
                this.uid = uid;
            }
        }

        public class Factory
        {
            private readonly EquipmentDatabase _equipmentDatabase;

            public Factory(EquipmentDatabase equipmentDatabase)
            {
                _equipmentDatabase = equipmentDatabase;
            }

            public Equipment Create(SaveData saveData)
            {
                return new Equipment(saveData, _equipmentDatabase.GetTypeData(saveData.typeId));
            }
        }
    }
}