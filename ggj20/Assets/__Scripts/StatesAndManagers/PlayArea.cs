using UnityEngine;
using Zenject;
using UniRx;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public TMP_Text TextAreaText;
    public Image progressBar;

    // [Inject] private ApplicationState applicationDataState;
    [Inject] private ClientState clientState;
    [Inject] private InteractableButton.ClickButtonFactory clickButtonFactory;
    [Inject] private InteractableButton.ToggleButtonFactory toggleButtonFactory;
    [Inject] private LevelConfig levelConfig;

    private float currentProgress;
    private float currentMaxProgress;

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

        clientState.CurrentInstruction
            .Where(buttonInfo => buttonInfo != null)
            .DistinctUntilChanged()
            .TakeUntilDisable(this)
            .Subscribe(ResetProgress);
    }

    private void ResetProgress(ButtonInfo buttonInfo)
    {
        currentMaxProgress = levelConfig.LevelInfo[clientState.CurrentLevel.Value.Index].Timeout;
        currentProgress = currentMaxProgress;
    }

    private void Update()
    {
        if(currentProgress > 0.0f)
        {
            currentProgress -= Time.unscaledDeltaTime;
            progressBar.fillAmount = currentProgress / currentMaxProgress;
        }
        else
        {
            //TODO:Callback to server that instruction timed out.
        }
    }
}
