using UnityEngine;
using Zenject;
using LiteNetLib;

[CreateAssetMenu(fileName = "LocalServerInstaller", menuName = "Installers/LocalServerInstaller")]
public class LocalServerInstaller : ScriptableObjectInstaller<LocalServerInstaller>
{

    [SerializeField]
    private LocalServer.LocalServerSettings settings;

    public override void InstallBindings() {

        // move all this to a client settings installer
        Container.BindInstance(settings);
        Container.BindInterfacesAndSelfTo<ClientListener>().AsSingle();
        Container.BindInterfacesAndSelfTo<ClientState>().AsSingle();
        Container.BindInterfacesAndSelfTo<ClientManager>().AsSingle().NonLazy();

        Container.BindFactory<LocalServer,LocalServer.Factory>();
        Container.BindFactory<int, NetPeer, Player,Player.Factory>();
        // only referenced within local server,
        // so, hopefully, never created on a normal client
        Container.BindInterfacesAndSelfTo<ServerState>().AsSingle();
        Container.BindInterfacesAndSelfTo<ServerManager>().AsSingle();
    }
}
