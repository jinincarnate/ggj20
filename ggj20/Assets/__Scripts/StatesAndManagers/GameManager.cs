using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        MainMenu,
        LobbyLoading,
        LobbyWaiting,
        Playing
    }

    public const string LOBBY = "Lobby";
    public const string PLAYAREA = "PlayArea";

    public static GameManager gameManager;

    public ReactiveProperty<GameStates> GameState = new ReactiveProperty<GameStates>(GameStates.MainMenu);

    private void Awake()
    {
        if(gameManager != null && gameManager != this)
        {
            Destroy(this);
            return;
        }
        gameManager = this;
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        GameState.TakeUntilDestroy(this)
            .Subscribe(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameStates gameState)
    {
        switch(gameState)
        {
            case GameStates.LobbyLoading:
                SceneManager.LoadScene(LOBBY);
                break;
            case GameStates.Playing:
                SceneManager.LoadScene(PLAYAREA);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
