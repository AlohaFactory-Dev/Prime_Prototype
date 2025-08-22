using System.Collections.Generic;
using System.Numerics;

namespace Aloha.Coconut.Missions
{
    public struct MissionData
    {
        public int id;
        public MissionType type;
        public int var;
        public BigInteger objective;
        public List<Property> rewards;
        public List<IExtendedMissionData> extendedDatas;
        
        public T GetExtendedData<T>() where T : IExtendedMissionData
        {
            foreach (var extendedData in extendedDatas)
            {
                if (extendedData is T data)
                {
                    return data;
                }
            }

            return default;
        }
    }
}