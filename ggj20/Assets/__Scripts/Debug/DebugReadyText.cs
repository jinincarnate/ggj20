using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class DebugReadyText : MonoBehaviour {

    [Inject]
    private ClientState clientState;

    [SerializeField]
    private Text text;

    private void Awake() {
        clientState.Players.ObserveAdd()
            .TakeUntilDestroy(this)
            .Subscribe(HandleAdd);

        clientState.Players.ObserveReplace()
            .TakeUntilDestroy(this)
            .Subscribe(HandleReplace);
    }

    private void HandleAdd(DictionaryAddEvent<int, PlayerData> player) {
        SetText();
    }

    private void HandleReplace(DictionaryReplaceEvent<int, PlayerData> player) {
        SetText();
    }

    private void SetText() {
        var str = "";
        foreach(KeyValuePair<int, PlayerData> kvp in clientState.Players) {
            str += $"Player {kvp.Value.Id}: {kvp.Value.Ready}, ";
        }
        text.text = str;
    }
}
