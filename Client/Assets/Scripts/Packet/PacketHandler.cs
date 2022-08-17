using Data;
using DG.Tweening;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;

        if (enterGamePacket.MapId != Managers.Map.Id)
        {
			Managers.Map.Id = enterGamePacket.MapId;
            Managers.Map.LoadMap(Managers.Map.Id);
        }

		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
		Managers.Object.Clear();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
        S_Spawn spawnPacket = packet as S_Spawn;
        foreach (ObjectInfo obj in spawnPacket.Objects)
            Managers.Object.Add(obj, myPlayer: false);
    }

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		foreach (int id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}




	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Object.FindById(movePacket.ObjectId);
		if (go == null)
			return;

		if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
        {
			Vector2Int cellPos = new Vector2Int(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY);
			Managers.Info.SetCoordText(cellPos);
			return;
        }

		BaseController bc = go.GetComponent<BaseController>();
		if (bc == null)
			return;

		bc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.UseSkill(skillPacket.Info);
		}
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;

		GameObject go = Managers.Object.FindById(changePacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
        {
			GameObjectType type = ObjectManager.GetObjectTypeById(cc.Id);
			if (changePacket.Hp < cc.Hp) // 피격
            {
				int damage = cc.Hp - changePacket.Hp;
				cc.PlayDamageTextEffect(damage, changePacket.IsCritical, type);
            }
			else if(changePacket.Hp > cc.Hp) // 회복
            {
				int recovery = changePacket.Hp - cc.Hp;
                cc.PlayRecoverTextEffect(recovery, changePacket.IsCritical, type);
            }

			cc.Hp = changePacket.Hp;


			if(type == GameObjectType.Boss)
            {
                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                gameSceneUI.bossUI.ChangeHp(changePacket.Hp);
				cc.gameObject.Blink(0.3f, 0.1f, 4, Color.white);

				Transform target = cc.transform.GetChild(0);
				Vector3 originScale = target.localScale;
				cc.transform.GetChild(0).DOScale(originScale * 1.05f, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InSine)
					.onComplete += () =>
                    {
						cc.transform.GetChild(0).localScale = originScale;
                    };
			}
		}
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;

		GameObject go = Managers.Object.FindById(diePacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.Hp = 0;
			cc.OnDead();
		}
	}

	
	
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");
		C_Login loginPacket = new C_Login();

		string path = Application.dataPath;
		loginPacket.UniqueId = path.GetHashCode().ToString();
		Managers.Network.Send(loginPacket);
	}

	// 로그인 OK + 캐릭터 목록
	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

		// 해당 계정의 캐릭터가 없으면 캐릭터 생성 요청을 보낸다.
		if (loginPacket.Players == null || loginPacket.Players.Count == 0)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPacket);
		}
		else // 해당 계정의 캐릭터가 있으면 첫번째 캐릭터로 게임입장 시도 한다.
		{
			// 무조건 첫번째 로그인
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			enterGamePacket.IsTest = false;

            Managers.Map.Id = loginPacket.MapId;
            Managers.Scene.LoadScene(Define.Scene.Game);
            Managers.Network.Send(enterGamePacket);
        }
	}

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

		if (createOkPacket.Player == null)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPacket);
		}
		else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createOkPacket.Player.Name;
			Managers.Network.Send(enterGamePacket);
		}
	}

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemList = (S_ItemList)packet;

		Managers.Inven.Clear();

		// 메모리에 아이템 정보 적용
		Debug.Log($"ItemList : {itemList.Items.Count}");
		foreach (ItemInfo itemInfo in itemList.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.SkillUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemList = (S_AddItem)packet;

		// 메모리에 아이템 정보 적용
		foreach (ItemInfo itemInfo in itemList.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

		Debug.Log("아이템을 획득했습니다!");

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();
		gameSceneUI.SkillUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem itemList = (S_RemoveItem)packet;

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Remove(item);
        }

        Debug.Log("아이템을 소모했습니다!");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();
        gameSceneUI.SkillUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

       

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;

		// 메모리에 아이템 정보 적용
		Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
		if (item == null)
			return;

		item.Equipped = equipItemOk.Equipped;
		Debug.Log("아이템 착용 변경!");

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat itemList = (S_ChangeStat)packet;

		// TODO
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{

		// tick 계산
		int pingpongCycleMs = 5000;
		if(Managers.Network.LastPingTick != 0)
        {
			int curTick = System.Environment.TickCount;
			int ping = Mathf.Abs(curTick - Managers.Network.LastPingTick);
			Managers.Info.SetPingText(ping);
			Managers.Network.LastPingTick = curTick + pingpongCycleMs;
        }
        else
        {
			Managers.Network.LastPingTick = System.Environment.TickCount + pingpongCycleMs;
		}

		C_Pong pongPacket = new C_Pong();
		Debug.Log("[Server] PingCheck");
		Managers.Network.Send(pongPacket);
	}

    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
		S_Chat chatPacket = (S_Chat)packet;

		GameScene gameScene = (GameScene)Managers.Scene.CurrentScene;
		Debug.Log($"rev message : {chatPacket.Message}");
		Managers.Chat.RecvChatPacket(chatPacket);
	}

    public static void S_UsePotionHandler(PacketSession session, IMessage packet)
    {
        S_UsePotion usePotionPacket = (S_UsePotion)packet;
		Debug.Log($"S_UsePotion {usePotionPacket.ObjectId}");
		GameObject go = Managers.Object.FindById(usePotionPacket.ObjectId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
			pc.ShowRecoverEffect();
        }
    }
}


