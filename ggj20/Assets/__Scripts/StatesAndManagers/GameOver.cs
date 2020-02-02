using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Button restartLevel;

    private void OnEnable()
    {
        restartLevel.OnClickAsObservable()
            .TakeUntilDisable(this)
            .Subscribe(_ =>
            {
                restartLevel.interactable = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
    }
}
