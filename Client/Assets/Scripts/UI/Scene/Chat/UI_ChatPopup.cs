using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatPopup : UI_Popup
{
    const float showDuration = 3f;
    const float fadeDuration = 1f;

    [SerializeField]
    private TextMeshProUGUI _messageTxt;

    [SerializeField]
    private Image _bg;

    private Coroutine _cFadeOut = null;

    private float _initTextAlpha = 0;
    private float _initBgAlpha = 0;

    private void Awake()
    {
        _initTextAlpha = _messageTxt.alpha;
        _initBgAlpha = _bg.color.a;

        SetFade(0);
    }

    public void Show(string message)
    {
        _messageTxt.text = message;

        if(_cFadeOut != null)
            StopCoroutine(_cFadeOut);
        _cFadeOut = StartCoroutine(CFadeOut(showDuration, fadeDuration));
    }

    private IEnumerator CFadeOut(float duration, float after)
    {
        SetFade(1f);
        yield return new WaitForSeconds(duration);
        
        while(after > 0)
        {
            after -= Time.deltaTime;
            float ratio = after / fadeDuration;
            SetFade(ratio);

            yield return new WaitForEndOfFrame();
        }
    }

    private void SetFade(float ratio)
    {
        Color temp = _bg.color;
        temp.a = _initBgAlpha * ratio;
        _bg.color = temp;

        temp = _messageTxt.color;
        temp.a = _initTextAlpha * ratio;
        _messageTxt.color = temp;
    }
}
