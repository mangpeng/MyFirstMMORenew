using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
	enum GameObjects
	{
		AccountName,
	}

	enum Images
	{
		CreateBtn,
		LoginBtn
	}

	ServerInfo Info = new ServerInfo();

    public override void Init()
	{
        base.Init();

        //Info.IpAddress = "192.168.0.16";
		Info.IpAddress = "218.153.112.108";
		Info.Port = 5001;

        Bind<GameObject>(typeof(GameObjects));
		Bind<Image>(typeof(Images));

		GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
		GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
	}

    public void OnClickLoginButton(PointerEventData evt)
    {
		Debug.Log("OnClickLoginButton");
        Managers.Network.ConnectToGame(Info);
    }

    public void OnClickCreateButton(PointerEventData evt)
	{
		Debug.Log("OnClickLoginButton");
		InputField input = Get<GameObject>((int)GameObjects.AccountName).gameObject.GetComponent<InputField>();
		Managers.Network.InputAccountId = input.text;
		input.text = string.Empty;
		
		Managers.Network.ConnectToGame(Info);
	}


}
