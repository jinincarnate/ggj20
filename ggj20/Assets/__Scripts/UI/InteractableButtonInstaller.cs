using UnityEngine;
using Zenject;

public class InteractableButtonInstaller : MonoInstaller
{
    [SerializeField] public GameObject clickButtonPrefab;
    [SerializeField] public GameObject toggleButtonPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<InteractableButton, InteractableButton.ClickButtonFactory>().FromComponentInNewPrefab(clickButtonPrefab).UnderTransform(transform);
        Container.BindFactory<InteractableButton, InteractableButton.ToggleButtonFactory>().FromComponentInNewPrefab(toggleButtonPrefab).UnderTransform(transform);
    }
}