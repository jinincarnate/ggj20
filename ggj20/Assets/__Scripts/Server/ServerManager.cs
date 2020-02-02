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
    private List<ButtonInfo> activeInstructions = new List<ButtonInfo>();

    public void Initialize() {
        // TODO: Add to disposable
        serverState.PeerMessages
            .Where(message => message != null)
            .ObserveOnMainThread()
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

        serverState.CurrentHealth = currentLevel.MaxHealth;
        serverState.CurrentPoints = 0;

        int count = 0;
        foreach(KeyValuePair<int, Player> kvp in serverState.Players) {
            var player = kvp.Value;
            var config = new LevelData {
                Index = level.Index,
                Buttons = playerButtons.GetRange(count,LevelInfo.ButtonCount)
            };
            var configString = JsonConvert.SerializeObject(config);
            var data = new NetworkData {
                Type = MessageType.CURRENT_INFO,
                Data = configString
            };
            SendToPlayer(player, data);
            count += LevelInfo.ButtonCount;
        }

        instructionSet = new List<ButtonInfo>(ButtonConfig.GetRandomButtons(playerButtons, playerButtons.Count)).Select(info => info.Randomize()).ToList();
        activeInstructions.Clear();

        Observable.Timer(TimeSpan.FromSeconds(LevelInfo.WaitTime))
            .First()
            .Subscribe(_ => {
                   foreach(KeyValuePair<int, Player> kvp in serverState.Players) {
                       SendInstructionToPlayer(kvp.Value);
                   }
                   serverState.ServerMode.Value = ServerState.Mode.GAME;
                });
    }

    private void SendToPlayer(Player player, NetworkData data) {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(JsonConvert.SerializeObject(data));
        player.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void SendInstructionToPlayer(Player player) {
        var instruction = instructionSet[UnityEngine.Random.Range(0,instructionSet.Count)];
        instruction.Randomize();
        instruction.PlayerId = player.Id;
        activeInstructions.Add(instruction);
        SendToPlayer(player, new NetworkData {
                Type = MessageType.INSTRUCTION,
                Data = JsonConvert.SerializeObject(instruction)
            });

        Debug.Log($"Sending instruction to {player.Id}");
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
        serverState.ServerMode.Value = ServerState.Mode.LOADING;
        serverState.CurrentLevel.Value = new Level {
            Index = 0
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
            case MessageType.RESPONSE:
                HandleResponse(message.PeerId, message.Message);
                break;
            case MessageType.TIMER_OVER:
                HandleTimeout(message.PeerId, message.Message);
                break;
            default:
                break;
        }
    }

    private void OnLevelWon() {
        if(serverState.CurrentLevel.Value.Index >= levels.LevelInfo.Count - 1)  {
            SendToAll(new NetworkData {
                    Type = MessageType.GAME_WON,
                    Data = ""
                });
        }
        else {
            serverState.ServerMode.Value = ServerState.Mode.LOADING;
            serverState.CurrentLevel.Value = new Level {
                Index = serverState.CurrentLevel.Value.Index + 1
            };
        }
    }

    private void HandleTimeout(int id, NetworkData data) {
        if(serverState.ServerMode.Value != ServerState.Mode.GAME) {
            return;
        }

        var buttonInfo = JsonConvert.DeserializeObject<ButtonInfo>(data.Data);
        var found = activeInstructions.FindIndex(button => button.Equals(buttonInfo));

        if(found > -1) {
            serverState.CurrentHealth -= 1;
            var instr = activeInstructions[found];
            activeInstructions.RemoveAt(found);
            if(serverState.CurrentHealth < 0) {
                OnLevelLost();
            }
            else {
                var player = serverState.Players[id];
                SendInstructionToPlayer(player);
            }
            SendToAll(new NetworkData {
                    Type = MessageType.HEALTH,
                    Data = JsonConvert.SerializeObject(new HealthData { Health = serverState.CurrentHealth })
                });
        }
    }

    private void OnLevelLost() {
        // game over
        Debug.Log($"[SERVER] GAME OVER");
        serverState.ServerMode.Value = ServerState.Mode.GAME_OVER;
        SendToAll(new NetworkData {
            Type = MessageType.GAME_OVER,
            Data = ""
            });
    }

    private void HandleResponse(int id, NetworkData data) {

        if(serverState.ServerMode.Value != ServerState.Mode.GAME) {
            return;
        }

        var buttonInfo = JsonConvert.DeserializeObject<ButtonInfo>(data.Data);

        var found = activeInstructions.FindIndex(button => button.Equals(buttonInfo));

        if(found > -1) {
            serverState.CurrentPoints += 1;
            var currentLevel = levels.LevelInfo[serverState.CurrentLevel.Value.Index];
            if(serverState.CurrentPoints >= currentLevel.InstructionCount) {
                // Level Won!
                // Reset and start with next level
                OnLevelWon();
            }
            else {
                var instr = activeInstructions[found];
                activeInstructions.RemoveAt(found);
                var player = serverState.Players[instr.PlayerId];
                SendInstructionToPlayer(player);
            }
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
