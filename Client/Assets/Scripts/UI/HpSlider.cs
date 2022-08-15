using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpSlider : MonoBehaviour
{
    const float CHANGE_DURATION = 1f;

    [SerializeField] private Image _sliderBg;
    [SerializeField] private Image _sliderFg;

    [SerializeField] private TextMeshProUGUI _valueText;

    private int _curHp = 0;
    private int _maxHp = 0;

    private Coroutine _cChange;

    public void Init(int curHp, int maxHp)
    {
        _curHp = curHp;
        _maxHp = maxHp;

        _sliderBg.fillAmount = (float)_curHp / _maxHp;
        _sliderFg.fillAmount = (float)_curHp / _maxHp;

        _valueText.text = $"{_curHp}/{_maxHp}";

        Debug.Log("설마 여기?");
    }

    public void ResetValue()
    {
        _sliderBg.fillAmount = 1;
        _sliderFg.fillAmount = 1;
        _valueText.text = String.Empty;
    }

    public void Change(int to)
    {
        _sliderFg.fillAmount = (float)to / _maxHp;
        _valueText.text = $"{to}/{_maxHp}";

        if (_cChange != null)
        {
            StopCoroutine(_cChange);
            _cChange = null;
        }

        _cChange = StartCoroutine(CChange(_curHp, to));
        _curHp = to;
    }
    
    IEnumerator CChange(int from, int to)
    {
        float elapsed = 0f;
        while(elapsed < CHANGE_DURATION)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / CHANGE_DURATION;

            float dest = ((from - (from - to) * ratio)) / (float)_maxHp;
            _sliderBg.fillAmount = dest;

            yield return new WaitForEndOfFrame();
        }

        _cChange = null;

        Debug.Log("CChange End");
        yield return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
