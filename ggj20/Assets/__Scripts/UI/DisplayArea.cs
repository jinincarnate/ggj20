using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Zenject;

public class DisplayArea : MonoBehaviour {

    [Inject] private ClientState clientState;
    [Inject] private LevelConfig levelConfig;

    [SerializeField]
    private float minX;
    [SerializeField]
    private float maxX;

    [SerializeField]
    private Image virusBar;

    private void OnEnable() {
        clientState.CurrentHealth
            .Where(health => health >= 0)
            .TakeUntilDisable(this)
            .Subscribe(health => {
                    var level = levelConfig.LevelInfo[clientState.CurrentLevel.Value.Index];
                    var maxHealth = level.MaxHealth;
                    var xVal = maxX - (maxX-minX)*health/maxHealth;
                    virusBar.fillAmount = xVal;
                });
    }
}
