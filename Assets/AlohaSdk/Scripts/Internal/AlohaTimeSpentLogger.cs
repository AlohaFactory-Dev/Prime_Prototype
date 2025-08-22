using System.Collections;
using UnityEngine;

namespace Aloha.Sdk
{
    internal class AlohaTimeSpentLogger
    {
        private WaitForSeconds _waitForSeconds;
        
        private int MinutesPassed
        {
            get => PlayerPrefs.GetInt("alohaSdk.minutes_passed", 0);
            set => PlayerPrefs.SetInt("alohaSdk.minutes_passed", value);
        }
        private float LastTimeSpent
        {
            get => PlayerPrefs.GetFloat("alohaSdk.last_time_spent", 0f);
            set => PlayerPrefs.SetFloat("alohaSdk.last_time_spent", value);
        }

        public void Initialize()
        {
            AlohaSdk.StartCoroutineByInstance(StartTimeSpentLog());
        }
        
        private IEnumerator StartTimeSpentLog()
        {
            //게임 처음 시작 오차
            _waitForSeconds = new WaitForSeconds(LastTimeSpent + 60f - AlohaSdk.Context.PlayTime);
            
            yield return _waitForSeconds;
            LogTimeSpent();
            
            _waitForSeconds = new WaitForSeconds(60f);
            
            while (true)
            {
                yield return _waitForSeconds;
                LogTimeSpent();
            }
        }

        private void LogTimeSpent()
        {
            MinutesPassed++;
            AlohaSdk.Event.LogTimeSpent(MinutesPassed);
            LastTimeSpent = AlohaSdk.Context.PlayTime;
        }
    }
}
