using UnityEngine;
using Zenject;
using UniRx;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class PlayArea : MonoBehaviour
{
    public Transform buttonsPanel;
    public TMP_Text TextAreaText;
    public Image progressBar;

    // [Inject] private ApplicationState applicationDataState;
    [Inject] private ClientState clientState;
    [Inject] private InteractableButton.ClickButtonFactory clickButtonFactory;
    [Inject] private InteractableButton.ToggleButtonFactory toggleButtonFactory;
    [Inject] private LevelConfig levelConfig;
    [Inject] private ClientListener client;

    [SerializeField]
    private GameObject Loading;
    [SerializeField]
    private TMP_Text LoadingText;

    private float currentProgress;
    private float currentMaxProgress;

    private CompositeDisposable updateDisposable = new CompositeDisposable();

    private List<InteractableButton> buttonList = new List<InteractableButton>();

    private void OnEnable()
    {
        SetupPlayArea();
    }

    private InteractableButton GetButton(ButtonInfo buttonData) {
        InteractableButton interactable = null;
        switch(buttonData.Type) {
            case ButtonType.BUTTON:
                {
                    interactable = clickButtonFactory.Create();
                }
                break;

            case ButtonType.TOGGLE:
                {
                    interactable = toggleButtonFactory.Create();
                }
                break;
            case ButtonType.SLIDER:
                {
                    interactable = toggleButtonFactory.Create();
                }
                break;

            default:
                break;
        }
        interactable?.SetButtonData(buttonData);
        return interactable;
    }

    public void SetupPlayArea()
    {
        var levelObservable = clientState.CurrentLevel
            .Where(level => level != null)
            .DistinctUntilChanged();

        levelObservable
            .TakeUntilDisable(this)
            .Subscribe(level => {

                buttonList.ForEach(button => Destroy(button.gameObject));
                buttonList.Clear();

                foreach(var buttonData in level.Buttons) {
                    var button = GetButton(buttonData);
                    buttonList.Add(button);
                }
                TextAreaText.text = "Wait for instructions";
                // set health to max locally
                var levelCon = levelConfig.LevelInfo[clientState.CurrentLevel.Value.Index];
                var maxHealth = levelCon.MaxHealth;
                clientState.CurrentHealth.Value = maxHealth;

                var texts = levelCon.Names.Split(',');
                var loadingText = texts[UnityEngine.Random.Range(0,texts.Count())];

                LoadingText.text = loadingText;

                });

        levelObservable
            .TakeUntilDisable(this)
            .Do(_ => Loading.SetActive(true))
            .SelectMany(_ => Observable.Timer(TimeSpan.FromSeconds(LevelInfo.WaitTime/2)).First())
            .Subscribe(level => Loading.SetActive(false));

        var instructionStream = clientState.CurrentInstruction
            .Where(buttonInfo => buttonInfo != null)
            .DistinctUntilChanged();

        instructionStream
            .TakeUntilDisable(this)
            .Subscribe(buttonInfo => TextAreaText.text = buttonInfo.ButtonTextOptions.Count > 0 ? buttonInfo.ButtonTextOptions[0] : "Not Found");//TODO:randomise

        instructionStream
            .TakeUntilDisable(this)
            .Select(_ => levelConfig.LevelInfo[clientState.CurrentLevel.Value.Index].Timeout)
            .SelectMany(totalTime => {
                    clientState.ButtonInteractable = true;
                    var timerObs =  Observable.EveryUpdate()
                    .ObserveOnMainThread()
                    .TakeUntil(instructionStream.Skip(1))
                    .TakeUntil(levelObservable.Skip(1));

                    var delta = totalTime;
                    return timerObs
                    .TakeUntil(timerObs.Where(val =>  {
                                delta -= Time.deltaTime;
                                return delta < 0;
                            }))
                    .Select(val => {
                            totalTime -= Time.deltaTime;
                            return totalTime;
                        })
                    .Finally(() => {
                            if(delta <= 0) {
                                client.SendMessage(new NetworkData {
                                        Type = MessageType.TIMER_OVER,
                                        Data = JsonConvert.SerializeObject(clientState.CurrentInstruction.Value)
                                    });
                                clientState.ButtonInteractable = false;
                            }
                        });
                })
            .Subscribe(val => {
                    var max = levelConfig.LevelInfo[clientState.CurrentLevel.Value.Index].Timeout;
                    progressBar.fillAmount = val / max;
                });
    }
}
