using System.Collections.Generic;
using Aloha.Coconut;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoconutMilk.Equipments
{
    public class EquipmentSlot
    {
        // 프로퍼티
        public Dictionary<StatType, int> StatBonus { get; private set; }
        public EquipmentPart Part { get; }
        public int EquippedUID => _saveData.equippedUID[_index];
        public Equipment EquippedEquipment { get; private set; }
        public int MaxLevel => _saveData.maxLevel;
        public int Level => _saveData.level;
        public int StatLevel => _saveData.statLevel;
        public int Index => _index;

        public ISubject<EquipmentPart> OnLevelUp { get; } = new Subject<EquipmentPart>();
        public ReactiveProperty<bool> CanLevelUp { get; } = new(false);

        // 필드
        private SaveData _saveData;
        private readonly PropertyManager _propertyManager;
        private readonly EquipmentDatabase _equipmentDatabase;
        private readonly int _index = 0;

        // 생성자
        private EquipmentSlot(EquipmentPart part, SaveData saveData, PropertyManager propertyManager, EquipmentDatabase equipmentDatabase, int index)
        {
            Part = part;
            _saveData = saveData;
            _propertyManager = propertyManager;
            _equipmentDatabase = equipmentDatabase;
            _index = index;

            _saveData.maxLevel = _equipmentDatabase.GetMaxLevel(_saveData.statLevel);

            _propertyManager.OnPropertyUpdated
                .Where(x => x.type.Alias == PropertyTypeAlias.EquipmentStone || x.type.Alias == PropertyTypeAlias.EquipmentScroll)
                .Subscribe(_ => CheckCanLevelUp());

            OnLevelUp.Where(x => x == Part).Subscribe(_ => CheckCanLevelUp());
        }

        // 레벨업 가능 여부 체크
        public void CheckCanLevelUp()
        {
            var isAffordable = IsLevelUpAffordable() && _equipmentDatabase.GetTotalMaxLevel() > _saveData.maxLevel;
            CanLevelUp.Value = isAffordable;
        }

        // 장비 장착
        public void Equip(Equipment equipment)
        {
            EquippedEquipment = equipment;
            EquippedEquipment.IsEquipped = true;
            _saveData.equippedUID[_index] = equipment.UID;

            UpdateStatBonus();
            UpdatePlayerStat(true);
        }

        // 장비 해제
        public void Unequip()
        {
            if (EquippedEquipment == null) return;
            EquippedEquipment.IsEquipped = false;
            EquippedEquipment = null;
            _saveData.equippedUID[_index] = 0;
            UpdatePlayerStat(false);
            StatBonus = new Dictionary<StatType, int>();
        }

        // 스탯 보너스 갱신
        private void UpdateStatBonus()
        {
            if (EquippedEquipment == null) return;
            var stats = EquippedEquipment.Type.EquipmentTypeTableData.StatBonus;
            StatBonus = new Dictionary<StatType, int>();
            foreach (var stat in stats)
            {
                StatBonus[stat.type] = CalculateStatBonus(stat);
            }
        }

        // 플레이어 스탯 갱신 (TODO)
        private void UpdatePlayerStat(bool add)
        {
            Debug.Log("플레이어 스탯 업데이트 하세요");
        }

        // 재료 타입 반환
        public EquipmentType GetIngredientEquipmentType(Equipment equipment)
        {
            return _equipmentDatabase.GetIngredientOf(equipment);
        }

        // 스탯 보너스 계산
        public int CalculateStatBonus(EquipmentStatBonus statBonus)
        {
            return _equipmentDatabase.GetStatBonus(statBonus, StatLevel, Level);
        }

        // 레벨업 비용 관련
        public bool IsLevelUpAffordable()
        {
            return _propertyManager.HaveEnough(GetLevelUpCost());
        }

        public Property GetLevelUpCost()
        {
            return GetLevelUpCost(StatLevel, Level);
        }

        public Property GetLevelUpCost(int statLevel, int level)
        {
            return _equipmentDatabase.GetLevelUpCost(Part, statLevel, level);
        }

        // 레벨업
        public void LevelUp()
        {
            if (_equipmentDatabase.GetTotalMaxLevel() <= Level)
            {
                Debug.Log("최대 레벨에 도달했습니다: " + Level);
                return;
            }

            var property = GetLevelUpCost();
            if (IsLevelUpAffordable())
            {
                if (EquippedEquipment != null)
                    UpdatePlayerStat(false);

                if (property.type.Alias == PropertyTypeAlias.EquipmentStone)
                    _saveData.level++;
                else
                    _saveData.statLevel++;

                _saveData.maxLevel = _equipmentDatabase.GetMaxLevel(_saveData.statLevel);
                _propertyManager.TryUse(property, PlayerAction.UNTRACKED);

                OnLevelUp.OnNext(Part);

                if (EquippedEquipment != null)
                {
                    UpdateStatBonus();
                    UpdatePlayerStat(true);
                }

                return;
            }

            Debug.Log("레벨업 비용이 부족합니다: " + property.type.nameKey);
        }

        // 일괄 레벨업
        public void BatchLevelUp()
        {
            if (!IsLevelUpAffordable())
            {
                var property = GetLevelUpCost();
                Debug.Log("레벨업 비용이 부족합니다: " + property.type.nameKey);
                return;
            }

            var count = MaxLevel - Level;

            // MaxLevel이 Level과 같을 경우(EquipmnetScroll로 돌파 가능한 경우)
            if (count == 0)
            {
                count = 1;
            }

            for (var i = 0; i < count; i++)
            {
                if (!IsLevelUpAffordable()) break;
                LevelUp();
            }
        }

        // 저장 데이터 클래스
        public class SaveData
        {
            public int[] equippedUID;
            public int maxLevel = 10;
            public int level = 1;
            public int statLevel = 1;
        }

        // 팩토리 클래스
        public class Factory
        {
            private readonly PropertyManager _propertyManager;
            private readonly EquipmentDatabase _equipmentDatabase;

            public Factory(PropertyManager propertyManager, EquipmentDatabase equipmentDatabase)
            {
                _propertyManager = propertyManager;
                _equipmentDatabase = equipmentDatabase;
            }

            public EquipmentSlot Create(EquipmentPart part, SaveData saveData, int index)
            {
                return new EquipmentSlot(part, saveData, _propertyManager, _equipmentDatabase, index);
            }
        }
    }
}