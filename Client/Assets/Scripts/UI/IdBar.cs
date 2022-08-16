using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IdBar : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;

    public void SetText(string text)
    {
        _text.text = text;
    }
}
