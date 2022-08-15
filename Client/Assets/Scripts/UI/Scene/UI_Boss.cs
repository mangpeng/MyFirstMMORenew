using System;
using UnityEngine;

public class UI_Boss : UI_Base
{
	[SerializeField]
	private HpSlider _hpSlider;



	public override void Init()
	{
		_hpSlider.ResetValue();
		
	}

    public void InitHpUI(int curHp, int maxHp)
    {
        _hpSlider.Init(curHp, maxHp);
    }

    internal void ChangeHp(int hp)
    {
        _hpSlider.Change(hp);
    }
}
