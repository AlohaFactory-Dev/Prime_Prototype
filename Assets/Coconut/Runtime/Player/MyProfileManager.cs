using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut.Player
{
    public enum NicknameChangeResultFailureType
    {
        InvalidNickname,
        NotAffordable,
    }

    public struct NicknameChangeResult
    {
        public bool isSuccess;
        public NicknameChangeResultFailureType failureType;
        public string failureMessage;
    }

    public class MyProfileManager
    {
        public GrowthLevel GrowthLevel { get; }
        public bool IsNickChangeFree => _saveData.nicknameChangeCount < _config.FreeNicknameChangeCount;
        public string Name => _saveData.myProfile.Nickname;
        public string Cuid => _saveData.myProfile.UID;
        public int ProfileImageId => _saveData.myProfile.ProfileImageId;
        public int ProfileFrameId => _saveData.myProfile.ProfileFrameId;

        private readonly MyProfileConfig _config;
        private readonly SaveData _saveData;
        private readonly PropertyManager _propertyManager;
        private readonly IServerNicknameSetter _serverNicknameSetter;
        private readonly INicknameFilter _nicknameFilter;

        public IObservable<Unit> OnNicknameChanged => _onNicknameChanged;
        private Subject<Unit> _onNicknameChanged = new Subject<Unit>();
        
        public IObservable<Unit> OnProfileImageChanged => _onProfileImageChanged;
        private Subject<Unit> _onProfileImageChanged = new Subject<Unit>();

        public MyProfileManager(SaveDataManager saveDataManager, GrowthLevel.Factory growthLevelFactory,
            PropertyManager propertyManager,
            [InjectOptional] IMyUIDProvider myUidProvider, [InjectOptional] INicknameFilter nicknameFilter,
            [InjectOptional] IServerNicknameSetter serverNicknameSetter)
        {
            _config = CoconutConfig.Get<MyProfileConfig>();
            _saveData = saveDataManager.Get<SaveData>("my_profile_manager");
            _propertyManager = propertyManager;
            _serverNicknameSetter = serverNicknameSetter;
            _nicknameFilter = nicknameFilter ?? new DefaultNicknameFilter(_config);

            // 프로필 생성
            if (_saveData.myProfile == null)
            {
                var uid = myUidProvider?.MyUID ?? $"{Guid.NewGuid().ToString()}";
                _saveData.myProfile = new PlayerProfile($"{_config.DefaultNicknamePrefix}{uid.Substring(0, 4)}", uid,
                    _config.DefaultProfileImageId, _config.DefaultProfileFrameId, new GrowthLevel.SaveData());
            }

            // 어떤 이유에서든지 UID 정보가 일치하지 않으면, 현재의 UID로 덮어씌움
            if (myUidProvider != null && _saveData.myProfile.UID != myUidProvider.MyUID)
            {
                _saveData.myProfile.UID = myUidProvider.MyUID;
            }

            // GrowthLevel 생성
            var growthLevelEntries = TableManager.Get<GrowthLevel.TableEntry>("player_level_exp");
            growthLevelEntries.Sort((a, b) => a.level.CompareTo(b.level));
            for (var i = 0; i < growthLevelEntries.Count - 1; i++)
            {
                Assert.IsTrue(growthLevelEntries[i].level + 1 == growthLevelEntries[i + 1].level,
                    $"Level entries must be continuous. Missing level: {growthLevelEntries[i].level + 1}");
            }

            var requiredExp = growthLevelEntries.ConvertAll(entry => entry.req_exp);
            GrowthLevel = growthLevelFactory.Create(_config.LevelExpPropertyType, requiredExp,
                _saveData.myProfile.levelSaveData);

            if (_serverNicknameSetter != null)
            {
                _serverNicknameSetter.Nickname = Name;
            }
        }

        public UniTask<NicknameFilterResult> CheckNickname(string nickname)
        {
            return _nicknameFilter.Check(nickname);
        }

        public async UniTask<NicknameChangeResult> ChangeNickname(string nickname)
        {
            var checkResult = await CheckNickname(nickname);
            if (!checkResult.isValid)
            {
                return new NicknameChangeResult
                {
                    isSuccess = false,
                    failureType = NicknameChangeResultFailureType.InvalidNickname,
                    failureMessage = checkResult.failureMessage
                };
            }

            if (!IsNickChangeFree)
            {
                if (!_propertyManager.TryUse(_config.NicknameChangeCost, _config.NickChangeActionName))
                {
                    checkResult.isValid = false;
                    return new NicknameChangeResult
                    {
                        isSuccess = false,
                        failureType = NicknameChangeResultFailureType.NotAffordable,
                    };
                }
            }

            _saveData.myProfile.Nickname = nickname;
            _saveData.nicknameChangeCount++;
            if (_serverNicknameSetter != null) _serverNicknameSetter.Nickname = Name;
            _onNicknameChanged.OnNext(Unit.Default);
            return new NicknameChangeResult {isSuccess = true};
        }

        public void ChangeProfileImage(int newProfileImageId)
        {
            _saveData.myProfile.ProfileImageId = newProfileImageId;
            
            _onProfileImageChanged.OnNext(Unit.Default);
        }

        public void ChangeProfileFrame(int newProfileFrameId)
        {
            _saveData.myProfile.ProfileFrameId = newProfileFrameId;
            
            _onProfileImageChanged.OnNext(Unit.Default);
        }

        public class SaveData
        {
            public PlayerProfile myProfile;
            public int nicknameChangeCount;
        }
    }
}