using System;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace Aloha.Coconut.Missions
{
    public class MissionFactory
    {
        private readonly DiContainer _container;
        private readonly PropertyManager _propertyManager;
        
        private readonly Dictionary<MissionType, Type> _missionTypes = new();

        public MissionFactory(DiContainer container, PropertyManager propertyManager)
        {
            _container = container;
            _propertyManager = propertyManager;

            //find every Mission subtypes with MissionAttribute, and cache them to _missionTypes
            var missionType = typeof(Mission);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsExcludedAssembly(assembly)) continue;
                
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsSubclassOf(missionType)) continue;

                    var attributes = type.GetCustomAttributes(typeof(MissionAttribute), true);
                    if (attributes.Length > 0)
                    {
                        var missionAttribute = attributes[0] as MissionAttribute;
                        _missionTypes[missionAttribute.MissionType] = type;
                    }
                }
            }
        }
        
        private bool IsExcludedAssembly(Assembly assembly)
        {
            return assembly.FullName.StartsWith("Unity") 
                   || assembly.FullName.StartsWith("System") 
                   || assembly.FullName.StartsWith("mscorlib");
        }

        public Mission Create(MissionData missionData, Mission.SaveData missionSaveData = null)
        {
            if (_missionTypes.TryGetValue(missionData.type, out var type))
            {
                return _container.InstantiateExplicit(type, new List<TypeValuePair>
                {
                    new (typeof(MissionData), missionData),
                    new (typeof(PropertyManager), _propertyManager),
                    new (typeof(Mission.SaveData), missionSaveData)
                }) as Mission;
            }

            return null;
        }
    }
}