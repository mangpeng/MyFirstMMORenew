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

		_idBar.SetText(name);
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
		_coSkill = StartCoroutine("CoStartPunch");
	}

	private void PlaySkillNormalArrow(SkillInfo info)
    {
		_coSkill = StartCoroutine("CoStartShootArrow");
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
        _coSkill = StartCoroutine("CoStartShootArrow");
    }

	private void PlaySkillArcherBuff(SkillInfo info)
    {
		
	}

    private void PlaySkillKnightActive(SkillInfo info)
    {
        GameObject go = Managers.Resource.Instantiate("Effect/SwordSlashEffect");

        go.transform.position = transform.position;
        Vector3 temp = go.transform.localEulerAngles;
        if (Dir == MoveDir.Up)
        {
            temp.z = -180;
        }
        else if (Dir == MoveDir.Down)
        {
            temp.z = 0;
        }
        else if (Dir == MoveDir.Right)
        {
            temp.z = 90;
        }
        else if (Dir == MoveDir.Left)
        {
            temp.z = -90;
        }
        go.transform.localEulerAngles = temp;

        Managers.Resource.DestroyAfter(go, 0.5f);
        _coSkill = StartCoroutine("CoStartPunch");
	}

    private void PlaySkillKnightBuff(SkillInfo info)
    {
        Debug.LogWarning($"아직 구현되지 않은 스킬 id {info.SkillId}");
    }


    IEnumerator CoStartPunch()
    {
        // 대기 시간
        _rangedSkill = false;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }

    IEnumerator CoStartShootArrow()
    {
        // 대기 시간
        _rangedSkill = true;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }
}
