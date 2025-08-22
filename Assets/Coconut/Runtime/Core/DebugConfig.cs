using System;
using UnityEngine;

namespace Aloha.Coconut
{
    [Serializable]
    public class DebugConfig
    {
        public const string PATH = "debug_config.json";

        public static bool UseDebug => _instance.useDebug;
        public static bool TutorialOn => _instance.tutorialOn;
        public static bool UseDevServer => _instance.useDevServer;
    
        private static DebugConfig _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            var decodedJson = StreamingAssetReader.ReadText(PATH);
            _instance = JsonUtility.FromJson<DebugConfig>(decodedJson);
        }

        public bool useDebug;
        public bool tutorialOn;
        public bool useDevServer;
    }
}