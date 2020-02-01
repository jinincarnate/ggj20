using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

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

    [Inject]
    private ServerState serverState;

    [Inject]
    private Player.Factory playerFactory;

    public void Initialize() {
        EventBasedNetListener listener = new EventBasedNetListener();

        listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;

        // TODO:  make this a function of the class
        listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < serverSettings.MaxPlayers &&
                   serverState.ServerMode.Value == ServerState.Mode.WAITING)
                    request.AcceptIfKey(serverSettings.Key);
                else
                    request.Reject();
            };

        listener.PeerConnectedEvent += OnPlayerConnected;
        listener.PeerDisconnectedEvent += OnPlayerDisconnected;

        listener.NetworkReceiveEvent += OnNetworkReceive;

        server = new NetManager(listener);
        server.BroadcastReceiveEnabled = true;
        server.Start(serverSettings.Port);
        Debug.Log($"Local server started on port {serverSettings.Port}");

    }

    private void OnNetworkReceive (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {
        serverState.PeerMessages.Value = new PeerMessage {
            PeerId = peer.Id,
            Message = JsonConvert.DeserializeObject<NetworkData>(reader.GetString())
        };
        reader.Recycle();
    }

    private void OnPlayerConnected (NetPeer peer) {
        serverState.Players.Add(peer.Id, playerFactory.Create(peer.Id, peer));
    }

    private void OnPlayerDisconnected (NetPeer peer, DisconnectInfo info) {

    }

    private void OnNetworkReceiveUnconnected (IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
        // Debug.Log($"[Server] ReceiveUnconnected {messageType}. From: {remoteEndPoint}. Data: {reader.GetString(100)}");
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
