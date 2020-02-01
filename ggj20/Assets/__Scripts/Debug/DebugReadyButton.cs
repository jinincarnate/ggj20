using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Newtonsoft.Json;

public class DebugReadyButton : MonoBehaviour {

    [Inject]
    private ClientManager clientManager;

    [Inject]
    private ClientListener clientListener;

    [Inject]
    private ClientState clientState;

    private void Awake() {
        gameObject.SetActive(false);

        clientState.ConnectionState
            .TakeUntilDestroy(this)
            .Subscribe(c => {
                    if(c == ClientState.ServerConnection.CONNECTED) {
                        gameObject.SetActive(true);
                    }
                });
    }

    public void OnReady() {
        clientListener.SendMessage(new NetworkData {
                Type = MessageType.READY,
                Data = JsonConvert.SerializeObject(new ReadyData { Ready = true })
            });
    }
}
