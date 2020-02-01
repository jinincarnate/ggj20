using LiteNetLib;
using Zenject;
using System;
using System.Net;
using LiteNetLib.Utils;
using UnityEngine;

public class ClientListener : IInitializable, ITickable, IDisposable {

    [Inject]
    private LocalServer.LocalServerSettings serverSettings;

    [Inject]
    private ClientState clientState;

    private NetManager client;
    private NetPeer server = null;

    public void Initialize() {
        EventBasedNetListener listener = new EventBasedNetListener();

        listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;

        listener.PeerConnectedEvent += OnServerConnected;

        client = new NetManager(listener)
            {
                UnconnectedMessagesEnabled = true
            };

        client.Start();
    }

    // client received a response from the server
    // can connect to server!
    private void OnNetworkReceiveUnconnected (IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
        var text = reader.GetString(100);
        Debug.Log($"[Client] ReceiveUnconnected {messageType}. From: {remoteEndPoint}. Data: {text}");
        if (messageType == UnconnectedMessageType.BasicMessage &&
            text == serverSettings.DiscoveryMessage) {
            client.Connect(remoteEndPoint, serverSettings.Key);
        }
    }

    private void OnServerConnected (NetPeer peer) {

        clientState.ConnectionState.Value = ClientState.ServerConnection.CONNECTED;
        server = peer;
    }

    public void Tick() {
        client.PollEvents();
    }

    public void Dispose() {
        client.Stop();
    }

    // On start game
    // broadcast looking for a server
    public void SendBroadcast() {
        Debug.Log("Sending broadcast to find server");
        NetDataWriter writer = new NetDataWriter();
        writer.Put("Server Broadcast");
        client.SendBroadcast(writer, serverSettings.Port);
        writer.Reset();
    }
}
