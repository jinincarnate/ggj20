using LiteNetLib;
using Zenject;
using System;
using System.Net;
using LiteNetLib.Utils;
using UnityEngine;
using System.Net.Sockets;
using Newtonsoft.Json;

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
        listener.PeerDisconnectedEvent += (NetPeer peer, DisconnectInfo disconnectInfo) => {
            Debug.Log($"Server disconnected because {disconnectInfo.Reason}");
        };

        listener.NetworkErrorEvent += (IPEndPoint endPoint, SocketError socketError) => {
            Debug.Log($"Server connection error {endPoint} {socketError}");
        };

        listener.NetworkReceiveEvent += OnNetworkReceive;

        client = new NetManager(listener)
            {
                UnconnectedMessagesEnabled = true
            };

        client.Start();
    }

    private void OnNetworkReceive (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {

        clientState.NetworkMessages.Value = JsonConvert.DeserializeObject<NetworkData>(reader.GetString());
    }

    public void SendMessage (NetworkData message) {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(JsonConvert.SerializeObject(message));
        server?.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    // client received a response from the server
    // can connect to server!
    private void OnNetworkReceiveUnconnected (IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
        var text = reader.GetString(100);
        // Debug.Log($"[Client] ReceiveUnconnected {messageType}. From: {remoteEndPoint}. Data: {text}");
        if (remoteEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
            messageType == UnconnectedMessageType.BasicMessage &&
            text == serverSettings.DiscoveryMessage) {
            clientState.ConnectionState.Value = ClientState.ServerConnection.CONNECTING;
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
        NetDataWriter writer = new NetDataWriter();
        writer.Put("Server Broadcast");
        client.SendBroadcast(writer, serverSettings.Port);
        writer.Reset();
    }
}
