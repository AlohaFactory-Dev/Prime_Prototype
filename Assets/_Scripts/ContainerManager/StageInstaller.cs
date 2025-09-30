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
    private StageManager _stageManager;
    public override void InstallBindings()
    {
        StageConainer.Initialize(Container);

        // 필요한 컴포넌트들을 가져옵니다.
        _coconutCanvas = GetComponentInChildren<CoconutCanvas>();
        _factoryManager = GetComponentInChildren<FactoryManager>();
        _stageManager = GetComponentInChildren<StageManager>();

        Container.Bind<CoconutCanvas>().FromInstance(_coconutCanvas).AsSingle().NonLazy();
        Container.Bind<FactoryManager>().FromInstance(_factoryManager).AsSingle().NonLazy();
        Container.Bind<StageManager>().FromInstance(_stageManager).AsSingle().NonLazy();

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