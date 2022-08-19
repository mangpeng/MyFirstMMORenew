using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;
	protected bool _rangedSkill = false;

	protected UI_ChatPopup _chatPopup;
	protected IdBar _idBar;

	public ClassType ClassType = ClassType.None;
	public Skill NormalSkillData = null;
	public Skill ActiveSkillData = null;
	public Skill BuffSkillData = null;

	protected override void Init()
	{
		base.Init();

		_chatPopup = GetComponentInChildren<UI_ChatPopup>();
		_idBar = GetComponentInChildren<IdBar>();

		SetIdText(name);
	}

    public virtual void SetIdText(string id)
    {
		_idBar.SetText($"<color=yellow>{id}</color>");
	}

    protected override void UpdateAnimation()
	{
		if (_animator == null || _sprite == null)
			return;

		if (State == CreatureState.Idle)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("IDLE_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("IDLE_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Moving)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("WALK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("WALK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Skill)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play((_rangedSkill && ClassType == ClassType.Archer) ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play((_rangedSkill && ClassType == ClassType.Archer) ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play((_rangedSkill && ClassType == ClassType.Archer) ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play((_rangedSkill && ClassType == ClassType.Archer) ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else
		{

		}
	}

	protected override void UpdateController()
	{		
		base.UpdateController();
	}

	public override void UseSkill(SkillInfo info)
	{
		switch(info.SkillId)
        {
			// normal
			case Const.SKILL_FIST:
				PlaySkillNormalFist(info);
				break;
            case Const.SKILL_ARROW:
				PlaySkillNormalArrow(info);
				break;
            // archer
            case Const.SKILL_ARCHER_ACTIVE:
				PlaySkillArcherActive(info);
                break;
            case Const.SKILL_ARCHER_BUFF:
				PlaySkillArcherBuff(info);
                break;
            // knight
            case Const.SKILL_KNIGHT_ACTIVE:
                PlaySkillKnightActive(info);
                break;
            case Const.SKILL_KNIGHT_BUFF:
                PlaySkillKnightBuff(info);
                break;

        }
	}

	protected virtual void CheckUpdatedFlag()
	{

	}

	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
	}

	public virtual void ShowChatBox(string message)
    {
		_chatPopup.Show(message);
    }

    public void ShowRecoverEffect()
    {
		GameObject effect = Managers.Resource.Instantiate("Particle/RecoverEffect", transform);
		effect.transform.localPosition = new Vector3(0, -0.3f, 0);
    }

	private void PlaySkillNormalFist(SkillInfo info)
    {
		_coSkill = StartCoroutine(CoStartPunch(0.5f));
	}

	private void PlaySkillNormalArrow(SkillInfo info)
    {
		_coSkill = StartCoroutine(CoStartShootArrow(0.3f));
	}

    private void PlaySkillArcherActive(SkillInfo info)
    {
        GameObject go = Managers.Resource.Instantiate("Effect/ArrowEffect");

        go.transform.position = transform.position;
        Vector3 temp = go.transform.localEulerAngles;
        if (Dir == MoveDir.Up)
        {
            temp.z = -90;
        }
        else if (Dir == MoveDir.Down)
        {
            temp.z = 90;
        }
        else if (Dir == MoveDir.Right)
        {
            temp.z = 180;
        }
        else if (Dir == MoveDir.Left)
        {
            temp.z = 0;
        }
        go.transform.localEulerAngles = temp;

        Managers.Resource.DestroyAfter(go, 0.5f);
        _coSkill = StartCoroutine(CoStartShootArrow(0.3f));
    }

	private void PlaySkillArcherBuff(SkillInfo info)
    {
		
	}

    private void PlaySkillKnightActive(SkillInfo info)
    {
        GameObject go = Managers.Resource.Instantiate("Effect/SwordSlashEffect");

        go.transform.position = transform.position;
        Vector3 tempAngle = go.transform.localEulerAngles;
		Vector3 tempSacle = Vector3.one;
        if (Dir == MoveDir.Up)
        {
            tempAngle.z = -180;
			tempSacle.x = 1;
		}
        else if (Dir == MoveDir.Down)
        {
            tempAngle.z = 0;
			tempSacle.x = 1;
		}
        else if (Dir == MoveDir.Right)
        {
            tempAngle.z = 0;
			tempSacle.x = -1;
			tempSacle.y = 1;
		}
        else if (Dir == MoveDir.Left)
        {
			tempAngle.z = 0;
            tempSacle.x = 1;
            tempSacle.y = -1;
        }
        go.transform.localEulerAngles = tempAngle;
		go.transform.localScale = tempSacle;

        Managers.Resource.DestroyAfter(go, 0.5f);
        _coSkill = StartCoroutine(CoStartPunch(0.5f));
	}

    private void PlaySkillKnightBuff(SkillInfo info)
    {
        Debug.LogWarning($"아직 구현되지 않은 스킬 id {info.SkillId}");
    }


    IEnumerator CoStartPunch(float duration)
    {
        // 대기 시간
        _rangedSkill = false;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(duration);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }

    IEnumerator CoStartShootArrow(float duration)
    {
        // 대기 시간
        _rangedSkill = true;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(duration);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }


}
