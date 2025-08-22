using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut.Launcher
{
    public class Launcher : MonoBehaviour
    {
        private bool _isLaunchingComplete;

        [SerializeField] private SceneContext sceneContext;

        [Inject] private GameSceneManager _gameSceneManager;
        [Inject] private List<ILaunchingProcess> _launchingProcesses;

        private ITitleScreen _titleScreen;

        void Start()
        {
            Assert.IsFalse(sceneContext.autoRun,
                "SceneContext AutoRun must be false. Launcher will run the sceneContext.");
            Launch();
        }

        private async UniTaskVoid Launch()
        {
            var launcherConfig = CoconutConfig.Get<LauncherConfig>();
            TypeUtility.CacheDerivedTypes(typeof(LauncherInstaller), typeof(IPreLaunchScript));
            
            // PreLaunchScripts 실행
            foreach (var preLaunchScript in launcherConfig.preLaunchScripts)
            {
                var scriptType = TypeUtility.GetType(typeof(IPreLaunchScript), preLaunchScript);
                Assert.IsNotNull(scriptType, $"PreLaunchScript {preLaunchScript} is not found.");
                
                var script = (IPreLaunchScript) System.Activator.CreateInstance(scriptType);
                script.OnPreLaunch();
            }
            
            // Title 생성
            var title = launcherConfig.titlePrefab.InstantiateAsync().WaitForCompletion();
            _titleScreen = title.GetComponent<ITitleScreen>();
            Assert.IsNotNull(_titleScreen, "TitleScreen must have a component that implement ITitleScreen");
            
            _titleScreen.Report(0f);
            
            // LauncherInstaller 등록
            foreach (var launcherInstaller in launcherConfig.launcherInstallers)
            {
                var installerType = TypeUtility.GetType(typeof(LauncherInstaller), launcherInstaller);
                Assert.IsNotNull(installerType, $"LauncherInstaller {launcherInstaller} is not found.");
                
                var installer = (LauncherInstaller) System.Activator.CreateInstance(installerType);
                sceneContext.AddNormalInstaller(installer);
            }

            var instantiateTasks = new List<UniTask<GameObject>>();

            // LauncherObject 생성
            foreach (var launcherObjectReference in launcherConfig.launcherObjects)
            {
                var launcherObjectTask = launcherObjectReference.InstantiateAsync().ToUniTask();
                instantiateTasks.Add(launcherObjectTask);
            }

            var instances = await UniTask.WhenAll(instantiateTasks);

            // LauncherObject에 포함된 MonoInstaller 등록 후 Install
            var monoInstallers = (List<MonoInstaller>)sceneContext.Installers;
            foreach (var launcherObjectInstance in instances)
            {
                var installers = launcherObjectInstance.GetComponentsInChildren<MonoInstaller>(true);
                if (installers.Length > 0) monoInstallers.AddRange(installers);
            }

            sceneContext.Run();

            // Install된 LaunchingProcess들 실행
            _launchingProcesses.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var launchingProcess in _launchingProcesses)
            {
                _titleScreen.SetMessage(launchingProcess.Message);

                if (launchingProcess.IsBlocker) await launchingProcess.Run(_titleScreen);
                else launchingProcess.Run(_titleScreen).Forget();
            }

            _titleScreen.Report(1);
            _isLaunchingComplete = true;

            if (launcherConfig.autoBootAfterLaunch) Boot();
        }

        public async UniTaskVoid Boot()
        {
            Assert.IsTrue(_isLaunchingComplete, "Launcher is not completed yet.");
            _gameSceneManager.LinkProgress(_titleScreen);
            await _gameSceneManager.LoadSceneAsync(CoconutConfig.Get<LauncherConfig>().rootGameScene);
            _titleScreen.Hide();
        }

        public async UniTaskVoid Reboot()
        {
            _titleScreen.Show();
            Assert.IsTrue(_isLaunchingComplete, "Launcher is not completed yet.");
            await _gameSceneManager.UnloadEverySceneAsync();
            Boot();
        }
    }
}