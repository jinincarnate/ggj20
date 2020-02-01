using UnityEngine;
using Zenject;
using UniRx;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public GameObject clickButtonPrefab;
    public GameObject toggleButtonPrefab;

    // [Inject] private ApplicationState applicationDataState;
    [Inject] private ClientState clientState;

    private void OnEnable()
    {
        SetupPlayArea();
    }

    public void SetupPlayArea()
    {

        clientState.CurrentLevel
            .Where(level => level != null)
            .TakeUntilDisable(this)
            .Subscribe(level => {
                    foreach(var buttonData in level.Buttons) {
                        switch(buttonData.Type) {
                            case ButtonType.BUTTON:
                            {
                                GameObject button = Instantiate(clickButtonPrefab, buttonsPanel);
                                InteractableButton interactable = button.GetComponent<InteractableButton>();
                                interactable.SetButtonText(buttonData.Name);
                            }
                            break;

                            case ButtonType.TOGGLE:
                            {
                                GameObject button = Instantiate(toggleButtonPrefab, buttonsPanel);
                                InteractableButton interactable = button.GetComponent<InteractableButton>();
                                interactable.SetButtonText(buttonData.Name);
                            }
                            break;

                            default:
                            break;
                        }
                    }
                });
    }
}
