using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "LocalServerInstaller", menuName = "Installers/LocalServerInstaller")]
public class LocalServerInstaller : ScriptableObjectInstaller<LocalServerInstaller>
{

    [SerializeField]
    private LocalServer.LocalServerSettings settings;

    public override void InstallBindings() {
        Container.BindInstance(settings);
        Container.BindInterfacesAndSelfTo<ClientListener>().AsSingle();
        Container.BindInterfacesAndSelfTo<ClientState>().AsSingle();
        Container.BindInterfacesAndSelfTo<ClientManager>().AsSingle().NonLazy();

        Container.BindFactory<LocalServer,LocalServer.Factory>();
    }
}
