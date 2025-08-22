using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Aloha.Coconut.Launcher
{
    [CreateAssetMenu(fileName = "LauncherConfig", menuName = "Coconut/Config/LauncherConfig")]
    public class LauncherConfig : CoconutConfig
    {
        [ValidateInput(nameof(ValidateScenePath), "Launcher Scene Path is invalid.")]
        public string launcherScenePath;

        [Space]
        [ValueDropdown(nameof(GetPreLaunchScripts))]
        public List<string> preLaunchScripts;
        
        [ValueDropdown(nameof(GetLauncherInstallers))]
        public List<string> launcherInstallers;
        
        [Space]
        public bool autoBootAfterLaunch = true;
        public AssetReference rootGameScene;
        public AssetReferenceGameObject titlePrefab;

        [Space]
        [InfoBox("Launcher 씬에 생성되어야 하는 오브젝트들.\n" +
                 "MonoInstaller를 포함하고 있을 경우, Launcher Container에서 해당 Installer 실행")]
        public List<AssetReferenceGameObject> launcherObjects;

        private bool ValidateScenePath()
        {
            if (string.IsNullOrEmpty(launcherScenePath) || !File.Exists(launcherScenePath))
            {
                return false;
            }

            return true;
        }

        private static IEnumerable<string> _preLaunchScriptsCache;

        private IEnumerable<string> GetPreLaunchScripts()
        {
            if(_preLaunchScriptsCache != null) return _preLaunchScriptsCache;
            
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes()
                        .Where(t => !t.IsInterface && typeof(IPreLaunchScript).IsAssignableFrom(t)));
                }
                catch (ReflectionTypeLoadException)
                {
                    // Some assemblies might not be accessible, skip them.
                }   
            }

            _preLaunchScriptsCache = types.Select(t => t.FullName);
            return _preLaunchScriptsCache;
        }
        
        private static IEnumerable<string> _launcherInstallersCache;

        private IEnumerable<string> GetLauncherInstallers()
        {
            if(_launcherInstallersCache != null) return _launcherInstallersCache;
            
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes()
                        .Where(t => !t.IsAbstract && typeof(LauncherInstaller).IsAssignableFrom(t)));
                }
                catch (ReflectionTypeLoadException)
                {
                    // Some assemblies might not be accessible, skip them.
                }   
            }

            _launcherInstallersCache = types.Select(t => t.FullName);
            return _launcherInstallersCache;
        }

        public void Reset()
        {
            preLaunchScripts = new List<string>();
            launcherInstallers = new List<string>() { "Aloha.Coconut.Launcher.DefaultLauncherInstaller" };
            
            rootGameScene = null;
            titlePrefab = null;
            launcherObjects = new List<AssetReferenceGameObject>();
        }
    }
}