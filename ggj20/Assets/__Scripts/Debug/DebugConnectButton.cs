using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DebugConnectButton : MonoBehaviour {

    [Inject]
    private ClientManager clientManager;

    public void OnConnect() {
        clientManager.Connect();
    }
}
