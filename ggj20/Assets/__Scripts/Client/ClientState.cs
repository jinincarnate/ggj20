using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ClientState {
    public enum ServerConnection {
        UNCONNECTED,
        DISCOVERING,
        CONNECTED,
        ERROR
    }

    public ReactiveProperty<ServerConnection> ConnectionState =
        new ReactiveProperty<ServerConnection>(ServerConnection.UNCONNECTED);

}
