using UnityEngine;
using Zenject;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public GameObject clickButtonPrefab;
    public GameObject toggleButtonPrefab;

    [Inject] private ApplicationState applicationDataState;

    private void OnEnable()
    {
        SetupPlayArea();
    }

    public void SetupPlayArea()
    {
        foreach(ButtonData buttonData in applicationDataState.currentGameData.buttonData)
        {
            if(buttonData.buttonType == "click")
            {
                GameObject button = Instantiate(clickButtonPrefab, buttonsPanel);
                InteractableButton interactable = button.GetComponent<InteractableButton>();
                interactable.SetButtonText(buttonData.buttonName);
            }
            else if(buttonData.buttonType == "toggle")
            {
                GameObject button = Instantiate(toggleButtonPrefab, buttonsPanel);
                InteractableButton interactable = button.GetComponent<InteractableButton>();
                interactable.SetButtonText(buttonData.buttonName);
            }
        }
    }
}
