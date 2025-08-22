using System;
using System.Collections.Generic;
using Aloha.Coconut.Missions;
using UniRx;

namespace Aloha.Coconut.Achievements
{
    public class AchievementGroup : IDisposable
    {
        public int GroupId => _groupId;
        public Mission CurrentMission => _currentMission;
        public bool IsCompleted => _saveData.index == _achievementDatas.Count - 1 && _currentMission.IsRewardsClaimed;
        
        private readonly int _groupId;
        private readonly List<AchievementData> _achievementDatas;

        private readonly SaveData _saveData;
        private readonly MissionFactory _missionFactory;
        private Mission _currentMission;

        internal AchievementGroup(int groupId, List<AchievementData> achievementDatas, SaveData saveData, 
            MissionFactory missionFactory)
        {
            _groupId = groupId;
            _achievementDatas = achievementDatas;
            _saveData = saveData;
            _missionFactory = missionFactory;

            // AchievementGroup이 완료됐었으나 새로운 업적이 추가된 경우, 세이브데이터를 바꿔서 추가된 업적을 시작해줌
            if (_saveData.index < _achievementDatas.Count - 1 && _saveData.mission.isRewardsClaimed)
            {
                _saveData.index++;
                _saveData.mission = null;
            }

            StartCurrentMission();
        }

        private void StartCurrentMission()
        {
            _currentMission?.Dispose();

            _currentMission = _missionFactory.Create(_achievementDatas[_saveData.index].MissionData, _saveData.mission);
            _currentMission.Start();
            _currentMission.Progress.Subscribe(v =>
            {
                RedDot.SetNotified($"Achievement/group_{_groupId}", _currentMission.IsRewardsClaimable);
            });
        }

        public void Complete(PlayerAction playerAction)
        {
            if (CurrentMission.IsRewardsClaimable)
            {
                CurrentMission.Claim(playerAction);
                if(_saveData.index < _achievementDatas.Count - 1)
                {
                    _saveData.index++;
                    _saveData.mission = null;
                    StartCurrentMission();
                }   
            }
        }

        public void Dispose()
        {
            _currentMission?.Dispose();
        }

        public class SaveData
        {
            public int index;
            public Mission.SaveData mission;
        }
    }
}