using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button joinGameButton;

    [Inject] private ApplicationState applicationState;
    [Inject] private ClientManager clientManager;

    private void OnEnable()
    {
        joinGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDisable(this)
            .Subscribe(_ =>
            {
                joinGameButton.interactable = false;
                clientManager.Connect();
                applicationState.currentGameState.Value = ApplicationState.GameState.LobbyLoading;
            });
    }
}
