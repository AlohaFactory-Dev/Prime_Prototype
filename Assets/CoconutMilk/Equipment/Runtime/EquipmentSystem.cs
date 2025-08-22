using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aloha.Coconut;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Random = UnityEngine.Random;

namespace CoconutMilk.Equipments
{
    public class EquipmentSystem : IPropertyHandler, IDisposable
    {
        public IReadOnlyReactiveCollection<Equipment> Inventory => _inventory;

        public IObservable<Unit> OnInventoryUpdated => _onInventoryUpdated;
        private Subject<Unit> _onInventoryUpdated = new();

        public IObservable<EquipmentSlot> OnEquipStateUpdated => _onEquipStateUpdated;
        private Subject<EquipmentSlot> _onEquipStateUpdated = new();

        public IObservable<EquipmentSlot> OnUnequipStateUpdated => _onUnequipStateUpdated;
        private Subject<EquipmentSlot> _onUnequipStateUpdated = new();

        private Subject<Equipment> _onRemoveEquipment = new();

        private readonly SaveData _saveData;
        private readonly Dictionary<EquipmentPart, List<EquipmentSlot>> _slots = new();
        private readonly Equipment.Factory _equipmentFactory;
        private readonly LazyInject<PropertyManager> _propertyManager;

        private ReactiveCollection<Equipment> _inventory = new();
        private SaveDataManager _saveDataManager;

        public EquipmentSystem(SaveDataManager saveDataManager, LazyInject<PropertyManager> propertyManager,
            Equipment.Factory equipmentFactory, EquipmentSlot.Factory slotFactory)
        {
            _saveDataManager = saveDataManager;
            _equipmentFactory = equipmentFactory;
            _propertyManager = propertyManager;
            _saveData = saveDataManager.Get<SaveData>("equipment_system");

            foreach (var part in EquipmentConfigs.Parts)
            {
                if (!_saveData.slotSaveDatas.ContainsKey(part))
                {
                    _saveData.slotSaveDatas[part] = new EquipmentSlot.SaveData();
                    if (part == EquipmentPart.Accessories || part == EquipmentPart.Ring)
                    {
                        // 액세서리와 반지 슬롯은 각각 2개의 서브 슬롯을 가지므로, 초기화 시 2개의 서브 슬롯을 생성
                        _saveData.slotSaveDatas[part].equippedUID = new int[2];
                    }
                    else
                    {
                        // 일반 슬롯은 하나의 서브 슬롯만 가지므로, 초기화 시 하나의 서브 슬롯을 생성
                        _saveData.slotSaveDatas[part].equippedUID = new int[1];
                    }
                }

                if (part == EquipmentPart.Accessories || part == EquipmentPart.Ring)
                {
                    // 액세서리 슬롯은 2개로 나누어져 있으므로, 각각의 슬롯을 생성
                    _slots[part] = new List<EquipmentSlot>
                    {
                        slotFactory.Create(part, _saveData.slotSaveDatas[part], 0),
                        slotFactory.Create(part, _saveData.slotSaveDatas[part], 1)
                    };
                }
                else
                {
                    // 일반 슬롯은 하나만 생성
                    _slots[part] = new List<EquipmentSlot>() { slotFactory.Create(part, _saveData.slotSaveDatas[part], 0) };
                }
            }

            foreach (var saveData in _saveData.eqSaveDatas)
            {
                var equipment = equipmentFactory.Create(saveData);
                _inventory.Add(equipment);
                for (int i = 0; i < _slots[equipment.Type.Part].Count; i++)
                {
                    if (_slots[equipment.Type.Part][i].EquippedUID == equipment.UID)
                    {
                        Equip(equipment, i);
                    }
                }
            }


            _onInventoryUpdated.Subscribe(_ =>
            {
                //TODO: 인벤토리 업데이트 시 장비 아이콘 레드닷 업데이트
            });
            _onEquipStateUpdated.Subscribe(x =>
            {
                //TODO: 장비 장착 시 레드닷 업데이트
            });
            _onUnequipStateUpdated.Subscribe(x =>
            {
                //TODO: 장비 장착 해제 시 레드닷 업데이트
            });
            _onRemoveEquipment.Subscribe(x =>
            {
                //TODO: 장비 제거 시 레드닷 업데이트
            });
        }

        public void LevelUp(Equipment equipment)
        {
            var slot = _slots[equipment.Type.Part][0];
            slot.LevelUp();
        }


        public void BatchLevelUp(Equipment equipment)
        {
            var slot = _slots[equipment.Type.Part][0];
            slot.BatchLevelUp();
        }

        public void Equip(Equipment equipment, int index = -1)
        {
            if (_slots[equipment.Type.Part].Count == 1)
            {
                var equippedEquipment = _slots[equipment.Type.Part][0].EquippedEquipment;
                if (equippedEquipment != null)
                {
                    Unequip(equipment.Type.Part, equippedEquipment.UID);
                }

                _slots[equipment.Type.Part][0].Equip(equipment);
                _onEquipStateUpdated.OnNext(_slots[equipment.Type.Part][0]);
            }
            else
            {
                if (index == -1)
                {
                    // 액세서리나 반지 슬롯의 경우, 두 개의 슬롯이 장착 돼 있는지 확인 후 두 개 모두 장착 돼 있다면 첫번째 슬롯에 장착
                    if (_slots[equipment.Type.Part][0].EquippedEquipment != null && _slots[equipment.Type.Part][1].EquippedEquipment != null)
                    {
                        Unequip(equipment.Type.Part, _slots[equipment.Type.Part][0].EquippedEquipment.UID);
                        _slots[equipment.Type.Part][0].Equip(equipment);
                        _onEquipStateUpdated.OnNext(_slots[equipment.Type.Part][0]);
                    }
                    else if (_slots[equipment.Type.Part][0].EquippedEquipment == null)
                    {
                        _slots[equipment.Type.Part][0].Equip(equipment);
                        _onEquipStateUpdated.OnNext(_slots[equipment.Type.Part][0]);
                    }
                    else if (_slots[equipment.Type.Part][1].EquippedEquipment == null)
                    {
                        _slots[equipment.Type.Part][1].Equip(equipment);
                        _onEquipStateUpdated.OnNext(_slots[equipment.Type.Part][1]);
                    }
                }
                else
                {
                    _slots[equipment.Type.Part][index].Equip(equipment);
                    _onEquipStateUpdated.OnNext(_slots[equipment.Type.Part][index]);
                }
            }
        }


        public void Unequip(EquipmentPart part, int uid)
        {
            foreach (var slot in _slots[part])
            {
                if (slot.EquippedEquipment != null && slot.EquippedEquipment.UID == uid)
                {
                    _onUnequipStateUpdated.OnNext(slot);
                    slot.Unequip();
                    return;
                }
            }
        }

        public bool IsMergeable(Equipment mainEquipment)
        {
            return _inventory.Count(eq => eq.IsIngredientOf(mainEquipment)) >= mainEquipment.Type.RarityData.requiredCount;
        }

        public List<MergeResult> BatchMerge()
        {
            var results = new List<MergeResult>();
            var used = new HashSet<Equipment>();

            // 인벤토리에서 머지 가능한 장비만 추출
            var mergeables = _inventory
                .Where(e => IsMergeable(e) && !e.Type.IsMaxRarity)
                .ToList();

            foreach (var main in mergeables)
            {
                if (used.Contains(main))
                    continue;

                // 머지 재료 자동 선택 (아직 사용되지 않은 것만)
                var ingredients = _inventory
                    .Where(eq => eq.IsIngredientOf(main) && eq != main && !used.Contains(eq))
                    .Take(main.Type.RarityData.requiredCount)
                    .ToList();

                // 재료가 부족하면 스킵
                if (ingredients.Count < main.Type.RarityData.requiredCount)
                    continue;

                used.Add(main);
                foreach (var ing in ingredients)
                    used.Add(ing);

                var result = Merge(main, ingredients);
                if (result.resultEquipment != null)
                    results.Add(result);
            }

            return results;
        }

        public MergeResult Merge(Equipment mainEquipment, List<Equipment> ingredients)
        {
            var mergeResult = new MergeResult();
            if (mainEquipment.Type.IsMaxRarity) return mergeResult;

            Assert.IsTrue(mainEquipment.Type.RarityData.requiredCount == ingredients.Count);
            foreach (var ingredient in ingredients)
            {
                Assert.IsTrue(ingredient.IsIngredientOf(mainEquipment));
            }

            var wasIngredientEquipped = mainEquipment.IsEquipped;
            mergeResult.mainEquipment = mainEquipment.Type;
            Remove(mainEquipment);

            foreach (var ingredient in ingredients)
            {
                wasIngredientEquipped |= ingredient.IsEquipped;
                mergeResult.ingredients.Add(ingredient.Type);
                Remove(ingredient);
            }

            // 로그 등을 일관되게 처리하기 위해 PropertyManager를 통해서 획득
            _propertyManager.Value.Obtain(new Property(mainEquipment.Type.Next.ToPropertyType(), 1), PlayerAction.UNTRACKED);

            // IPropertyHandler.Obtain에 의해 _equipments의 마지막에 추가된 것이 합성된 코스튬임
            mergeResult.resultEquipment = _inventory.Last();
            if (wasIngredientEquipped)
            {
                Equip(mergeResult.resultEquipment);
            }


            return mergeResult;
        }

        public Equipment GetIngredientOf(Equipment equipment)
        {
            var tempType = _slots[equipment.Type.Part][0].GetIngredientEquipmentType(equipment);
            if (tempType == null) return null;
            var tempSaveData = new Equipment.SaveData(tempType.TypeId, 0);
            return _equipmentFactory.Create(tempSaveData);
        }

        private void Remove(Equipment equipment)
        {
            if (equipment.IsEquipped) Unequip(equipment.Type.Part, equipment.UID);

            _inventory.Remove(equipment);
            _onRemoveEquipment.OnNext(equipment);
            var eqSaveData = _saveData.eqSaveDatas.Find(s => s.uid == equipment.UID);
            _saveData.eqSaveDatas.Remove(eqSaveData);

            _onInventoryUpdated.OnNext(Unit.Default);
        }

        public List<EquipmentOption> GetActiveOptions()
        {
            var options = new List<EquipmentOption>();
            foreach (var part in EquipmentConfigs.Parts)
            {
                foreach (var slot in _slots[part])
                {
                    if (slot.EquippedEquipment != null)
                    {
                        options.AddRange(slot.EquippedEquipment.Type.ActiveOptions);
                    }
                }
            }

            return options;
        }

        public EquipmentSlot GetSlot(EquipmentPart part, int index = 0)
        {
            Assert.IsTrue(_slots.ContainsKey(part), $"Invalid equipment part: {part}");
            Assert.IsTrue(index >= 0 && index < _slots[part].Count, $"Invalid index for part {part}: {index}");
            return _slots[part][index];
        }

        public List<Equipment> GetEquipped(EquipmentPart type)
        {
            return _slots[type]
                .Where(slot => slot.EquippedEquipment != null)
                .Select(slot => slot.EquippedEquipment)
                .ToList();
        }

        public void Dispose()
        {
            _onInventoryUpdated.Dispose();
            _onEquipStateUpdated.Dispose();
        }

        public class MergeResult
        {
            public EquipmentType mainEquipment;
            public List<EquipmentType> ingredients = new();
            public Equipment resultEquipment;
        }

        private class SaveData
        {
            public Dictionary<EquipmentPart, EquipmentSlot.SaveData> slotSaveDatas = new();
            public List<Equipment.SaveData> eqSaveDatas = new();
        }

        public Equipment CreateEquipment(Property property)
        {
            // 장비하지 않음을 UID 0으로 나타내므로, UID는 1부터 할당
            var newUID = 0;
            do
            {
                newUID = Random.Range(1, int.MaxValue);
            } while (_saveData.eqSaveDatas.Exists(s => s.uid == newUID));

            var newSaveData = new Equipment.SaveData(property.type.id, newUID);
            return _equipmentFactory.Create(newSaveData);
        }

        #region IPropertyHandler

        List<PropertyTypeGroup> IPropertyHandler.HandlingGroups { get; } = new() { EquipmentConfigs.PropertyTypeGroup };

        void IPropertyHandler.Obtain(Property property)
        {
            // 장비하지 않음을 UID 0으로 나타내므로, UID는 1부터 할당
            var newUID = 0;
            do
            {
                newUID = Random.Range(1, int.MaxValue);
            } while (_saveData.eqSaveDatas.Exists(s => s.uid == newUID));

            var newSaveData = new Equipment.SaveData(property.type.id, newUID);
            _saveData.eqSaveDatas.Add(newSaveData);

            var equipment = _equipmentFactory.Create(newSaveData);
            _inventory.Add(equipment);
            _onInventoryUpdated.OnNext(Unit.Default);
        }

        void IPropertyHandler.Use(Property property)
        {
            Debug.LogError("Equipment cannot be used.");
        }

        void IPropertyHandler.Set(Property property)
        {
            Debug.LogError("Equipment cannot be set.");
        }

        BigInteger IPropertyHandler.GetBalance(PropertyType property)
        {
            return 0;
        }

        #endregion
    }
}