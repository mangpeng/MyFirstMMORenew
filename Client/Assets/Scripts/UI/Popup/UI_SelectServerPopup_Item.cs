using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
	public ServerInfo Info { get; set; }

	enum Buttons
	{
		SelectServerButton
	}

	enum Texts
	{ 
		NameText,
		CrowdText,
	}

	public override void Init()
	{
		Bind<Button>(typeof(Buttons));
		Bind<Text>(typeof(Texts));

		GetButton((int)Buttons.SelectServerButton).gameObject.BindEvent(OnClickButton);
	}

	public void RefreshUI()
	{
		if (Info == null)
			return;

		GetText((int)Texts.NameText).text = Info.Name;
		GetText((int)Texts.CrowdText).text = Info.BusyScore.ToString();
	}

	void OnClickButton(PointerEventData evt)
	{
		Managers.Network.ConnectToGame(Info);
		//Managers.Scene.LoadScene(Define.Scene.Game); => S_LoginHandler로 이동 mapID를 받기 위해서
		Managers.UI.ClosePopupUI();
	}
}
