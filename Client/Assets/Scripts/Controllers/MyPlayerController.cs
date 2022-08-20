using Google.Protobuf.Protocol;
using System;
using System.Collections;
using UnityEngine;


public class MyPlayerController : PlayerController
{
	bool _moveKeyPressed = false;

    public override int MaxHp { get { return Stat.MaxHp + AddHp; } }
    public override int Attack { get { return Stat.Attack + AddDamage; } }
	public override int Defense { get { return Stat.Defense + AddDefence; } }
	public override int MoveSpeed { get { return Stat.MoveSpeed + AddMoveSpeed; } }
	public override int Critical { get { return Stat.Critical + AddCritical; } }
	public override int CriticalDamage { get { return Stat.CriticalDamage + AddCriticalDamage; } }

	public int AddHp { get; private set; }
	public int AddDamage { get; private set; }
	public int AddDefence { get; private set; }
	public int AddMoveSpeed { get; private set; }
	public int AddCritical { get; private set; }
	public int AddCriticalDamage { get; private set; }

    const int visionRange = 3;
	const int cameraSafeArea = 7;

	ItemSetType _activeSetsEffect = ItemSetType.None;
	GameObject _setsEffect = null;

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

                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                gameSceneUI.SkillUI.HideGuid();

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

        if (_coSkillCooltime == null)
		{
			if(Input.GetKey(KeyCode.Space))
            {
                C_Skill skill = new C_Skill() { Info = new SkillInfo() };
                skill.Info.SkillId = NormalSkillData.id;
                Managers.Network.Send(skill);

                _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
				UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
				gameSceneUI.SkillUI.OnClickSKillActive(null);
			}
            else if (Input.GetKeyDown(KeyCode.W))
            {
                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                gameSceneUI.SkillUI.OnClickSKillBuff(null);
            }
            //else if (Input.GetKeyDown(KeyCode.E))
            //{
            //    UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            //    gameSceneUI.SkillUI.OnClickSKillPotion(null);
            //}
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

    protected override void AddHpMpBar()
    {
        base.AddHpMpBar();

        {
            //GameObject go = Managers.Resource.Instantiate("UI/MpBar", transform);
            //go.transform.localPosition = new Vector3(0, -0.6f, 0);
            //go.name = "MpBar";
            //MpBar = go.GetComponent<HpMpBar>();
        }
    }

    public void RefreshAdditionalStat()
	{
		AddHp = 0;

		AddDamage = 0;
		AddDefence = 0;

		AddMoveSpeed = 0;

		AddCritical = 0;
		AddCriticalDamage = 0;

		int normalSetsCount = 0;
		int forestSetsCount = 0;

		foreach (Item item in Managers.Inven.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			if (item.ItemSetType == ItemSetType.Normal)
				++normalSetsCount;
			else if (item.ItemSetType == ItemSetType.Forest)
				++forestSetsCount;

			switch (item.ItemType)
			{
				case ItemType.Weapon:
					AddDamage += ((Weapon)item).AddDamaged;
					break;
				case ItemType.Armor:
					AddDefence += ((Armor)item).AddDefence;
					AddHp += ((Armor)item).AddHp;
					AddMoveSpeed += ((Armor)item).AddMoveSpeed;
					break;
                case ItemType.Accessory:
                    AddCritical += ((Accessory)item).AddCritical;
					AddCriticalDamage += ((Accessory)item).AddCriticalDamage;
					break;
            }
		}

		if(normalSetsCount == 6)
			ActiveNoramlSetEffect();
        else if (forestSetsCount == 6)
            ActiveForestSetEffect();
        else
        {
            if (_setsEffect != null)
                Managers.Resource.Destroy(_setsEffect);
        }

        UpdateHpBar();
	}

    private void ActiveNoramlSetEffect()
    {
		if(_activeSetsEffect != ItemSetType.Normal)
        {
			if (_setsEffect != null)
				Managers.Resource.Destroy(_setsEffect);

			_setsEffect = Managers.Resource.Instantiate("Particle/SetEffectNormal", transform);
			_setsEffect.transform.localPosition = new Vector3(0, -0.5f, 0);
        }
    }

    private void ActiveForestSetEffect()
    {
        if (_activeSetsEffect != ItemSetType.Forest)
        {
            if (_setsEffect != null)
                Managers.Resource.Destroy(_setsEffect);

            _setsEffect = Managers.Resource.Instantiate("Particle/SetEffectForest", transform);
            _setsEffect.transform.localPosition = new Vector3(0, -0.5f, 0);
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
