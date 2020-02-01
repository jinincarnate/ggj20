using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerObjectPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<PlayerObject, PlayerObject.Factory>().FromComponentInNewPrefab(playerObjectPrefab).UnderTransform(transform);
    }
}