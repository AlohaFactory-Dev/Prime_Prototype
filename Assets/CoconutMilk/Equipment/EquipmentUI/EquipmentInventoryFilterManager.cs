using System.Collections.Generic;
using System.Linq;
using CoconutMilk.Equipments;
using UniRx;

public enum OrderFilterType
{
    Rarity,
    Part
}

public enum PartFilterType
{
    All,
    Weapon = 1,
    Body = 2,
    Ring = 3,
    Accessories = 4
}

public class EquipmentInventoryFilterManager
{
    public ReactiveProperty<OrderFilterType> CurrentOrderFilterType { get; } = new(OrderFilterType.Rarity);
    public ReactiveProperty<PartFilterType> CurrentPartFilterType { get; } = new(PartFilterType.All);
    public ReactiveProperty<EquipmentIcon> OnMergePartFilter { get; } = new(null);

    public EquipmentInventoryFilterManager()
    {
    }

    // Rarity 기준 내림차순 정렬
    public List<Equipment> SortByRarity(IEnumerable<Equipment> equipments)
    {
        return equipments
            .OrderByDescending(e => e.Type.RarityData.rarity)
            .ThenBy(e => e.Type.Part)
            .ThenBy(e => e.Type.TypeId)
            .ToList();
    }

    // Part 기준 오름차순 정렬
    public List<Equipment> SortByPart(IEnumerable<Equipment> equipments)
    {
        return equipments
            .OrderBy(e => e.Type.Part)
            .ThenByDescending(e => e.Type.RarityData.rarity)
            .ThenBy(e => e.Type.TypeId)
            .ToList();
    }
}