using System;
using _Scripts;
using Aloha.Coconut.UI;
using FactorySystem;
using UnityEngine;
using Zenject;

public class StageInstaller : MonoInstaller
{
    private FactoryManager _factoryManager;
    private CoconutCanvas _coconutCanvas;

    public override void InstallBindings()
    {
        StageConainer.Initialize(Container);

        // Bind the necessary components for the stage

        // 필요한 컴포넌트들을 가져옵니다.
        _coconutCanvas = GetComponentInChildren<CoconutCanvas>();
        _factoryManager = GetComponentInChildren<FactoryManager>();


        Container.Bind<CoconutCanvas>().FromInstance(_coconutCanvas).AsSingle().NonLazy();
        Container.Bind<FactoryManager>().FromInstance(_factoryManager).AsSingle().NonLazy();

        Container.Bind<StageManager>().AsSingle().NonLazy();
        // Container.Bind<MapManager>().FromInstance(mapManager).AsSingle().NonLazy();
        // Container.Bind<DropHealManager>().AsSingle().NonLazy();
        //
        //
        // Container.Bind<WaveManager>().FromInstance(waveManager).AsSingle().NonLazy();
        // Container.Bind<StageUI>().FromInstance(stageUI).AsSingle().NonLazy();
        // Container.Bind<StageCameraManager>().FromInstance(stageCameraManager).AsSingle().NonLazy();

        Init();
    }

    private async void Init()
    {
        try
        {
            await _factoryManager.Init(Container);
        }
        catch (Exception e)
        {
        }
    }
}