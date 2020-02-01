using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using System;
using Newtonsoft.Json;

public class ClientManager : IInitializable, ITickable, IDisposable {

    [Inject]
    private ClientListener clientListener;

    [Inject]
    private ClientState clientState;

    [Inject]
    private LocalServer.Factory serverFactory;

    private LocalServer localServer;

    public void Initialize() {
        clientState.NetworkMessages
            .Where(message => message != null)
            .Subscribe(HandleMessages);
    }

    private void HandleMessages(NetworkData message) {
        switch(message.Type) {
            case MessageType.PLAYER_DATA:
                HandlePlayerState(JsonConvert.DeserializeObject<PlayerList>(message.Data));
                break;
            case MessageType.CURRENT_INFO:
                HandleCurrentInfo(JsonConvert.DeserializeObject<LevelData>(message.Data));
                break;
            default:
                break;
        }
    }

    private void HandleCurrentInfo(LevelData data) {
        clientState.CurrentLevel.Value = data;
    }

    private void HandlePlayerState (PlayerList data) {
        data.List.ForEach(pd => {
                clientState.Players[pd.Id] = pd;
            });
    }

    public void Dispose() {
        localServer?.Dispose();
    }

    public void Tick() {
        localServer?.Tick();
    }

    public void Connect() {
        if(clientState.ConnectionState.Value != ClientState.ServerConnection.UNCONNECTED) {

            // already trying, dont respond
            return;
        }

        clientState.ConnectionState.Value = ClientState.ServerConnection.DISCOVERING;

        clientListener.SendBroadcast();

        Observable.Timer(TimeSpan.FromSeconds(2))
            .TakeUntil(clientState.ConnectionState.Where(c => c == ClientState.ServerConnection.CONNECTED))
            .Subscribe(_ => {
                    Debug.Log("Not connected after first broadcast, creating server");
                    localServer = serverFactory.Create();
                    localServer.Initialize();
                });

        clientState.ConnectionState
            .Subscribe(_ => Debug.Log(_));

        // try every three seconds
        Observable.Interval(TimeSpan.FromMilliseconds(250))
            // till client is connected
            .TakeUntil(clientState.ConnectionState.Where(c => c == ClientState.ServerConnection.CONNECTED || c == ClientState.ServerConnection.CONNECTING))
            // try 10 times
            .Take(40)
            .Finally(() => {
                    // after 10 tries, if still not connected, switch to error state
                    if(clientState.ConnectionState.Value != ClientState.ServerConnection.CONNECTED
                       && clientState.ConnectionState.Value != ClientState.ServerConnection.CONNECTING) {
                        clientState.ConnectionState.Value = ClientState.ServerConnection.ERROR;
                    }
                })
            .Subscribe(_ => {
                    clientListener.SendBroadcast();
                });
    }
}
