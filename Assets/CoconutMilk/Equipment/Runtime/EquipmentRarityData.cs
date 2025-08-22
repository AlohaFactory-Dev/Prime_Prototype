namespace CoconutMilk.Equipments
{
    public class EquipmentRarityData
    {
        [CSVColumn] public EquipmentRarity rarity;

        // 머지 관련 정보
        [CSVColumn] public EquipmentRarity requiredRarity;
        [CSVColumn] public EquipmentRequiredSource requiredSource;
        [CSVColumn] public int requiredCount;
        [CSVColumn] public string nameKey;
    }
}