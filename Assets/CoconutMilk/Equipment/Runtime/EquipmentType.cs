using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut;

namespace CoconutMilk.Equipments
{
    public class EquipmentType
    {
        public int TypeId => EquipmentTypeTableData.id;
        public EquipmentRarity Rarity => RarityData.rarity;
        public EquipmentPart Part { get; }
        public EquipmentSet Set { get; }
        public EquipmentRarityData RarityData { get; }
        public EquipmentType Next { get; internal set; }
        public bool IsMaxRarity => Next == null;

        public List<EquipmentOption> PotentialOptions { get; private set; }
        public List<EquipmentOption> ActiveOptions { get; private set; }
        public EquipmentTypeTableData EquipmentTypeTableData { get; }

        public EquipmentType(EquipmentTypeTableData tableData, EquipmentRarityData rarityData, List<EquipmentOption> potentialOptions)
        {
            EquipmentTypeTableData = tableData;
            (_, Set, Part) = DecodeId(TypeId);
            RarityData = rarityData;
            var newOptions = new List<EquipmentOption>();
            foreach (var potentialOption in potentialOptions)
            {
                newOptions.Add(new EquipmentOption
                {
                    part = potentialOption.part,
                    set = potentialOption.set,
                    rarity = potentialOption.rarity,
                    optionType = potentialOption.optionType,
                    isActive = potentialOption.rarity <= Rarity
                });
            }

            PotentialOptions = newOptions;
            ActiveOptions = PotentialOptions.Where(effect => effect.isActive).ToList();
        }

        public static (EquipmentRarity rarity, EquipmentSet set, EquipmentPart part) DecodeId(int typeId)
        {
            return ((EquipmentRarity)(typeId % 100), (EquipmentSet)(typeId / 100 % 100), (EquipmentPart)(typeId / 10000));
        }

        public static int EncodeId(EquipmentRarity rarity, EquipmentSet set, EquipmentPart part)
        {
            return (int)part * 10000 + (int)set * 100 + (int)rarity;
        }

        public PropertyType ToPropertyType()
        {
            return PropertyType.Get(EquipmentConfigs.PropertyTypeGroup, TypeId);
        }
    }
}