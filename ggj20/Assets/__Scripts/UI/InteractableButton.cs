using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using Newtonsoft.Json;

public class InteractableButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    [Inject] private ClientListener client;

    private ButtonInfo buttonInfo;

    public void SetButtonText(string text)
    {
        buttonText.text = text;
    }

    public void OnClick() {
        client.SendMessage(new NetworkData {
                Type = MessageType.RESPONSE,
                Data = JsonConvert.SerializeObject(buttonInfo)
            });
    }
}
