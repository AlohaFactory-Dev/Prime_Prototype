using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut;
using UnityEngine;

namespace CoconutMilk.Equipments
{
    public class EquipmentDatabase
    {
        private class EquipmentLevelUpCost
        {
            [CSVColumn] public int statLevel;
            [CSVColumn] public int level;
            [CSVColumn] public int stoneCost;
            [CSVColumn] public int scrollCost;
            [CSVColumn] public float statMultipler;
        }

        private Dictionary<int, EquipmentType> _typeDatas = new();
        private Dictionary<EquipmentRarity, EquipmentRarityData> _rarityDatas = new();
        private Dictionary<EquipmentRarity, List<EquipmentType>> _typeDatasByRarity = new();
        private List<EquipmentLevelUpCost> _levelUpCosts = new();
        private List<EquipmentData> _equipmentData = new();

        public EquipmentDatabase()
        {
            _equipmentData = TableManager.Get<EquipmentData>("equipment");
            var options = new Dictionary<(EquipmentSet, EquipmentPart), List<EquipmentOption>>();
            foreach (var data in _equipmentData)
            {
                if (!options.ContainsKey((data.set, data.part)))
                {
                    options[(data.set, data.part)] = new List<EquipmentOption>();
                }

                if (!TableManager.IsMagicNumber(data.optionType))
                {
                    options[(data.set, data.part)].Add(ConvertEquipmentOption(data.part, data.set, data.rarity, data.optionType));
                }
            }

            var rarityDatas = TableManager.Get<EquipmentRarityData>("equipment_rarity");
            foreach (var data in rarityDatas)
            {
                _rarityDatas[data.rarity] = data;
            }

            foreach (var data in _equipmentData)
            {
                var statBonus = new List<EquipmentStatBonus>();
                for (var i = 0; i < data.propertyType.Count; i++)
                {
                    if (data.propertyType[i] == StatType.None) continue;
                    statBonus.Add(new EquipmentStatBonus
                    {
                        type = data.propertyType[i],
                        baseBonus = data.baseBonus[i],
                        bonusInc = data.bonusInc[i]
                    });
                }

                var type = new EquipmentTypeTableData
                {
                    id = data.id,
                    skinId = data.skinId,
                    StatBonus = statBonus
                };
                var tempOtion = options[(data.set, data.part)].ToList();
                var typeData = new EquipmentType(type, _rarityDatas[data.rarity], tempOtion);
                _typeDatas[typeData.TypeId] = typeData;
            }

            // set next grade type data
            foreach (var typeData in _typeDatas.Values)
            {
                var (rarity, set, part) = EquipmentType.DecodeId(typeData.TypeId);
                if (EquipmentConfigs.GetMaxRarity() == rarity) continue;

                var nextRarity = rarity + 1;
                var nextTypeId = EquipmentType.EncodeId(nextRarity, set, part);
                typeData.Next = _typeDatas[nextTypeId];
            }

            var levelUpCosts = TableManager.Get<EquipmentLevelUpCost>("equipment_level_cost");
            foreach (var data in levelUpCosts)
            {
                _levelUpCosts.Add(data);
            }
        }

        private EquipmentOption ConvertEquipmentOption(EquipmentPart part, EquipmentSet set, EquipmentRarity rarity, string optionType)
        {
            return new EquipmentOption
            {
                part = part,
                set = set,
                rarity = rarity,
                optionType = optionType
            };
        }

        public EquipmentType GetTypeData(int typeId)
        {
            return _typeDatas[typeId];
        }

        public EquipmentType GetIngredientOf(Equipment equipment)
        {
            if (equipment.Type.IsMaxRarity) return null;

            var rarityData = equipment.Type.RarityData;
            if (rarityData.requiredSource == EquipmentRequiredSource.SameEquipment)
            {
                return GetTypeData(EquipmentType.EncodeId(rarityData.requiredRarity, equipment.Type.Set, equipment.Type.Part));
            }
            else
            {
                //TODO : Material이 구현 되면 수정
                return GetTypeData(EquipmentType.EncodeId(rarityData.requiredRarity, equipment.Type.Set, equipment.Type.Part));
            }
        }

        public List<EquipmentType> GetEveryTypeData()
        {
            return new List<EquipmentType>(_typeDatas.Values);
        }

        public List<EquipmentType> GetEveryTypeDataByRarity(EquipmentRarity rarity)
        {
            if (!_typeDatasByRarity.ContainsKey(rarity))
            {
                _typeDatasByRarity[rarity] = new List<EquipmentType>();
                foreach (var typeData in _typeDatas.Values)
                {
                    if (typeData.Rarity == rarity)
                    {
                        _typeDatasByRarity[rarity].Add(typeData);
                    }
                }
            }

            return _typeDatasByRarity[rarity];
        }

        public Property GetLevelUpCost(EquipmentPart equipmentPart, int statLevel, int targetLevel)
        {
            var levelUpCost = _levelUpCosts.Find(x => x.statLevel == statLevel && x.level == targetLevel);

            if (!TableManager.IsMagicNumber(levelUpCost.scrollCost))
            {
                return new Property(EquipmentConfigs.GetStatLevelUpPropertyType(), levelUpCost.scrollCost);
            }

            return new Property(EquipmentConfigs.GetLevelUpCostPropertyType(), levelUpCost.stoneCost);
        }

        public int GetMaxLevel(int statLevel)
        {
            return _levelUpCosts.Where(x => x.statLevel == statLevel).Max(x => x.level);
        }

        public int GetTotalMaxLevel()
        {
            return _levelUpCosts.Max(x => x.level);
        }

        public int GetStatBonus(EquipmentStatBonus statBonus, int statLevel, int level)
        {
            var statMultiple = _levelUpCosts.Find(x => x.statLevel == statLevel).statMultipler;
            return Mathf.CeilToInt(statBonus.baseBonus + statBonus.bonusInc * (level - 1) * statMultiple);
        }
    }
}