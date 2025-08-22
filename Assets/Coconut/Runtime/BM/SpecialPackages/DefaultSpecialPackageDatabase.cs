using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class DefaultSpecialPackageDatabase : ISpecialPackageDatabase
    {
        public List<SpecialPackageData> GetSpecialPackageDatas()
        {
            return TableManager.Get<SpecialPackageData>("bm_special_packages");
        }
    }
}
