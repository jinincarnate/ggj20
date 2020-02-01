using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    public void SetButtonText(string text)
    {
        buttonText.text = text;
    }
}
