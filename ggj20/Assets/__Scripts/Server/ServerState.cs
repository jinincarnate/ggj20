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

public class ServerState {

    public ReactiveDictionary<int,Player> Players = new ReactiveDictionary<int,Player>();
    public ReactiveProperty<PeerMessage> PeerMessages = new ReactiveProperty<PeerMessage>(null);

}
