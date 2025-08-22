using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Aloha.Coconut
{
    /// <summary>
    /// Coconut에 사용하는 설정 파일들의 기본 클래스<br/>
    /// CreateAssetMenu의 경로는 Coconut/Config/{name}으로 설정
    /// </summary>
    public abstract class CoconutConfig: ScriptableObject
    {
        private static IList<CoconutConfig> _configs;
        
        public static T Get<T>() where T : CoconutConfig
        {
            if (_configs == null)
            {
                _configs = Addressables.LoadAssetsAsync<CoconutConfig>("coconut.configs", c =>
                {
                    Debug.Log($"Loaded config {c.GetType()}");
                    c.Validate();
                }).WaitForCompletion();
            }
            
            foreach (var config in _configs)
            {
                if (config is T t)
                {
                    return t;
                }
            }
            
            Debug.LogError($"Config of type {typeof(T)} not found");
            return null;
        }
        
        [RuntimeInitializeOnLoadMethod]
        public static void RuntimeInitializeOnLoad()
        {
            _configs = null;
        }

        // 테스트 목적 등을 위해 외부에서 Config를 생성할 수 있도록 하는 메소드
        public static T Create<T>() where T : CoconutConfig
        {
            return CreateInstance<T>();
        }
        
        protected virtual void Validate() {}
    }
}