using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameStatesInstaller", menuName = "Installers/GameStatesInstaller")]
public class GameStatesInstaller : ScriptableObjectInstaller<GameStatesInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ApplicationState>().AsSingle().NonLazy();
    }
}
