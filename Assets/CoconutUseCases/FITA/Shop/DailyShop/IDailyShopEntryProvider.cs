using System.Collections.Generic;

public interface IDailyShopEntryProvider
{
    public List<DailyShopEntry> GetDailyShopRawEntries();
}

public class DefaultDailyShopEntryProvider : IDailyShopEntryProvider
{
    public List<DailyShopEntry> GetDailyShopRawEntries()
    {
        return CSVReader.ReadResource<DailyShopEntry>("daily_shop");
    }
}