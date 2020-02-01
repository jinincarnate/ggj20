using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UniRx;
using Zenject;

public class ApplicationState : IInitializable, IDisposable
{

    public enum GameState
    {
        MainMenu,
        LobbyLoading,
        LobbyWaiting,
        Playing
    }

    public ReactiveProperty<GameState> currentGameState = new ReactiveProperty<GameState>(GameState.MainMenu);
    public GameData currentGameData;
    public Subject<string> ApplicationDataReceivedSubject = new Subject<string>();

    private CompositeDisposable disposable = new CompositeDisposable();

    public void Initialize()
    {
        ApplicationDataReceivedSubject
            .Subscribe(data =>
            {
                JObject jObj = JObject.Parse(data);
                HandleApplicationDataReceived(jObj["Type"].ToString(), jObj["Data"].ToString());
            })
            .AddTo(disposable);
        TestButton();
    }

    private void HandleApplicationDataReceived(string messageType, string data)
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

    public void Dispose()
    {
        disposable.Dispose();
    }
}
