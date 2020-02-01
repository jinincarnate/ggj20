using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ClientState {
    public enum ServerConnection {
        UNCONNECTED,
        DISCOVERING,
        CONNECTING,
        CONNECTED,
        ERROR
    }

    public ReactiveProperty<ServerConnection> ConnectionState =
        new ReactiveProperty<ServerConnection>(ServerConnection.UNCONNECTED);

    public ReactiveProperty<NetworkData> NetworkMessages = new ReactiveProperty<NetworkData>(null);

    public ReactiveDictionary<int,PlayerData> Players = new ReactiveDictionary<int,PlayerData>();

    public ReactiveProperty<LevelData> CurrentLevel = new ReactiveProperty<LevelData>();

    public ReactiveProperty<ButtonInfo> CurrentInstruction = new ReactiveProperty<ButtonInfo>(null);
}
