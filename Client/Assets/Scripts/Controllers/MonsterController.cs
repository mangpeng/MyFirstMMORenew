using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
	Coroutine _coSkill;
    AnimatorByParts _monsterAnimator;

    public Skill NormalSkillData = null;

    protected override void Init()
	{
		base.Init();
        _monsterAnimator = GetComponent<AnimatorByParts>();
    }

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged()
	{
		//Managers.Object.Remove(Id);
		//Managers.Resource.Destroy(gameObject);
	}

	public override void UseSkill(SkillInfo info)
	{
		if (info.SkillId == NormalSkillData.id)
		{
			State = CreatureState.Skill;
		}
	}

    
    protected override void UpdateAnimation()
    {
        if (_monsterAnimator == null)
            return;

        if (State == CreatureState.Idle)
        {
            _monsterAnimator.SetSate(AnimatorByParts.State.IDLE, Dir);
        }
        else if (State == CreatureState.Moving)
        {
            _monsterAnimator.SetSate(AnimatorByParts.State.WALK, Dir);
        }
        else if (State == CreatureState.Skill)
        {
            _monsterAnimator.SetSate(AnimatorByParts.State.ATTACK, Dir);
        }
    }
}
