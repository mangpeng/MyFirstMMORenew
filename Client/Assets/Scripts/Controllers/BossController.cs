using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BossController : CreatureController
{
	Coroutine _coSkill;

	protected override void Init()
	{
		base.Init();

		
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		if(!gameSceneUI.bossUI.gameObject.activeSelf)
        {
            gameSceneUI.bossUI.gameObject.SetActive(true);
            gameSceneUI.bossUI.InitHpUI(Hp, Stat.MaxHp);
        }

    }

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged()
	{
		//Managers.Object.Remove(Id);
		//Managers.Resource.Destroy(gameObject);
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.bossUI.ChangeHp(Hp);
	}

	public override void UseSkill(int skillId)
	{
		if (skillId == 1)
		{
			State = CreatureState.Skill;
		}
	}
}
