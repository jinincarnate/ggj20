using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using Newtonsoft.Json;
using UnityEngine.UI;

public class InteractableButton : MonoBehaviour
{
    public class ClickButtonFactory: PlaceholderFactory<InteractableButton>
    {

    }

    public class ToggleButtonFactory : PlaceholderFactory<InteractableButton>
    {

    }

    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;
    [SerializeField] private Toggle toggle;

    [Inject] private ClientListener client;
    [Inject] private ClientState clientState;

    private ButtonInfo buttonInfo;

    public void SetButtonData(ButtonInfo buttonInfo)
    {
        this.buttonInfo = buttonInfo;
        buttonText.text = buttonInfo.Name;
        if(buttonInfo.Type == ButtonType.TOGGLE)
        {
            toggle.isOn = buttonInfo.On;
        }
    }

    public void OnClick() {

        if(clientState.ButtonInteractable == false) {
            return;
        }

        if(buttonInfo.Type == ButtonType.TOGGLE)
        {
            buttonInfo.On = !buttonInfo.On;
        }

        client.SendMessage(new NetworkData {
                Type = MessageType.RESPONSE,
                Data = JsonConvert.SerializeObject(buttonInfo)
            });
    }
}
