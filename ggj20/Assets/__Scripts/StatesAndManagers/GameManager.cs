using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static ApplicationState;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject playAreaPanel;

    [Inject] private ApplicationState applicationState;
    [Inject] private ClientState clientState;
    private CompositeDisposable disposable = new CompositeDisposable();

    public void Start()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        playAreaPanel.SetActive(false);

        applicationState.currentGameState
            .Subscribe(HandleGameStateChanged)
            .AddTo(disposable);

        clientState.CurrentLevel
            .Where(level => level != null)
            .TakeUntilDisable(this)
            .Subscribe(_ => applicationState.currentGameState.Value = GameState.Playing);

        applicationState.currentGameState.Value = GameState.MainMenu;
    }

    private void HandleGameStateChanged(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.MainMenu:
                mainMenuPanel.SetActive(true);
                lobbyPanel.SetActive(false);
                playAreaPanel.SetActive(false);
                break;
            case GameState.LobbyLoading:
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                playAreaPanel.SetActive(false);
                break;
            case GameState.Playing:
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                playAreaPanel.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
