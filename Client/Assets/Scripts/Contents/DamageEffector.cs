using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DamageEffector : MonoBehaviour
{
    [SerializeField] private GameObject _normalPrefab;
    [SerializeField] private GameObject _ciriticalPrefab;
    private BoxCollider2D _collider;

    private Coroutine _cPlayLoopDamageEffect = null;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        //PlayLoopDamageEffect();       
    }

    private void PlayLoopDamageEffect()
    {
        _cPlayLoopDamageEffect = StartCoroutine(CPlayLoopDamageEffect());
    }

    IEnumerator CPlayLoopDamageEffect()
    {
        while(true)
        {
          

            int ranDamage = UnityEngine.Random.Range(20, 100);

            float ranCritical = UnityEngine.Random.Range(0f, 1f);
            if(ranCritical < 0.2f)
            {
                GenerateCriticalText(ranDamage, Color.red, 0.1f);
            }
            else 
                GenerateText(ranDamage, Color.red, 1f, 1f);

            float ranInterval = UnityEngine.Random.Range(0.1f, 0.3f);
            yield return new WaitForSeconds(ranInterval);
        }
    }

    public void GenerateText(int value, Color color, float moveDistance, float duration)
    {
        Vector3 ranPos = _collider.GetRanndomPosInArea();

        GameObject box = GameObject.Instantiate(_normalPrefab, transform);
        box.transform.localPosition = ranPos;

        TextMeshProUGUI textBox = box.GetComponentInChildren<TextMeshProUGUI>();
        textBox.text = value.ToString();
        textBox.color = color;

        float dest = ranPos.y + moveDistance;
        box.transform.DOLocalMoveY(dest, duration)
            .SetEase(Ease.InSine)
            .onComplete += () =>
            {
                GameObject.Destroy(box);
            };

        textBox.DOFade(0, duration);
    }

    public void GenerateCriticalText(int value, Color color, float duration)
    {
        Vector3 ranPos = new Vector3(0, 0.5f, 0);
        GameObject box = GameObject.Instantiate(_ciriticalPrefab, transform);
        box.transform.localPosition = ranPos;

        TextMeshProUGUI textBox = box.GetComponentInChildren<TextMeshProUGUI>();
        textBox.text = value.ToString();
        textBox.color = color;

        Vector3 dest = box.transform.localScale * 1.2f;
        box.transform.DOScale(dest, duration)
            .SetEase(Ease.OutSine)
            .onComplete += () =>
            {
                GameObject.Destroy(box, 0.2f);
            };
    }
}
