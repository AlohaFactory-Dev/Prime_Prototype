using System;
using System.Collections.Generic;
using Aloha.Coconut;
using UnityEngine;

namespace Aloha.Durian
{
    [CreateAssetMenu(menuName = "Coconut/Config/DurainConfig")]
    public class DurianConfig : CoconutConfig
    {
        public string ServerUrl => DebugConfig.UseDevServer ? devServerUrl : prodServerUrl;
        
        [SerializeField] private string prodServerUrl;
        [SerializeField] private string devServerUrl;

        public int statusCheckInterval = 5;
        public int versionCheckInterval = 5;
        
        [Header("Mail")]
        public int mailPollingInterval = 60;
        public int mailPlayerActionId = 0;
        public string mailRedDotPath = "Lobby/Main/Menu/Mail";
        
        [Header("Announcement")]
        public int announcementPollingInterval = 300;
        public string announcementRedDotPath = "Lobby/Main/Menu/Announcement";
        
        [Header("Leaderboard")]
        public List<LeaderboardId> leaderboardIds;
        
        [Header("Auth")]
        public bool autoSignInAsGuest = true;
        public string gcpWebClientId;
#if UNITY_EDITOR || UNITY_STANDALONE
        public string gcpWebClientSecret;
#endif

        [Header("Others")]
        public string publicDataSavePath;
        
        public void Reset()
        {
            prodServerUrl = "";
            devServerUrl = "";
            autoSignInAsGuest = true;
            mailPollingInterval = 60;
            mailPlayerActionId = 0;
            mailRedDotPath = "Lobby/Main/Menu/Mail";
            announcementPollingInterval = 300;
            announcementRedDotPath = "Lobby/Main/Menu/Announcement";
            gcpWebClientId = "";
            publicDataSavePath = "";
#if UNITY_EDITOR || UNITY_STANDALONE
            gcpWebClientSecret = "";
#endif
        }

        [Serializable]
        public struct LeaderboardId
        {
            public string name;
            public string Id => DebugConfig.UseDevServer ? devId : prodId;
            [SerializeField] private string devId;
            [SerializeField] private string prodId;
        }
    }
}
