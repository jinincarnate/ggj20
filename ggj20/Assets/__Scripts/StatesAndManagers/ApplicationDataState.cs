using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class ApplicationDataState : MonoBehaviour
{
    public static ApplicationDataState applicationDataState;

    public GameData currentGameData;

    public Subject<string> ApplicationDataReceivedSubject = new Subject<string>();

    private void Awake()
    {
        if(applicationDataState != null && applicationDataState != this)
        {
            Destroy(this);
            return;
        }
        applicationDataState = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        ApplicationDataReceivedSubject
            .TakeUntilDestroy(this)
            .Subscribe(data =>
            {
                JObject jObj = JObject.Parse(data);
                HandleApplicationDataReceived(jObj["Type"].ToString(), jObj["Data"].ToString());
            });
        TestButton();
    }

    void HandleApplicationDataReceived(string messageType, string data)
    {
        switch(messageType)
        {
            case "LevelStart":
                currentGameData = JsonConvert.DeserializeObject<GameData>(data);
                break;
            default:
                break;
        }
    }

    public void TestButton()
    {
        string json = @"{
              'Type': 'LevelStart',
              'Data': 
              {
                'stageName': 'Cough and Cold',
                'buttonData': 
                 [
                     {
                         'buttonName': 'sneeze',
                         'buttonType': 'click'
                     },
                     {
                         'buttonName': 'sweeat',
                         'buttonType': 'toggle'
                     },
                     {
                         'buttonName': 'cough',
                         'buttonType': 'click'
                     },
                     {
                         'buttonName': 'raise temprate',
                         'buttonType': 'toggle'
                     }
                 ]
              }
            }";
        ApplicationDataReceivedSubject.OnNext(json);
    }

}
