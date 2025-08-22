using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Aloha.Coconut.Launcher
{
    public class GameSceneManager
    {
        private List<AsyncOperationHandle<SceneInstance>> _loadedSceneHandles = new();
        private bool _isBusy;
        private IProgress<float> _progress;
    
        public void LinkProgress(IProgress<float> progress)
        {
            _progress = progress;
        }
    
        public async UniTask LoadSceneAsync(AssetReference sceneAssetReference, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            await LoadSceneAsync(sceneAssetReference.LoadSceneAsync(loadSceneMode));
        }
    
        public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            await LoadSceneAsync(Addressables.LoadSceneAsync(sceneName, loadSceneMode));
        }

        private async UniTask LoadSceneAsync(AsyncOperationHandle<SceneInstance> operationHandle)
        {
            _isBusy = true;
            _progress?.Report(0f);
            var scene = await operationHandle.ToUniTask(_progress);
            await scene.ActivateAsync();
            SceneManager.SetActiveScene(scene.Scene);
            _loadedSceneHandles.Add(operationHandle);
            _progress?.Report(1f);
            _isBusy = false;
        }
    
        public async UniTask UnloadSceneAsync(string sceneName)
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            var index = _loadedSceneHandles.FindIndex(o => o.Result.Scene.name == sceneName);
            if (index >= 0)
            {
                await UnloadSceneAsync(_loadedSceneHandles[index]);
            }
        }

        public async UniTask UnloadLastSceneAsync()
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            if (_loadedSceneHandles.Count > 0)
            {
                await UnloadSceneAsync(_loadedSceneHandles[^1]);
            }
        }
        
        private async UniTask UnloadSceneAsync(AsyncOperationHandle<SceneInstance> operationHandle)
        {
            _isBusy = true;
            _progress?.Report(0f);
            await Addressables.UnloadSceneAsync(operationHandle).ToUniTask(_progress);
            _loadedSceneHandles.Remove(operationHandle);
            _progress?.Report(1f);
            _isBusy = false;
        }

        public async UniTask UnloadEverySceneAsync()
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            _isBusy = true;
            _progress?.Report(0f);

            var sceneCount = _loadedSceneHandles.Count;
            while (_loadedSceneHandles.Count > 0)
            {
                await Addressables.UnloadSceneAsync(_loadedSceneHandles[^1])
                    .ToUniTask(Progress.Create<float>(p => _progress?.Report(p * (sceneCount - _loadedSceneHandles.Count + 1) / sceneCount)));
                _loadedSceneHandles.RemoveAt(_loadedSceneHandles.Count - 1);
            }
        
            _progress?.Report(1f);
            _isBusy = false;
        }
    
        public async UniTask ReloadSceneAsync(string sceneName)
        {
            if (_isBusy) throw new Exception("SceneManager can handle only one scene loading at a time.");
            await UnloadSceneAsync(sceneName);
            await LoadSceneAsync(sceneName);
        }
    }
}