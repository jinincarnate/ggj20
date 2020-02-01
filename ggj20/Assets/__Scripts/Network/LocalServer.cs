using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

// These interfaces are not called automatically
//  because local server is created dynamically by a factory
public class LocalServer : IInitializable, ITickable, IDisposable {

    [Serializable]
    public class LocalServerSettings {
        public int Port;
        public string DiscoveryMessage = "DISCOVERED";
        public string Key = "key";
        public int MaxPlayers = 4;
    }

    public class Factory: PlaceholderFactory<LocalServer> {}

    private NetManager server;

    [Inject]
    private LocalServerSettings serverSettings;

    public void Initialize() {
        EventBasedNetListener listener = new EventBasedNetListener();

        listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;

        listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < serverSettings.MaxPlayers)
                    request.AcceptIfKey(serverSettings.Key);
                else
                    request.Reject();
            };


        server = new NetManager(listener);
        server.BroadcastReceiveEnabled = true;
        server.Start(serverSettings.Port);
        Debug.Log($"Local server started on port {serverSettings.Port}");
    }

    private void OnNetworkReceiveUnconnected (IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
        Debug.Log($"[Server] ReceiveUnconnected {messageType}. From: {remoteEndPoint}. Data: {reader.GetString(100)}");
        NetDataWriter writer = new NetDataWriter();
        writer.Put(serverSettings.DiscoveryMessage);
        server.SendUnconnectedMessage(writer, remoteEndPoint);
    }

    public void Tick() {
        server.PollEvents();
    }

    public void Dispose() {
        server.Stop();
    }
}
