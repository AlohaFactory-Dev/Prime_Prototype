using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aloha.Coconut
{
    [CreateAssetMenu(menuName = "Coconut/Config/PropertyTypeConfig")]
    public class PropertyTypeConfig : CoconutConfig
    {
        public List<string> propertyTypeGroups;
        
#if UNITY_EDITOR
        [InfoBox("PropertyTypeGroups와 property_types 테이블을 기반으로 enum 생성")]
        [Button]
        public void GeneratePropertyTypeEnums()
        {
            EnumGenerator<string, PropertyTypeGroup>.New()
                .SetSources(propertyTypeGroups)
                .SetFilePath("Assets/Coconut/Runtime/Core/Property/PropertyTypeGroup.cs")
                .SetGeneratorClass(nameof(PropertyTypeConfig))
                .SetNameSelector(s => s)
                .SetIntValueSelector(StringToIntHash)
                .Generate();
            
            PropertyType.Load();
            EnumGenerator<PropertyType, PropertyTypeAlias>.New()
                .SetSources(PropertyType.GetAll())
                .SetFilePath("Assets/Coconut/Runtime/Core/Property/PropertyTypeAlias.cs")
                .SetGeneratorClass(nameof(PropertyTypeConfig))
                .SetNameSelector(p => p.alias)
                .SetIntValueSelector(p => StringToIntHash(p.alias))
                .Generate();
        }

        private static int StringToIntHash(string alias)
        {
            if (alias == "Default") return 0;
            
            var hash = 0;
            for (var i = 0; i < alias.Length; i++)
            {
                hash = (hash << 5) - hash + alias[i];
            }

            return hash;
        }
#endif
    }
}
