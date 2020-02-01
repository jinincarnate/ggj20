using UnityEngine;
using System.Collections.Generic;
using Zenject;

[System.Serializable]
public class LevelInfo {
    public int ButtonCount;
    public int InstructionCount;
    public float Timeout;
    public int MaxHealth;
};

[System.Serializable]
public class LevelConfig {
    public List<LevelInfo> LevelInfo;
};

public enum ButtonType {
    BUTTON,
    TOGGLE,
    SLIDER
};

[System.Serializable]
public class ButtonInfo {
    public ButtonType Type;
    public string Name;
    public int Min;
    public int Max;
};

[System.Serializable]
public class ButtonConfig {
    public List<ButtonInfo> Buttons;
};

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Installers/LevelSettings")]
public class LevelSettings : ScriptableObjectInstaller<LevelSettings> {

    [SerializeField]
    private LevelConfig levels;

    [SerializeField]
    private ButtonConfig buttons;

    public override void InstallBindings() {
        Container.BindInstance(levels);
        Container.BindInstance(buttons);
    }
}
