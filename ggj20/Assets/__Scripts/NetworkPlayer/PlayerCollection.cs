using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class PlayerCollection : MonoBehaviour
{
    [Inject] private ClientState clientState;
    [Inject] private PlayerObject.Factory playerObjectFactory;

    void Awake()
    {
        clientState.Players.ObserveAdd()
            .TakeUntilDestroy(this)
            .Subscribe(HandleAdd);
    }

    private void HandleAdd(DictionaryAddEvent<int, PlayerData> player)
    {
        PlayerObject playerObject = playerObjectFactory.Create();
        playerObject.playerId = player.Key;
    }
}
