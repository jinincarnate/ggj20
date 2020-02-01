using UnityEngine;
using System.Collections.Generic;
using Zenject;
using System.Linq;
using System;

[System.Serializable]
public class LevelInfo {
    public string Names;
    public int InstructionCount;
    public float Timeout;
    public int MaxHealth;
    public const int ButtonCount = 4;
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
    public string Name;
    public ButtonType Type;
    public int Min;
    public int Max;
};

public static class Utils {
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

[System.Serializable]
public class ButtonConfig {
    public List<ButtonInfo> Buttons;

    public static List<ButtonInfo> GetRandomButtons(List<ButtonInfo> list, int n) {
        list.Shuffle();
        return list.GetRange(0,n).ToList();
    }
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
