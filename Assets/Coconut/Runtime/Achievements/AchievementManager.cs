using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut.Missions;
using Zenject;

namespace Aloha.Coconut.Achievements
{
    public class AchievementManager : IDisposable
    {
        public static void InstallTo(DiContainer container)
        {
            container.Bind<AchievementManager>().FromSubContainerResolve()
                .ByMethod(subContainer =>
                {
                    subContainer.BindInterfacesTo<DefaultAchievementDataProvider>().AsSingle().NonLazy();
                    subContainer.BindInterfacesAndSelfTo<AchievementManager>().AsSingle().NonLazy();
                }).AsSingle().NonLazy();;
        }
        
        private Dictionary<int, AchievementGroup> _groups = new();
        private SaveData _saveData;
        
        public AchievementManager(SaveDataManager saveDataManager, IAchievementDataProvider dataProvider,
            MissionFactory missionFactory)
        {
            _saveData = saveDataManager.Get<SaveData>("achievement_manager");

            var achievementDatasByGroup = dataProvider.GetAchievementDatas().GroupBy(a => a.group)
                .ToDictionary(g => g.Key, g => g.OrderBy(a => a.order).ToList());
            
            foreach (var (groupId, datas) in achievementDatasByGroup)
            {
                if (!_saveData.groupSaveDatas.ContainsKey(groupId))
                {
                    _saveData.groupSaveDatas.Add(groupId, new AchievementGroup.SaveData());
                }
                
                var saveData = _saveData.groupSaveDatas[groupId];
                if (saveData.mission == null) saveData.mission = new Mission.SaveData();

                _groups.Add(groupId, new AchievementGroup(groupId, datas, saveData, missionFactory));
            }
        }
        
        public List<AchievementGroup> GetGroups()
        {
            return _groups.Values.ToList();
        }

        public void Dispose()
        {
            foreach (var group in _groups.Values)
            {
                group.Dispose();
            }
        }

        private class SaveData
        {
            public Dictionary<int, AchievementGroup.SaveData> groupSaveDatas = new();
        }
    }
}
