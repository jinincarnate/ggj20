using UniRx;
using Zenject;
using Newtonsoft.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Linq;

public class ServerManager : IInitializable {

    [Inject]
    private ServerState serverState;

    [Inject]
    private Player.Factory playerFactory;

    public void Initialize() {
        // TODO: Add to disposable
        serverState.PeerMessages
            .Where(message => message != null)
            .Subscribe(HandleMessage);

        serverState.Players.ObserveAdd()
            .Subscribe(HandleAdd);
        serverState.Players.ObserveReplace()
            .Subscribe(HandleReplace);
    }

    private void HandleAdd(DictionaryAddEvent<int, Player> player) {

        DistributePlayerInfo(player.Value);
    }

    private void HandleReplace(DictionaryReplaceEvent<int, Player> player) {
        DistributePlayerInfo(player.NewValue);
    }

    private void DistributePlayerInfo(Player _player) {
        var list = serverState.Players.Values.Select(player => {
                return new PlayerData {
                    Id = player.Id,
                    Ready = player.Ready
                };
            }).ToList();
        SendToAll(new NetworkData {
                Type = MessageType.PLAYER_DATA,
                Data = JsonConvert.SerializeObject(new PlayerList {
                        List = list
                    })
            });
    }

    public void SendToAll(NetworkData data) {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(JsonConvert.SerializeObject(data));
        foreach(KeyValuePair<int, Player> kvp in serverState.Players) {
            kvp.Value.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    private void HandleMessage(PeerMessage message) {

        var type = message.Message.Type;

        switch(type) {
            case MessageType.READY:
                HandleReady(message.PeerId, message.Message);
                break;
            default:
                break;
        }
    }

    private void HandleReady(int id, NetworkData data) {
        var readyData = JsonConvert.DeserializeObject<ReadyData>(data.Data);

        var oldPlayer = serverState.Players[id];
        var player = playerFactory.Create(id, oldPlayer.Peer);

        player.Ready = readyData.Ready;

        serverState.Players[id] = player;
    }
}
