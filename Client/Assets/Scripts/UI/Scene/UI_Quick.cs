using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Quick : UI_Base
{
    enum Buttons
    {
        InfoButton,
        InvenButton,
        FriendButton,
    }

    public override void Init()
	{
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.InfoButton).onClick.AddListener(() => { OnClickInfoButton(); });
        GetButton((int)Buttons.InvenButton).onClick.AddListener(() => { OnClickInvenButton(); });
        GetButton((int)Buttons.FriendButton).onClick.AddListener(() => { OnClickFriendButton(); });
    }

    public void OnClickInfoButton()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_Stat statUI = gameSceneUI.StatUI;

        if (statUI.gameObject.activeSelf)
        {
            statUI.gameObject.SetActive(false);
        }
        else
        {
            statUI.gameObject.SetActive(true);
            statUI.RefreshUI();
        }
    }

    public void OnClickInvenButton()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_Inventory invenUI = gameSceneUI.InvenUI;

        if (invenUI.gameObject.activeSelf)
        {
            invenUI.gameObject.SetActive(false);
        }
        else
        {
            invenUI.gameObject.SetActive(true);
            invenUI.RefreshUI();
        }
    }

    public void OnClickFriendButton()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_FriendList friendUI = gameSceneUI.FriendUI;

        if (friendUI.gameObject.activeSelf)
        {
            friendUI.gameObject.SetActive(false);
        }
        else
        {
            friendUI.gameObject.SetActive(true);
        }
    }
}
