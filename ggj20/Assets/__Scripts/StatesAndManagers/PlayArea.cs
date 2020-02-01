using UnityEngine;
using Zenject;
using UniRx;
using TMPro;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public TMP_Text TextAreaText;

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

        clientState.CurrentInstruction
            .Where(buttonInfo => buttonInfo != null)
            .DistinctUntilChanged()
            .TakeUntilDisable(this)
            .Subscribe(buttonInfo => TextAreaText.text = buttonInfo.ButtonTextOptions.Count > 0 ? buttonInfo.ButtonTextOptions[0] : "Not Found");//TODO:randomise
    }
}
