using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
	public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
	{
		return Util.GetOrAddComponent<T>(go);
	}

	public static void BindEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_Base.BindEvent(go, action, type);
	}

	public static bool IsValid(this GameObject go)
	{
		return go != null && go.activeSelf;
	}

	public static void Blink(this GameObject go, float toAlpha, float duration, int count, Color startColor, Action after = null)
    {
		DOTween.Kill(go);

		SpriteRenderer[] srr = go.GetComponentsInChildren<SpriteRenderer>();

		bool aftterCall = false;

		foreach (var s in srr)
			s.color = startColor;

        foreach (var s in srr)
        {
			SpriteRenderer temp = s;
			s.DOFade(toAlpha, duration).SetLoops(count, LoopType.Yoyo).SetEase(Ease.InSine)
				.onComplete += () =>
                {
					temp.color = startColor;

					if (aftterCall)
                        return;
                    aftterCall = true;
					

					after?.Invoke();
                };
        }
    }

}
