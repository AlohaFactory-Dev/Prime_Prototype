using System;
using System.Threading.Tasks;
using Aloha.Coconut;
using Aloha.Coconut.Launcher;
using Aloha.Coconut.UI;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameManager
{
    private GameSceneManager _gameSceneManager;
    private CoconutCanvas _coconutCanvas;
    private DiContainer _container;

    public GameManager(GameSceneManager gameSceneManager, CoconutCanvas coconutCanvas, DiContainer container)
    {
        _gameSceneManager = gameSceneManager;
        _coconutCanvas = coconutCanvas;
        _container = container;
    }

    //  TableListContainer가 완료되고 Bind해야 될 객체을 해줌
    public async UniTask Init()
    {
        await TableListContainer.InitAllTables();
        await ImageContainer.InitializeAsync();
        _container.Bind<PropertyIconPool>().AsSingle().NonLazy();
        LoadStage();
    }


    private async UniTask LoadLobby()
    {
        await _gameSceneManager.LoadSceneAsync("Lobby");
        // LobbyConainer.Get<LobbyUI>().Show();
    }

    public void LoadStage(bool isFirstLogin = false)
    {
        _coconutCanvas.Open(SceneTransitionViewer.ConfigName, new SceneTransitionViewer.SceneTransitionOpenArgs(Load, Resume));

        async UniTask Load()
        {
            Pause();
            await _gameSceneManager.LoadSceneAsync("Stage");
        }
    }

    public void ReLoadStage()
    {
        _coconutCanvas.Open(SceneTransitionViewer.ConfigName, new SceneTransitionViewer.SceneTransitionOpenArgs(ReLoad, Resume));

        async UniTask ReLoad()
        {
            Pause();
            await _gameSceneManager.ReloadSceneAsync("Stage");
            // await StageConainer.Get<StageManager>().Init();
        }
    }

    public void UnloadStage()
    {
        _coconutCanvas.Open(SceneTransitionViewer.ConfigName, new SceneTransitionViewer.SceneTransitionOpenArgs(Unload, Resume));


        async UniTask Unload()
        {
            Pause();
            await _gameSceneManager.UnloadSceneAsync("Stage");
            // LobbyConainer.Get<LobbyUI>().Show();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }
}