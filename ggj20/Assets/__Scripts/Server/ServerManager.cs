using UniRx;
using Zenject;
using Newtonsoft.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class ServerManager : IInitializable {

    [Inject]
    private ServerState serverState;

    [Inject]
    private Player.Factory playerFactory;

    [Inject]
    private LevelConfig levels;

    [Inject]
    private ButtonConfig buttons;

    private List<ButtonInfo> instructionSet = null;

    public void Initialize() {
        // TODO: Add to disposable
        serverState.PeerMessages
            .Where(message => message != null)
            .Subscribe(HandleMessage);

        serverState.Players.ObserveAdd()
            .Subscribe(HandleAdd);
        serverState.Players.ObserveReplace()
            .Subscribe(HandleReplace);

        serverState.CurrentLevel
            .Where(lvl => lvl != null)
            .Subscribe(StartLevel);
    }

    private void StartLevel(Level level) {
        var totalPlayers = serverState.Players.Count;
        var currentLevel = levels.LevelInfo[level.Index];

        var playerButtons = ButtonConfig.GetRandomButtons(buttons.Buttons, LevelInfo.ButtonCount*totalPlayers);

        int count = 0;
        foreach(KeyValuePair<int, Player> kvp in serverState.Players) {
            var player = kvp.Value;
            var config = new LevelData {
                Index = level.Index,
                Buttons = playerButtons.GetRange(count,LevelInfo.ButtonCount)
            };
            Debug.Log($"Sending buttons {config.Buttons.Count}");
            var configString = JsonConvert.SerializeObject(config);
            var data = new NetworkData {
                Type = MessageType.CURRENT_INFO,
                Data = configString
            };
            SendToPlayer(player, data);
            count += LevelInfo.ButtonCount;
        }

        instructionSet = new List<ButtonInfo>(ButtonConfig.GetRandomButtons(buttons.Buttons, currentLevel.InstructionCount));

        Observable.Timer(TimeSpan.FromSeconds(LevelInfo.WaitTime))
            .First()
            .Subscribe(_ => {
                   foreach(KeyValuePair<int, Player> kvp in serverState.Players) {
                       SendInstructionToPlayer(kvp.Value);
                   }
                });
    }

    private void SendToPlayer(Player player, NetworkData data) {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(JsonConvert.SerializeObject(data));
        player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void SendInstructionToPlayer(Player player) {
        if(instructionSet.Count > 0) {
            var instruction = instructionSet[0];
            instructionSet.RemoveAt(0);
            SendToPlayer(player, new NetworkData {
                    Type = MessageType.INSTRUCTION,
                    Data = JsonConvert.SerializeObject(instruction)
                });
        }
    }

    private void HandleAdd(DictionaryAddEvent<int, Player> player) {

        DistributePlayerInfo(player.Value);
    }

    private void HandleReplace(DictionaryReplaceEvent<int, Player> player) {
        CheckReady(DistributePlayerInfo(player.NewValue));
    }

    private List<PlayerData> DistributePlayerInfo(Player _player) {
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

        return list;
    }

    private void CheckReady(List<PlayerData> players) {
        var allReady = players.All(pd => pd.Ready);

        if(allReady && serverState.ServerMode.Value == ServerState.Mode.WAITING) {
            StartGame();
        }
    }

    private void StartGame() {
        serverState.ServerMode.Value = ServerState.Mode.GAME;
        serverState.CurrentLevel.Value = new Level {
            Index = 1
        };
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
