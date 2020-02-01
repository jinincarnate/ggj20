using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum MessageType {
    READY,
    PLAYER_DATA,
    SERVER_READY
};

[Serializable]
public class NetworkData {
    [JsonProperty("type")]
    public MessageType Type;
    [JsonProperty("data")]
    public string Data;
};

[Serializable]
public class PeerMessage {
    public int PeerId;
    public NetworkData Message;
};

[Serializable]
public class ReadyData {
    public bool Ready;
    public int Id;
};

[Serializable]
public class PlayerList {
    public List<PlayerData> List = new List<PlayerData>();
}

[Serializable]
public class PlayerData {
    public int Id;
    public bool Ready;
};
