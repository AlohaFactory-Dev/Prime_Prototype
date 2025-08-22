using System;
using Aloha.Coconut;
using UniRx;
using UnityEngine;

namespace Aloha.CoconutMilk
{
    public class UIRedDot : MonoBehaviour
    {
        [SerializeField] private string path;
        [SerializeField] private bool dontSetInactive;

        private IDisposable _listener;
        private bool _isPathSet;
        private bool _isNotified;

        void Awake()
        {
            transform.localScale = Vector3.zero;
            if (!dontSetInactive && !_isPathSet) gameObject.SetActive(false);
            if (string.IsNullOrEmpty(path) == false) SetPath(path);
        }

        private void OnEnable()
        {
            if (_isNotified && transform.localScale.x < 1f)
            {
                transform.localScale = Vector3.one;
            }
        }

        public void SetPath(string newPath)
        {
            if (string.IsNullOrEmpty(newPath) || (_isPathSet && path.Equals(newPath))) return;

            _isPathSet = true;
            path = newPath;
            
            _listener?.Dispose();
            _listener = RedDot.AddListener(path, isNotified =>
            {
                _isNotified = isNotified;
                if (isNotified)
                {
                    gameObject.SetActive(true);
                    transform.localScale = Vector3.one;
                }
                else
                {
                    if(!dontSetInactive) gameObject.SetActive(false);
                    transform.localScale = Vector3.zero;
                }
            }).AddTo(this);
        }
    }
}