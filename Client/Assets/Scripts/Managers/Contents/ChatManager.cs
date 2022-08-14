using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class ChatManager
{
    ChatScroll _chatScroll;
    public bool IsChat
    {
        get
        {
            if (_chatScroll == null)
                _chatScroll = GameObject.FindObjectOfType<ChatScroll>();

            return _chatScroll.IsChat;
        }
    }

    public bool IsActiveInput
    {
        get
        {
            if (_chatScroll == null)
                _chatScroll = GameObject.FindObjectOfType<ChatScroll>();

            return _chatScroll.inputField.isFocused;
        }
    }

    public void RecvChatPacket(S_Chat packet)
    {
        if (_chatScroll == null)
            _chatScroll = GameObject.FindObjectOfType<ChatScroll>();

        string playerId = packet.ObjectId.ToString();
        string msg = packet.Message;

        if (Managers.Object.MyPlayer.Id == packet.ObjectId)
        {
            _chatScroll.AddChat($"[{playerId}] {msg}");

        }
        else
        {
            _chatScroll.AddChat($"<color=yellow>[{playerId}] {msg}</color>");
        }

        PlayerController p = Managers.Object.FindById(packet.ObjectId).GetComponent<PlayerController>();
        p.ShowChatBox(msg);
    }
}
