using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpMpBar : MonoBehaviour
{
    [SerializeField]
    Transform _hpBar = null;

    public void SetHpMpBar(float ratio)
	{
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
	}
}
