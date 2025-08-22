using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Aloha.Coconut
{
    public static class RedDot
    {
        private static Dictionary<int, RedDotNode> _roots = new Dictionary<int, RedDotNode>();
        private static Dictionary<int, bool> _isTurnedOff = new Dictionary<int, bool>();

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            _roots.Clear();
            _isTurnedOff.Clear();
        }

        public static void SetNotified(string path, bool notified, int channel = 1)
        {
            if (IsTurnedOff(channel)) return;
            Get(path, channel).SetNotified(notified);
        }
        
        public static bool GetNotified(string path, int channel = 1)
        {
            return Get(path, channel).Notified.Value;
        }
    
        private static bool IsTurnedOff(int channel)
        {
            return _isTurnedOff.ContainsKey(channel) && _isTurnedOff[channel];
        }

        private static RedDotNode Get(string path, int channel)
        {
            var pathList = new List<string>(path.Split('/'));
            return GetRoot(channel).Get(pathList);
        }

        private static RedDotNode GetRoot(int channel)
        {
            if (!_roots.ContainsKey(channel)) _roots[channel] = new RedDotNode("root");
            return _roots[channel];
        }

        public static IDisposable AddListener(string path, Action<bool> onNotified, int channel = 1)
        {
            return Get(path, channel).Notified.Subscribe(onNotified);
        }

        public static void TurnOff(int channel = 1)
        {
            _roots[channel].TurnOffAll();
            _isTurnedOff[channel] = true;
        }
    }
}