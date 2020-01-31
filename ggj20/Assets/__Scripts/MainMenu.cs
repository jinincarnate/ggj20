using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button joinGameButton;

    // Start is called before the first frame update
    private void Start()
    {
        joinGameButton.OnClickAsObservable()
            .First()
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                joinGameButton.interactable = false;
                GameManager.gameManager.GameState.Value = GameManager.GameStates.LobbyLoading;
            });
    }
}
