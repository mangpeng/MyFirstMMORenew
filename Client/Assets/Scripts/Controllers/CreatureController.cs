using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using static Define;

public class CreatureController : BaseController
{
	HpBar _hpBar;
	DamageEffector _damageTextEffector;

	public override StatInfo Stat
	{
		get { return base.Stat; }
		set { base.Stat = value; UpdateHpBar(); }
	}

	public override int Hp
	{
		get { return Stat.Hp; }
		set { base.Hp = value; UpdateHpBar(); }
	}

	protected void AddHpBar()
	{
		GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
		go.transform.localPosition = new Vector3(0, 0.5f, 0);
		go.name = "HpBar";
		_hpBar = go.GetComponent<HpBar>();
		UpdateHpBar();
	}

    protected void AddDamageEffector()
    {
        GameObject go = Managers.Resource.Instantiate("DamageEffector", transform);
		go.transform.localPosition = Vector3.zero;
        go.name = "DamageEffector";
        _damageTextEffector = go.GetComponent<DamageEffector>();

		//boss 캐릭터의 경우 스케일이 0.001이기 때문에 보정(일반 캐릭터는 스케일이 1이다)
		GameObjectType type = ObjectManager.GetObjectTypeById(Id);
		if(type == GameObjectType.Boss)
        {
			float adjustScale = 1f / transform.localScale.x * 3f;
			_damageTextEffector.transform.localScale = _damageTextEffector.transform.localScale * adjustScale;
        }
    }

    void UpdateHpBar()
	{
		if (_hpBar == null)
			return;

		float ratio = 0.0f;
		if (Stat.MaxHp > 0)
			ratio = ((float)Hp) / Stat.MaxHp;

		_hpBar.SetHpBar(ratio);
	}

	protected override void Init()
	{
		base.Init();
		AddHpBar();
		AddDamageEffector();
	}

	public virtual void OnDamaged()
	{

	}

	public  void PlayDamageTextEffect(int damage, bool isCritical, GameObjectType objectType)
    {
		if (_damageTextEffector == null)
			return;

        if (isCritical)
        {
			Debug.Log("ciritical!");
            _damageTextEffector.GenerateCriticalText(damage, Color.red, 0.1f);
        }
        else
			_damageTextEffector.GenerateText(damage, Color.red, 1f, 1f);
    }

    public void PlayRecoverTextEffect(int recover, bool isCritical, GameObjectType objectType)
    {
        if (_damageTextEffector == null)
            return;

        if (isCritical)
        {
            _damageTextEffector.GenerateCriticalText(recover, Color.blue, 0.1f);
        }
        else
        {
			Debug.Log("recover!!");
            _damageTextEffector.GenerateText(recover, Color.blue, 1f, 1f);
        }
    }

    public virtual void OnDead()
	{
		State = CreatureState.Dead;

		GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
		effect.transform.position = transform.position;
		effect.GetComponent<Animator>().Play("START");
		GameObject.Destroy(effect, 0.5f);
	}

	public virtual void UseSkill(SkillInfo info)
	{

	}
}
