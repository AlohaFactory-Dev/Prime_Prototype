using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aloha.Coconut
{
    public static class EventBus
    {
        private static Dictionary<Type, object> _subjectGroups = new Dictionary<Type, object>();

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        public static void InitializeOnEnterPlayMode()
        {
            _subjectGroups.Clear();
        }
#endif

        public static Subject<T> GetSubject<T>() where T : Event
        {
            var type = typeof(T);
            if (_subjectGroups.ContainsKey(type) == false)
            {
                _subjectGroups.Add(type, new Subject<T>());
            }

            return (Subject<T>)_subjectGroups[type];
        }

        public static void Broadcast<T>(T e) where T : Event
        {
            if (_subjectGroups.TryGetValue(typeof(T), out var handlerGroup))
            {
                ((Subject<T>)handlerGroup).OnNext(e);
            }
            else
            {
                Debug.LogWarning($"No subscriber for event {e.GetType()}");
            }

            e.OnBroadcastComplete();
        }

        public static async Task<T> Await<T>() where T : Event
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            GetSubject<T>().First().Subscribe(ev => taskCompletionSource.SetResult(ev));
            return await taskCompletionSource.Task;
        }

        public static IEnumerator WaitForEvent<T>() where T : Event
        {
            yield return GetSubject<T>().First().ToYieldInstruction();
        }

        public static IEnumerator WaitForEventWhere<T>(Func<T, bool> predicate) where T : Event
        {
            yield return GetSubject<T>().Where(predicate).First().ToYieldInstruction();
        }
    }
}