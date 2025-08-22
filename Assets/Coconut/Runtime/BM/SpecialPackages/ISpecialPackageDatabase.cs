using System.Collections.Generic;

namespace Aloha.Coconut
{
    public struct SpecialPackageData
    {
        [CSVColumn] public int id;
        [CSVColumn] public string iapProductId;
        [CSVColumn] public int limit;
        [CSVColumn] public int condition;
    }
    
    public interface ISpecialPackageDatabase
    {
        public List<SpecialPackageData> GetSpecialPackageDatas();
    }
}
