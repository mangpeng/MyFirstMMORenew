using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class GameInfoManager : MonoBehaviour 
{
    private UI_GameScene _gameSceneUI;
    public UI_GameScene GameSceneUI
    {
        get
        {
            if (_gameSceneUI == null)
                _gameSceneUI = FindObjectOfType<UI_GameScene>();

            return _gameSceneUI;
        }
        set
        {
            _gameSceneUI = value;
        }
    }

    public void SetPingText(int pingMs)
    {
        if (GameSceneUI == null)
            return;

        GameSceneUI.gameInfo.SetPingText(pingMs);
    }

    public void SetCoordText(Vector2Int coord)
    {
        if (GameSceneUI == null)
            return;

        GameSceneUI.gameInfo.SetCoordText(coord);
    }

}
