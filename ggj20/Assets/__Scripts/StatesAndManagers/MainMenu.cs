using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button joinGameButton;

    [Inject] private ClientManager clientManager;
    [Inject] private ClientState clientState;

    private void OnEnable()
    {
        joinGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDisable(this)
            .Subscribe(_ =>
            {
                joinGameButton.interactable = false;
                clientManager.Connect();
                clientState.GameState.Value = ClientState.GameMode.LOBBY_LOADING;
            });
    }
}
