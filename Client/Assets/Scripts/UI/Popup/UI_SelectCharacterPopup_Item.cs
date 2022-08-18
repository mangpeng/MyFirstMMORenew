using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectCharacterPopup_Item : MonoBehaviour
{
    [SerializeField] GameObject _empty;
    [SerializeField] GameObject _charater;

    TMP_InputField _characcterNameInput;
    Button _generateCharacter;

    TextMeshProUGUI _characterInfo;
    Button _startButton;

    ClassType _classType = ClassType.None;

    private void Awake()
    {
        _characcterNameInput = _empty.transform.GetChild(0).GetComponent<TMP_InputField>();
        _generateCharacter = _empty.transform.GetChild(1).GetComponent<Button>();

        _characterInfo = _charater.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _startButton = _charater.transform.GetChild(1).GetComponent<Button>();

        _generateCharacter.onClick.AddListener(() => { OnClickCreateCharacter(); });
        _startButton.onClick.AddListener(() => { OnClickStartGame(); });
    }

    public void InitEmpty(ClassType type)
    {
        _classType = type;

        _empty.SetActive(true);
        _charater.SetActive(false);
    }

    public void InitCharacter(LobbyPlayerInfo info)
    {
        _empty.SetActive(false);
        _charater.SetActive(true);

        _characterInfo.text = info.Name;
    }

    private void OnClickCreateCharacter()
    {
        if (string.IsNullOrEmpty(_characcterNameInput.text))
            return;

        C_CreatePlayer createPacket = new C_CreatePlayer();
        createPacket.Name = _characcterNameInput.text;
        createPacket.ClassType = (int)_classType;
        Managers.Network.Send(createPacket);
    }

    private void OnClickStartGame()
    {
        Managers.Scene.LoadScene(Define.Scene.Game);

        C_EnterGame enterGamePacket = new C_EnterGame();
        enterGamePacket.Name = _characterInfo.text;
        enterGamePacket.IsTest = false;
        Managers.Network.Send(enterGamePacket);
    }
}
