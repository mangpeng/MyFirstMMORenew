using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SelectCharacterPopup : MonoBehaviour
{
    [SerializeField] UI_SelectCharacterPopup_Item _archer;
    [SerializeField] UI_SelectCharacterPopup_Item _knight;


    private void InitEmpty()
    {
        _archer.InitEmpty(ClassType.Archer);
        _knight.InitEmpty(ClassType.Knight);
    }

    public void Refresh(List<LobbyPlayerInfo> players)
    {
        InitEmpty();

        foreach(var info in players)
        {
            if ((ClassType)info.ClassType == ClassType.Archer)
            {
                _archer.InitCharacter(info);
            }
            else if ((ClassType)info.ClassType == ClassType.Knight)
            {
                _knight.InitCharacter(info);
            }
        }
        
    }
}
