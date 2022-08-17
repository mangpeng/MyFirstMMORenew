using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;


public class MyPlayerController : PlayerController
{
	bool _moveKeyPressed = false;

	public int WeaponDamage { get; private set; }
	public int ArmorDefence { get; private set; }

	const int visionRange = 3;
	const int cameraSafeArea = 7;

	protected override void Init()
	{
		base.Init();
		RefreshAdditionalStat();

		LoadPartialMap(visionRange);
	}

	protected override void UpdateController()
	{
		GetUIKeyInput();

		switch (State)
		{
			case CreatureState.Idle:
				GetDirInput();
				break;
			case CreatureState.Moving:
				GetDirInput();
				break;
		}

		base.UpdateController();
	}

	protected override void UpdateIdle()
	{
		// 이동 상태로 갈지 확인
		if (_moveKeyPressed)
		{
			State = CreatureState.Moving;
			return;
		}

        if (Managers.Chat.IsChat)
            return;

        if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space))
		{
			C_Skill skill = new C_Skill() { Info = new SkillInfo() };
			skill.Info.SkillId = 2;
			Managers.Network.Send(skill);

			_coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);
		}
	}

	Coroutine _coSkillCooltime;
	IEnumerator CoInputCooltime(float time)
	{
		yield return new WaitForSeconds(time);
		_coSkillCooltime = null;
	}

	void LateUpdate()
	{
		float min = Managers.Map.MinY + cameraSafeArea;
		float max = Managers.Map.MaxY - cameraSafeArea;
		float yPos = Mathf.Clamp(transform.position.y, min, max);
		Vector3 camPos = new Vector3(transform.position.x, yPos, -10);

		Camera.main.transform.position = camPos;
	}

	void GetUIKeyInput()
	{
        if (Managers.Chat.IsChat)
            return;

        if (Input.GetKeyDown(KeyCode.I))
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
		else if (Input.GetKeyDown(KeyCode.C))
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
		else if( Input.GetKeyDown(KeyCode.F))
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

	// 키보드 입력
	void GetDirInput()
	{
        if (Managers.Chat.IsChat)
            return;

        _moveKeyPressed = true;

		if (Input.GetKey(KeyCode.UpArrow))
		{
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			Dir = MoveDir.Right;
		}
		else
		{
			_moveKeyPressed = false;
		}
	}

	protected override void MoveToNextPos()
	{
		if (_moveKeyPressed == false)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3Int destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				destPos += Vector3Int.up;
				break;
			case MoveDir.Down:
				destPos += Vector3Int.down;
				break;
			case MoveDir.Left:
				destPos += Vector3Int.left;
				break;
			case MoveDir.Right:
				destPos += Vector3Int.right;
				break;
		}

		if (Managers.Map.CanGo(destPos))
		{
			if (Managers.Object.FindCreature(destPos) == null)
			{
				CellPos = destPos;
			}
		}

		CheckUpdatedFlag();
		LoadPartialMap(visionRange);
	}

	protected override void CheckUpdatedFlag()
	{
		if (_updated)
		{
			C_Move movePacket = new C_Move();
			movePacket.PosInfo = PosInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
		}
	}

	public void RefreshAdditionalStat()
	{
		WeaponDamage = 0;
		ArmorDefence = 0;

		foreach (Item item in Managers.Inven.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			switch (item.ItemType)
			{
				case ItemType.Weapon:
					WeaponDamage += ((Weapon)item).Damage;
					break;
				case ItemType.Armor:
					ArmorDefence += ((Armor)item).Defence;
					break;
			}
		}
	}

	private void LoadPartialMap(int range)
    {
		Managers.Map.ToggleDivision(CellPos, range);
    }

    public override void ShowChatBox(string message)
    {
        _chatPopup.Show(message);
    }
}
