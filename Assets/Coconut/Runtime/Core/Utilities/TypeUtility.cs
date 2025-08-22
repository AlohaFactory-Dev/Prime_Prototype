using System;
using System.Collections.Generic;

namespace Aloha.Coconut
{
    public static class TypeUtility
    {
        private static Dictionary<Type, List<Type>> _cachedDerivedTypes = new Dictionary<Type, List<Type>>();

        public static void CacheDerivedTypes(params Type[] parentTypes)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var parentType in parentTypes)
            {
                if (_cachedDerivedTypes.ContainsKey(parentType))
                {
                    continue;
                }
                
                _cachedDerivedTypes[parentType] = new List<Type>();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (parentType.IsAssignableFrom(type) && parentType != type)
                            {
                                _cachedDerivedTypes[parentType].Add(type);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Some assemblies might not be accessible, skip them.
                    }
                }
            }
        }
        
        public static List<Type> GetDerivedTypes(Type parentType)
        {
            if (!_cachedDerivedTypes.ContainsKey(parentType))
            {
                CacheDerivedTypes(parentType);
            }

            return _cachedDerivedTypes[parentType];
        }

        public static Type GetType(Type parentType, string typeFullName)
        {
            foreach (var type in GetDerivedTypes(parentType))
            {
                if (type.FullName == typeFullName)
                {
                    return type;
                }
            }
            
            return null;
        }
    }
}
