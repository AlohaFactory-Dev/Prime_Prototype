using System;
using Aloha.Coconut;
using Aloha.Coconut.Launcher;
using Aloha.Coconut.UI;
using Aloha.CoconutMilk;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using CoconutMilk.Equipments;

public class GlobalInstaller : MonoInstaller
{
    [SerializeField] private CoconutCanvas coconutCanvas;
    SaveDataManager _saveDataManager;

    public override void InstallBindings()
    {
        GlobalConainer.Initialize(Container);
        Container.Bind<SaveDataManager>().AsSingle().NonLazy();
        Container.BindInterfacesTo<SaveDataManagerLaunchingProcess>().AsSingle().NonLazy();

        // 서버 구축이 되기 전 사용, 서버에 저장할 데이터를 임시로 로컬에 저장
        _saveDataManager = Container.Resolve<SaveDataManager>();
        _saveDataManager.LinkFileDataSaver();
        _saveDataManager.Lock(true);

        Container.Bind<GameSceneManager>().AsSingle().NonLazy();
        Container.Bind<CoconutCanvas>().FromInstance(coconutCanvas).AsSingle().NonLazy();
        Container.Bind<GameManager>().AsSingle().NonLazy();
    }


    private void OnDisable()
    {
        SaveAll();
    }

    private void OnApplicationQuit()
    {
        SaveAll();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveAll();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveAll();
    }

    private void SaveAll()
    {
        if (_saveDataManager != null)
            _saveDataManager.Save();
    }
}