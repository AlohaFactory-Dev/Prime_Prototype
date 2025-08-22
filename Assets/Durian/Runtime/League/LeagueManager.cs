using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    public abstract class LeagueManager<TPlayerData> : IDisposable where TPlayerData : PlayerPublicData
    {
        public IReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
        private ReactiveProperty<bool> _isInitialized = new ReactiveProperty<bool>(false);

        public IReadOnlyReactiveProperty<bool> IsUploadingScore => _isUploadingScore;
        protected ReactiveProperty<bool> _isUploadingScore = new ReactiveProperty<bool>(false);
        
        public IObservable<Unit> OnLeaderboardRefresh => _onLeaderboardRefresh;
        protected Subject<Unit> _onLeaderboardRefresh = new Subject<Unit>();

        public IObservable<Unit> OnSeasonChanged => _onSeasonChanged;
        private Subject<Unit> _onSeasonChanged = new Subject<Unit>();

        public LeagueEnum MyLeague => _leagueDivision.league;
        public int MyRank => _leagueDivision.leaderboard.MyEntry.Rank;
        public int MyPower => _myPublicDataProvider.GetPlayerPublicData().Power;

        public Leaderboard Leaderboard => _leagueDivision.leaderboard;
        public DateTime EndTime => _leagueDivision.endTime;

        protected LeagueDivision _leagueDivision;
        protected readonly ILeagueServer<TPlayerData> _leagueServer;
        protected readonly SaveData _saveData;
        protected readonly IMyPublicDataProvider _myPublicDataProvider;
        protected readonly CompositeDisposable _disposables = new CompositeDisposable();

        private Dictionary<LeagueEnum, List<LeaderboardReward>> _dailyRewards;
        private Dictionary<LeagueEnum, List<LeaderboardReward>> _seasonRewards;
        private string _seasonId;

        private Dictionary<string, string> _saveBotNicknames = new Dictionary<string, string>();

        protected LeagueManager(ILeagueServer<TPlayerData> leagueServer, SaveDataManager saveDataManager, IMyPublicDataProvider myPublicDataProvider)
        {
            _leagueServer = leagueServer;
            _myPublicDataProvider = myPublicDataProvider;
            _saveData = saveDataManager.Get<SaveData>("league_mgr");

            _leagueServer.IsInitialized.Where(i => i)
                .Subscribe(_ => OnServerInitialized().Forget())
                .AddTo(_disposables);
        }

        private async UniTaskVoid OnServerInitialized()
        {
            _seasonId = await _leagueServer.GetCurrentSeasonId();

            if (_seasonId == null)
            {
                _isInitialized.Value = true;
                return;
            }

            await GetLeagueDivision();

            Clock.OnMinuteTick.Subscribe(_ =>
            {
                if (_leagueDivision != null && Clock.NoDebugNow >= _leagueDivision.endTime)
                {
                    _leagueDivision = null;
                    _seasonId = null;
                    _onSeasonChanged.OnNext(Unit.Default);
                }
            }).AddTo(_disposables);

            _isInitialized.Value = true;
        }

        public async UniTask<LeagueDivision> GetLeagueDivision()
        {
            if (_leagueDivision != null) return _leagueDivision;

            if (_seasonId == null)
            {
                _seasonId = await _leagueServer.GetCurrentSeasonId();
                if (_seasonId == null) return null;
            }

            if (!await _leagueServer.IsPlayerJoined(_seasonId))
            {
                _leagueDivision = await _leagueServer.Join(_seasonId);
            }
            else
            {
                _leagueDivision = await _leagueServer.GetLeagueDivision(_seasonId);
            }

            if (_leagueDivision.divisionId != _saveData.divisionId)
            {
                _saveData.divisionId = _leagueDivision.divisionId;
                await OnLeagueSeasonChanged();
            }
            ReplaceLeagueBotNickname();

            return _leagueDivision;
        }

        protected abstract UniTask OnLeagueSeasonChanged();

        private void ReplaceLeagueBotNickname()
        {
            List<string> botNicknames = GetBotNicknames();

            for (int i = 0; i < _leagueDivision.leaderboard.Entries.Count; i++)
            {
                if (_leagueDivision.leaderboard.Entries[i].IsBot)
                {
                    string uid = _leagueDivision.leaderboard.Entries[i].UID;
                    string nickname;

                    if (_saveBotNicknames.ContainsKey(uid))
                    {
                        nickname = _saveBotNicknames[uid];
                    }
                    else
                    {
                        int hash = uid.GetHashCode();
                        hash = hash < 0 ? -hash : hash;
                        nickname = botNicknames[hash % botNicknames.Count];
                        while (_saveBotNicknames.ContainsValue(nickname))
                        {
                            hash++;
                            nickname = botNicknames[hash % botNicknames.Count];
                        }

                        _saveBotNicknames.Add(uid, nickname);
                    }

                    _leagueDivision.leaderboard.Entries[i].OverrideNickname(nickname);
                }
            }
        }

        public async UniTask RefreshLeaderboard()
        {
            await UniTask.WaitWhile(() => IsUploadingScore.Value);
            await _leagueServer.RefreshLeaderboard(_leagueDivision);
            
            _onLeaderboardRefresh.OnNext(Unit.Default);
        }

        public async UniTask<List<LeaderboardReward>> GetDailyRewards(LeagueEnum league)
        {
            if (_dailyRewards == null)
            {
                _dailyRewards = await _leagueServer.GetDailyRewards(_leagueDivision);
            }

            if (_dailyRewards.ContainsKey(league)) return _dailyRewards[league];
            else return _dailyRewards[LeagueEnum.Bronze];
        }

        public async UniTask<List<LeaderboardReward>> GetSeasonRewards(LeagueEnum league)
        {
            if (_seasonRewards == null)
            {
                _seasonRewards = await _leagueServer.GetSeasonRewards(_leagueDivision);
            }

            if (_seasonRewards.ContainsKey(league)) return _seasonRewards[league];
            else return _seasonRewards[LeagueEnum.Bronze];
        }

        public async UniTask<TPlayerData> GetPlayerData(string uid)
        {
            for (int i = 0; i < _leagueDivision.leaderboard.Entries.Count; i++)
            {
                if (_leagueDivision.leaderboard.Entries[i].UID == uid)
                {
                    return await _leagueServer.GetPlayerData(_leagueDivision.leaderboard.Entries[i]);
                }
            }

            Debug.LogError("Player Data Not Found");
            return null;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected class SaveData
        {
            public string divisionId;
        }

        protected List<string> GetBotNicknames()
        {
            List<string> botNicknames = new List<string>();
            botNicknames.Add("Auza");
            botNicknames.Add("Bazel");
            botNicknames.Add("Caelum");
            botNicknames.Add("Dagda");
            botNicknames.Add("Erebus");
            botNicknames.Add("Fenrir");
            botNicknames.Add("Garm");
            botNicknames.Add("Hades");
            botNicknames.Add("Icarus");
            botNicknames.Add("Jormungandr");
            botNicknames.Add("Kraken");
            botNicknames.Add("Loki");
            botNicknames.Add("Medusa");
            botNicknames.Add("Nyx");
            botNicknames.Add("Odin");
            botNicknames.Add("Zeus");
            botNicknames.Add("Thor");
            botNicknames.Add("Ares");
            botNicknames.Add("Xena");
            botNicknames.Add("Ymir");
            botNicknames.Add("Wukong");
            botNicknames.Add("Viper");
            botNicknames.Add("Ursa");
            botNicknames.Add("Titan");
            botNicknames.Add("Sif");
            botNicknames.Add("Raiden");
            botNicknames.Add("Qilin");
            botNicknames.Add("Pluto");
            botNicknames.Add("Orion");
            botNicknames.Add("Ninja");
            botNicknames.Add("Mage");
            botNicknames.Add("Luna");
            botNicknames.Add("Kappa");
            botNicknames.Add("Jedi");
            botNicknames.Add("Iris");
            botNicknames.Add("Hero");
            botNicknames.Add("Gaia");
            botNicknames.Add("Fury");
            botNicknames.Add("Echo");
            botNicknames.Add("Draco");
            botNicknames.Add("Chaos");
            botNicknames.Add("Brute");
            botNicknames.Add("Atlas");
            botNicknames.Add("Axel");
            botNicknames.Add("Blade");
            botNicknames.Add("Claw");
            botNicknames.Add("Doom");
            botNicknames.Add("Edge");
            botNicknames.Add("Fang");
            botNicknames.Add("Grim");
            botNicknames.Add("Hawk");
            botNicknames.Add("Iron");
            botNicknames.Add("Jade");
            botNicknames.Add("Kane");
            botNicknames.Add("Link");
            botNicknames.Add("Mace");
            botNicknames.Add("Nova");
            botNicknames.Add("Onyx");
            botNicknames.Add("Peak");
            botNicknames.Add("Qrow");
            botNicknames.Add("Rage");
            botNicknames.Add("Sage");
            botNicknames.Add("Tank");
            botNicknames.Add("Urus");
            botNicknames.Add("Volt");
            botNicknames.Add("Wolf");
            botNicknames.Add("Xion");
            botNicknames.Add("Yale");
            botNicknames.Add("Zeal");
            botNicknames.Add("Abcd");
            botNicknames.Add("Qwer");
            botNicknames.Add("Asdf");
            botNicknames.Add("Zxcv");
            botNicknames.Add("Wasd");
            botNicknames.Add("Noob");
            botNicknames.Add("Pro");
            botNicknames.Add("Cool");
            botNicknames.Add("Boss");
            botNicknames.Add("King");
            botNicknames.Add("Dude");
            botNicknames.Add("Bruh");
            botNicknames.Add("Meow");
            botNicknames.Add("Woof");
            botNicknames.Add("Rawr");
            botNicknames.Add("Beep");
            botNicknames.Add("Boop");
            botNicknames.Add("Derp");
            botNicknames.Add("Herp");
            botNicknames.Add("Durp");
            botNicknames.Add("Burp");
            botNicknames.Add("Yeet");
            botNicknames.Add("Yolo");
            botNicknames.Add("Swag");
            botNicknames.Add("Doge");
            botNicknames.Add("Pepe");
            botNicknames.Add("UwU");
            botNicknames.Add("OwO");
            botNicknames.Add("Pog");
            botNicknames.Add("Kek");
            botNicknames.Add("Leet");
            botNicknames.Add("Pwn");
            botNicknames.Add("Rekt");
            botNicknames.Add("Git");
            botNicknames.Add("Gud");
            botNicknames.Add("Oof");
            botNicknames.Add("Lol");
            botNicknames.Add("Rofl");
            botNicknames.Add("Lmao");
            botNicknames.Add("Kthx");
            botNicknames.Add("Plox");
            botNicknames.Add("Halp");
            botNicknames.Add("Hax");
            botNicknames.Add("Newb");
            botNicknames.Add("Nub");
            botNicknames.Add("Scrub");
            botNicknames.Add("Pleb");
            botNicknames.Add("Casual");
            botNicknames.Add("Tryhard");
            botNicknames.Add("Smurf");
            botNicknames.Add("Alt");
            botNicknames.Add("Main");
            botNicknames.Add("Senpai");
            botNicknames.Add("Kohai");
            botNicknames.Add("Chan");
            botNicknames.Add("Kun");
            botNicknames.Add("San");
            botNicknames.Add("Sama");
            botNicknames.Add("Dono");
            botNicknames.Add("Tama");
            botNicknames.Add("Hime");
            botNicknames.Add("Neko");
            botNicknames.Add("Inu");
            botNicknames.Add("Kuma");
            botNicknames.Add("Tori");
            botNicknames.Add("Usagi");
            botNicknames.Add("Ryu");
            botNicknames.Add("Kami");
            botNicknames.Add("Oni");
            botNicknames.Add("Yuki");
            botNicknames.Add("Kaze");
            botNicknames.Add("Mizu");
            botNicknames.Add("Hi");
            botNicknames.Add("Tsuki");
            botNicknames.Add("Hoshi");
            botNicknames.Add("Ame");
            botNicknames.Add("Kiri");
            botNicknames.Add("Kumo");
            botNicknames.Add("Yama");
            botNicknames.Add("Mori");
            botNicknames.Add("Kawa");
            botNicknames.Add("Umi");
            botNicknames.Add("Sora");
            botNicknames.Add("Ten");
            botNicknames.Add("Chi");
            botNicknames.Add("Jin");
            botNicknames.Add("Rei");
            botNicknames.Add("Zen");
            botNicknames.Add("Shin");
            botNicknames.Add("Kai");
            botNicknames.Add("Mei");
            botNicknames.Add("Rin");
            botNicknames.Add("Ran");
            botNicknames.Add("Ryo");
            botNicknames.Add("Ken");
            botNicknames.Add("Kai");
            botNicknames.Add("Ryu");
            botNicknames.Add("Gin");
            botNicknames.Add("Kin");
            botNicknames.Add("Zan");
            botNicknames.Add("Dan");
            botNicknames.Add("Bon");
            botNicknames.Add("Don");
            botNicknames.Add("Ton");
            botNicknames.Add("Son");
            botNicknames.Add("Mon");
            botNicknames.Add("Kon");
            botNicknames.Add("Ron");
            botNicknames.Add("Won");
            botNicknames.Add("Yon");
            botNicknames.Add("Hon");
            botNicknames.Add("Pon");
            botNicknames.Add("Lon");
            botNicknames.Add("Jon");
            botNicknames.Add("Fon");
            botNicknames.Add("Von");
            botNicknames.Add("Zon");
            botNicknames.Add("Xon");
            botNicknames.Add("Qon");
            botNicknames.Add("Aon");
            botNicknames.Add("Eon");
            botNicknames.Add("Ion");
            botNicknames.Add("Uon");
            botNicknames.Add("Oon");
            botNicknames.Add("Napoleon");
            botNicknames.Add("Caesar");
            botNicknames.Add("Nero");
            botNicknames.Add("Attila");
            botNicknames.Add("Hannibal");
            botNicknames.Add("Leonidas");
            botNicknames.Add("Xerxes");
            botNicknames.Add("Cyrus");
            botNicknames.Add("Darius");
            botNicknames.Add("Alexander");
            botNicknames.Add("Ramses");
            botNicknames.Add("Cleopatra");
            botNicknames.Add("Spartacus");
            botNicknames.Add("Marcus");
            botNicknames.Add("Julius");
            botNicknames.Add("Augustus");
            botNicknames.Add("Hadrian");
            botNicknames.Add("Trajan");
            botNicknames.Add("Constantine");
            botNicknames.Add("Justinian");
            botNicknames.Add("Charlemagne");
            botNicknames.Add("Genghis");
            botNicknames.Add("Kublai");
            botNicknames.Add("Tamerlane");
            botNicknames.Add("Saladin");
            botNicknames.Add("Richard");
            botNicknames.Add("Arthur");
            botNicknames.Add("Merlin");
            botNicknames.Add("Lancelot");
            botNicknames.Add("Galahad");
            botNicknames.Add("Percival");
            botNicknames.Add("Gawain");
            botNicknames.Add("Tristan");
            botNicknames.Add("Isolde");
            botNicknames.Add("Morgan");
            botNicknames.Add("Guinevere");
            botNicknames.Add("Mordred");
            botNicknames.Add("Kay");
            botNicknames.Add("Bedivere");
            botNicknames.Add("Bors");
            botNicknames.Add("Gareth");
            botNicknames.Add("Lamorak");
            botNicknames.Add("Pellinore");
            botNicknames.Add("Uther");
            botNicknames.Add("Igraine");
            botNicknames.Add("Gorlois");
            botNicknames.Add("Vortigern");
            botNicknames.Add("Ambrosius");
            botNicknames.Add("Aurelius");
            botNicknames.Add("Constantine");
            botNicknames.Add("Urien");
            botNicknames.Add("Lot");
            botNicknames.Add("Gawain");
            botNicknames.Add("Agravain");
            botNicknames.Add("Gaheris");
            botNicknames.Add("Gareth");
            botNicknames.Add("Morgause");
            botNicknames.Add("Elaine");
            botNicknames.Add("Viviane");
            botNicknames.Add("Nimue");
            botNicknames.Add("Morgana");
            botNicknames.Add("Igraine");
            botNicknames.Add("Ygraine");
            botNicknames.Add("Ector");
            botNicknames.Add("Cai");
            botNicknames.Add("Bedwyr");
            botNicknames.Add("Gwalchmai");
            botNicknames.Add("Medrawd");
            botNicknames.Add("Gwenhwyfar");
            botNicknames.Add("Myrddin");
            botNicknames.Add("Taliesin");
            botNicknames.Add("Peredur");
            botNicknames.Add("Owain");
            botNicknames.Add("Geraint");
            botNicknames.Add("Enid");
            botNicknames.Add("Culhwch");
            botNicknames.Add("Olwen");
            botNicknames.Add("Ysbaddaden");
            botNicknames.Add("Goreu");
            botNicknames.Add("Kilydd");
            botNicknames.Add("Goleuddydd");
            botNicknames.Add("Cilydd");
            botNicknames.Add("Penarddun");
            botNicknames.Add("Rhiannon");
            botNicknames.Add("Pwyll");
            botNicknames.Add("Pryderi");
            botNicknames.Add("Manawyddan");
            botNicknames.Add("Math");
            botNicknames.Add("Gwydion");
            botNicknames.Add("Arianrhod");
            botNicknames.Add("Lleu");
            botNicknames.Add("Blodeuwedd");
            botNicknames.Add("Gronw");
            botNicknames.Add("Llyr");
            botNicknames.Add("Bran");
            botNicknames.Add("Branwen");
            botNicknames.Add("Efnysien");
            botNicknames.Add("Nisien");
            botNicknames.Add("Matholwch");
            botNicknames.Add("Bendigeidfran");
            botNicknames.Add("Caswallawn");
            botNicknames.Add("Lludd");
            botNicknames.Add("Llefelys");
            botNicknames.Add("Gwyn");
            botNicknames.Add("Gwythyr");
            botNicknames.Add("Creiddylad");
            botNicknames.Add("Cordelia");
            botNicknames.Add("Hafgan");
            botNicknames.Add("Arawn");
            botNicknames.Add("Annwn");
            botNicknames.Add("Pwyll");
            botNicknames.Add("Dyfed");
            botNicknames.Add("Teyrnon");
            botNicknames.Add("Rhiannon");
            botNicknames.Add("Pryderi");
            botNicknames.Add("Cigfa");
            botNicknames.Add("Manawydan");
            botNicknames.Add("Llwyd");
            botNicknames.Add("Gwawl");
            botNicknames.Add("Teirnon");
            botNicknames.Add("Pendaran");
            botNicknames.Add("Rhiannon");
            botNicknames.Add("Pwyll");
            botNicknames.Add("Dyfed");
            botNicknames.Add("Pryderi");
            botNicknames.Add("Cigfa");
            botNicknames.Add("Manawydan");
            botNicknames.Add("Llwyd");
            botNicknames.Add("Gwawl");
            botNicknames.Add("Teirnon");
            botNicknames.Add("Pendaran");

            return botNicknames;
        }
    }
}