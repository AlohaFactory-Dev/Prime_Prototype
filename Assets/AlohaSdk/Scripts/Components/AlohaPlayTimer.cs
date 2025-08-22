using UnityEngine;

namespace Aloha.Sdk
{
    public class AlohaPlayTimer : MonoBehaviour
    {
        private bool _skipFrame;
        private const string KEY_TIMER = "alohaSdk.playTimer";
        
        internal void Initialize()
        {
            AlohaSdk.Context.PlayTime = PlayerPrefs.GetFloat(KEY_TIMER, 0f);
            AlohaSdk.Context.SessionPlayTime = 0;
        }

        void Update()
        {
            if (_skipFrame)
            {
                _skipFrame = false;
            }
            else
            {
                AlohaSdk.Context.PlayTime += Time.unscaledDeltaTime;   
                AlohaSdk.Context.SessionPlayTime += Time.unscaledDeltaTime;   
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveCurrentTime();
            }
            else
            {
                _skipFrame = true;
            }
        }

        private void SaveCurrentTime()
        {
            PlayerPrefs.SetFloat(KEY_TIMER, AlohaSdk.Context.PlayTime);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentTime();
            }
            else
            {
                _skipFrame = true;
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentTime();
        }
    }
}
