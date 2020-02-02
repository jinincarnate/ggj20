using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using LiteNetLib;

public class Player {

    public class Factory: PlaceholderFactory<int,NetPeer,Player> {}

    public bool Ready;
    public int Id;
    public NetPeer Peer;

    public Player(int id, NetPeer _peer) {
        Id = id;
        Peer = _peer;
    }

};

public class Level {
    public int Index;
};

public class ServerState {

    public enum Mode {
        WAITING,
        LOADING,
        GAME,
        GAME_OVER
    }

    public ReactiveDictionary<int,Player> Players = new ReactiveDictionary<int,Player>();
    public ReactiveProperty<PeerMessage> PeerMessages = new ReactiveProperty<PeerMessage>(null);

    public ReactiveProperty<Mode> ServerMode = new ReactiveProperty<Mode>(Mode.WAITING);
    public ReactiveProperty<Level> CurrentLevel = new ReactiveProperty<Level>(null);
    public int CurrentHealth = -1;
    public int CurrentPoints = -1;
}
