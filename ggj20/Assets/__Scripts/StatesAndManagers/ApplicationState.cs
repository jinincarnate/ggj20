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

    private CompositeDisposable disposable = new CompositeDisposable();

    public void Initialize()
    {
        TestButton();
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
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
