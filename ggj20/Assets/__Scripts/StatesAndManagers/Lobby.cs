using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static ApplicationState;

public class Lobby : MonoBehaviour
{
    [SerializeField] private GameObject loadingBackground;
    [SerializeField] private Button startGameButton;

    [Inject] private ApplicationState applicationState;
    [Inject] private ClientState clientState;

    private void OnEnable()
    {
        loadingBackground.SetActive(true);
        startGameButton.gameObject.SetActive(false);

        clientState.ConnectionState
            .TakeUntilDestroy(this)
            .Subscribe(state => {
                if(state == ClientState.ServerConnection.CONNECTED)
                {
                    applicationState.currentGameState.Value = ApplicationState.GameState.LobbyWaiting;
                }
            });

        applicationState.currentGameState.TakeUntilDisable(this)
            .Subscribe(HandleGameStateChanged);

        startGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDisable(this)
            .Subscribe(_ =>
            {
                startGameButton.interactable = false;
                applicationState.currentGameState.Value = ApplicationState.GameState.Playing;
            });
    }

    private void HandleGameStateChanged(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.LobbyWaiting:
                loadingBackground.SetActive(false);
                startGameButton.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
