using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_GameInfo : UI_Base
{
    [SerializeField]
    private TextMeshProUGUI _coordText;
    [SerializeField]
    private TextMeshProUGUI _pingText;
    [SerializeField]
    private TextMeshProUGUI _frameText;

    public override void Init()
    {

    }

    private void Update()
    {
        int fps = (int)(1/Time.deltaTime);
        _frameText.text = $"{fps} fps";
    }

    public void SetCoordText(Vector2Int pos)
    {
        _coordText.text = $"({pos.x}/{pos.y})";
    }

    public void SetPingText(int pingMs)
    {
        _pingText.text = $"{pingMs}ms";
    }
}
