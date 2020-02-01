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
    [Inject] private InteractableButton.ClickButtonFactory clickButtonFactory;
    [Inject] private InteractableButton.ToggleButtonFactory toggleButtonFactory;

    private void OnEnable()
    {
        SetupPlayArea();
    }

    public void SetupPlayArea()
    {

        clientState.CurrentLevel
            .Where(level => level != null)
            .DistinctUntilChanged()
            .TakeUntilDisable(this)
            .Subscribe(level => {
                    foreach(var buttonData in level.Buttons) {
                        switch(buttonData.Type) {
                            case ButtonType.BUTTON:
                            {
                                InteractableButton interactable = clickButtonFactory.Create();
                                interactable.SetButtonData(buttonData);
                            }
                            break;

                            case ButtonType.TOGGLE:
                            {
                                InteractableButton interactable = toggleButtonFactory.Create();
                                interactable.SetButtonData(buttonData);
                            }
                            break;
                            case ButtonType.SLIDER:
                            {
                                InteractableButton interactable = toggleButtonFactory.Create();
                                interactable.SetButtonData(buttonData);
                            }
                            break;

                            default:
                            break;
                        }
                    }
                });
    }
}
