using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class Lobby : MonoBehaviour
{
    [SerializeField] private GameObject loadingBackground;
    [SerializeField] private Button startGameButton;

    private void Awake()
    {
        loadingBackground.SetActive(true);
        startGameButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        Observable.Timer(new System.TimeSpan(0, 0, 2))
            .First()
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                GameManager.gameManager.GameState.Value = GameManager.GameStates.LobbyWaiting;
            });

        GameManager.gameManager.GameState.TakeUntilDestroy(this)
            .Subscribe(HandleGameStateChanged);

        startGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                startGameButton.interactable = false;
                GameManager.gameManager.GameState.Value = GameManager.GameStates.Playing;
            });
    }

    private void HandleGameStateChanged(GameStates gameState)
    {
        switch(gameState)
        {
            case GameStates.LobbyWaiting:
                loadingBackground.SetActive(false);
                startGameButton.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
