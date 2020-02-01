using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public GameObject clickButtonPrefab;
    public GameObject toggleButtonPrefab;

    private void Start()
    {
        SetupPlayArea();
    }

    public void SetupPlayArea()
    {
        foreach(ButtonData buttonData in ApplicationDataState.applicationDataState.currentGameData.buttonData)
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
