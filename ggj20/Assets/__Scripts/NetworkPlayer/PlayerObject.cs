using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerObject : MonoBehaviour
{
    public class Factory: PlaceholderFactory<PlayerObject>
    {

    }

    public int playerId;
    [SerializeField] private Image image;

    [Inject] private ClientState clientState;

    private void Awake()
    {
        image.color = Color.red;
    }

    void Start()
    {
        clientState.Players.ObserveReplace()
            .TakeUntilDestroy(this)
            .Where(player => player.Key == playerId)
            .Subscribe(player => 
                image.color = player.NewValue.Ready ? Color.green: Color.red
            );

        clientState.Players.ObserveRemove()
            .TakeUntilDestroy(this)
            .Where(player => player.Key == playerId)
            .Subscribe(player =>
                Destroy(this)
            );
    }
}
