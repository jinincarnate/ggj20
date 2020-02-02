using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static ClientState;

public class Lobby : MonoBehaviour
{
    [SerializeField] private GameObject loadingBackground;
    [SerializeField] private GameObject lobby;
    [SerializeField] private Button startGameButton;

    [Inject] private ClientState clientState;
    [Inject] private ClientListener clientListener;

    private void OnEnable()
    {
        loadingBackground.SetActive(true);
        lobby.SetActive(false);
        startGameButton.gameObject.SetActive(false);

        clientState.ConnectionState
            .TakeUntilDestroy(this)
            .Subscribe(state => {
                if(state == ClientState.ServerConnection.CONNECTED)
                {
                    clientState.GameState.Value = ClientState.GameMode.LOBBY_WAITING;
                }
            });

        clientState.GameState
            .TakeUntilDisable(this)
            .Subscribe(HandleGameStateChanged);

        startGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDisable(this)
            .Subscribe(_ =>
            {
                startGameButton.interactable = false;
                clientListener.SendMessage(new NetworkData
                {
                    Type = MessageType.READY,
                    Data = JsonConvert.SerializeObject(new ReadyData { Ready = true })
                });
            });
    }

    private void HandleGameStateChanged(GameMode gameState)
    {
        switch(gameState)
        {
            case GameMode.LOBBY_WAITING:
                loadingBackground.SetActive(false);
                lobby.SetActive(true);
                startGameButton.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
