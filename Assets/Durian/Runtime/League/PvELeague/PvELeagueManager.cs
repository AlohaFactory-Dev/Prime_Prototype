using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Durian
{
    public class PvELeagueManager<TPlayerData> : LeagueManager<TPlayerData> where TPlayerData : PlayerPublicData
    {
        public static void InstallWithMockServer(DiContainer container)
        {
            Assert.IsTrue(container.HasBinding<ILeagueBotDataGenerator<TPlayerData>>());
            Assert.IsTrue(container.HasBinding<IMyPublicDataProvider>());
            Assert.IsTrue(container.HasBinding<SaveDataManager>());
            Assert.IsTrue(container.HasBinding<AuthManager>());
            Assert.IsTrue(container.HasBinding<LeaderboardManager>());

            container.BindInterfacesAndSelfTo<PvELeagueManager<TPlayerData>>().AsSingle();
            container.BindInterfacesTo<MockLeagueServer<TPlayerData>>().AsSingle();
        }

        public static void InstallWithDurianServer(DiContainer container, string leagueGroupId)
        {
            Assert.IsTrue(container.HasBinding<ILeagueBotDataGenerator<TPlayerData>>());
            Assert.IsTrue(container.HasBinding<IMyPublicDataProvider>());
            Assert.IsTrue(container.HasBinding<SaveDataManager>());
            Assert.IsTrue(container.HasBinding<AuthManager>());
            Assert.IsTrue(container.HasBinding<DurianConfig>());
            Assert.IsTrue(container.HasBinding<OtherPlayerDataManager>());
            Assert.IsTrue(container.HasBinding<LeaderboardManager>());

            container.BindInterfacesAndSelfTo<PvELeagueManager<TPlayerData>>().AsSingle();
            container.BindInterfacesTo<DurianLeagueServer<TPlayerData>>().AsSingle();
            container.BindInstance(leagueGroupId).WithId("league_group_id");
        }

        public PvELeagueManager(ILeagueServer<TPlayerData> leagueServer, SaveDataManager saveDataManager, IMyPublicDataProvider myPublicDataProvider)
            : base(leagueServer, saveDataManager, myPublicDataProvider)
        {
        }

        protected override async UniTask OnLeagueSeasonChanged()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask AddScore(int score)
        {
            _isUploadingScore.Value = true;
            await _leagueServer.UploadScoreDeltas(_leagueDivision, new PlayerScoreDelta(_leagueServer.PlayerUID, score));
            _isUploadingScore.Value = false;

            await RefreshLeaderboard();
        }
    }
}
