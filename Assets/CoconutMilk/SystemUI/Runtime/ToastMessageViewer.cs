using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aloha.CoconutMilk
{
    public class ToastMessageViewer : MonoBehaviour, IToastMessageViewer
    {
        [FormerlySerializedAs("systemMessagePrefab")] [SerializeField] private ToastMessageBox toastMessagePrefab;
    
        private List<ToastMessageBox> _spawnedMessages = new List<ToastMessageBox>();
        private List<ToastMessageBox> _pool = new List<ToastMessageBox>();

        public void Show(string message, float duration)
        {
            foreach (var systemMessageBox in _spawnedMessages)
            {
                systemMessageBox.IncreaseOffset();
            }
        
            var newSystemMessage = GetNewMessageBox();
            newSystemMessage.transform.SetParent(transform, false);
            newSystemMessage.transform.localPosition = new Vector3(0, 0, 0);
            ((RectTransform)newSystemMessage.transform).sizeDelta = 
                new Vector2(0, ((RectTransform)newSystemMessage.transform).sizeDelta.y);
        
            newSystemMessage.Show(message, duration);
            newSystemMessage.OnComplete.First()
                .Subscribe(_ =>
                {
                    _spawnedMessages.Remove(newSystemMessage);
                    newSystemMessage.gameObject.SetActive(false);
                });
        
            _spawnedMessages.Insert(0, newSystemMessage);
        }

        private ToastMessageBox GetNewMessageBox()
        {
            if (_pool.Count == 0)
            {
                return Instantiate(toastMessagePrefab, transform);
            }
            else
            {
                var messageBox = _pool[0];
                _pool.RemoveAt(0);
                messageBox.gameObject.SetActive(true);
                return messageBox;
            }
        }
    }
}