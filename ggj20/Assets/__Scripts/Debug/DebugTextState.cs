using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class DebugTextState : MonoBehaviour {

    [Inject]
    private ClientState clientState;

    [SerializeField]
    private Text text;

    private void Start() {
        clientState.ConnectionState
            .TakeUntilDestroy(this)
            .Subscribe(s => {
                    text.text = s.ToString();
                });
    }
}
