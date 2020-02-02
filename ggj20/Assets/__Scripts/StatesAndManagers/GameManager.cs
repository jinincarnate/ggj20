using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static ClientState;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject playAreaPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Inject] private ClientState clientState;
    private CompositeDisposable disposable = new CompositeDisposable();

    public void Start()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        playAreaPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        clientState.GameState
            .Subscribe(HandleGameStateChanged)
            .AddTo(disposable);

        clientState.CurrentLevel
            .Where(level => level != null)
            .TakeUntilDisable(this)
            .Subscribe(_ => clientState.GameState.Value = GameMode.PLAYING);
    }

    private void HandleGameStateChanged(GameMode gameState)
    {
        switch(gameState)
        {
            case GameMode.MAIN_MENU:
                mainMenuPanel.SetActive(true);
                lobbyPanel.SetActive(false);
                playAreaPanel.SetActive(false);
                gameOverPanel.SetActive(false);
                break;
            case GameMode.LOBBY_LOADING:
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                playAreaPanel.SetActive(false);
                gameOverPanel.SetActive(false);
                break;
            case GameMode.PLAYING:
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                playAreaPanel.SetActive(true);
                gameOverPanel.SetActive(false);
                break;
            case GameMode.GAME_OVER:
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                playAreaPanel.SetActive(false);
                gameOverPanel.SetActive(true);
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
