using System.Collections.Generic;
using Aloha.Coconut;

namespace CoconutMilk.Equipments
{
    public enum EquipmentRarity
    {
        None = 0,
        Common = 1,
        Great = 2,
        Rare = 3,
        Epic = 4,
        Epic1 = 5,
        Epic2 = 6,
        Legendary = 7,
        Legendary1 = 8,
        Legendary2 = 9,
        Legendary3 = 10,
        Mythic = 11,
        Mythic1 = 12,
        Mythic2 = 13,
        Mythic3 = 14,
        Mythic4 = 15,
    }

    public enum EquipmentRequiredSource
    {
        SameEquipment = 1,
        SameEquipmentOrMaterial = 2
    }

    // TypeID 규칙에 따라 최대 2자리까지(1~99)만 할당 가능    
    public enum EquipmentSet
    {
        Normal_01 = 1,
        Normal_02,
        Normal_03,
        Normal_04,
        Normal_05,
        Normal_06,
        Normal_07,
        Normal_08,
        Normal_09,
        Normal_10,
        Normal_11,
        Normal_12,
        Normal_13,
        Normal_14,
        Normal_15,
        Normal_16,
        Normal_17,
        Normal_18,
        Normal_19,
        Normal_20,
        SGrade_01,
        SGrade_02,
        SGrade_03,
        SGrade_04,
        SGrade_05,
        SGrade_06,
        SGrade_07,
        SGrade_08,
        SGrade_09,
        SGrade_10,
        SGrade_11,
        SGrade_12,
        SGrade_13,
        SGrade_14,
        SGrade_15,
        SGrade_16,
        SGrade_17,
        SGrade_18,
        SGrade_19,
        SGrade_20
    }

    // TypeID 규칙에 따라 최대 2자리까지(1~99)만 할당 가능
    public enum EquipmentPart
    {
        Weapon = 1,
        Body = 2,
        Ring = 3,
        Accessories = 4
    }


    public struct EquipmentTypeTableData
    {
        public int id;
        public string skinId;
        public List<EquipmentStatBonus> StatBonus;
    }

    public struct EquipmentOption
    {
        public EquipmentPart part;
        public EquipmentSet set;
        public EquipmentRarity rarity;
        public string optionType;
        public bool isActive;

        public void SetActive(bool active)
        {
            isActive = active;
        }
    }

    public struct EquipmentStatBonus
    {
        public StatType type;
        public int baseBonus;
        public int bonusInc;
    }

    public class EquipmentData
    {
        [CSVColumn] public int id;
        [CSVColumn] public EquipmentPart part;
        [CSVColumn] public EquipmentSet set;
        [CSVColumn] public EquipmentRarity rarity;
        [CSVColumn] public string skinId;
        [CSVColumn] public List<StatType> propertyType;
        [CSVColumn] public List<int> baseBonus;
        [CSVColumn] public List<int> bonusInc;
        [CSVColumn] public string optionType;
    }

    public enum StatType
    {
        None = 0,
        AttackPower = 1,
        MaxHp = 2,
        Armor = 3
    }

    public static class EquipmentConfigs
    {
        public static PropertyTypeGroup PropertyTypeGroup => PropertyTypeGroup.Equipment;

        public static EquipmentPart[] Parts { get; } =
        {
            EquipmentPart.Weapon,
            EquipmentPart.Body,
            EquipmentPart.Ring,
            EquipmentPart.Accessories
        };

        public static PropertyType GetLevelUpCostPropertyType()
        {
            return PropertyType.Get(PropertyTypeAlias.EquipmentStone);
        }

        public static PropertyType GetStatLevelUpPropertyType()
        {
            return PropertyType.Get(PropertyTypeAlias.EquipmentScroll);
        }

        public static EquipmentRarity GetMaxRarity()
        {
            return EquipmentRarity.Mythic4;
        }
    }
}