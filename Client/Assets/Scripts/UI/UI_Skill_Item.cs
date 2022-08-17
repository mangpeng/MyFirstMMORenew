using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill_Item : MonoBehaviour
{
    public bool IsCool = false;

    [SerializeField] Image _cover;
    [SerializeField] TextMeshProUGUI _countText;

    Image _skillIcon;
    Skill _skillData;

    Coroutine _cPlayCoolDown;

    private void Awake()
    {
        _skillIcon = GetComponent<Image>();
    }

    public void Init()
    {
        _cover.fillAmount = 0;
        
    }

    public void InitData(Skill skillData)
    {
        if (skillData == null)
            return;

        if (_skillIcon == null)
            _skillIcon = GetComponent<Image>();

        _skillData = skillData;
        if(!string.IsNullOrEmpty(_skillData.skillIcon))
            _skillIcon.sprite = Managers.Resource.Load<Sprite>(_skillData.skillIcon);
    }

    public void ResetSkill()
    {
        if(_cPlayCoolDown != null)
        {
            Debug.Log("쿨다운 상태인 스킬을 초기화 합니다.");
            StopCoroutine(_cPlayCoolDown);
            _cPlayCoolDown = null;
        }    

        IsCool = false;
        SetCover(0);
    }

    public void SetCool(float duration)
    {
        IsCool = true;
        PlaySkill(duration);
    }

    private void SetCover(float amount)
    {
        if (_cover == null)
            return;

        _cover.fillAmount = amount;
    }

    private void PlaySkill(float duration)
    {
        if (_cover == null)
            return;

        SetCover(1);
        _cPlayCoolDown = StartCoroutine(CPlayCoolDown(duration));
    }



    private IEnumerator CPlayCoolDown(float duration)
    {
        float elapsed = 0;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;

            _cover.fillAmount = 1 - (elapsed / duration);

            yield return new WaitForEndOfFrame();
        }

        SetCover(0);
        IsCool = false;
        _cPlayCoolDown = null;
    }

    public void SetText(string text)
    {
        if (_countText == null)
            return;

        _countText.text = text;
    }

    public string GetText()
    {
        if (_countText == null)
            return string.Empty;

        return _countText.text;
    }
}
