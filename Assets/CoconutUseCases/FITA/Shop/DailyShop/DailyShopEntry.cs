public struct DailyShopEntry 
{
    [CSVColumn] public int id;
    [CSVColumn] public string goodsType;
    [CSVColumn] public int goodsAmount;
    [CSVColumn] public string priceType;
    [CSVColumn] public int priceAmount;
    [CSVColumn] public int weight;
}