using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageEffector : MonoBehaviour
{
    public GameObject normalPrefab;
    public GameObject ciriticalPrefab;
    public BoxCollider2D collider;

    private Coroutine _cPlayLoopDamageEffect = null;

    void Start()
    {
        PlayLoopDamageEffect();       
    }

    private void PlayLoopDamageEffect()
    {
        _cPlayLoopDamageEffect = StartCoroutine(CPlayLoopDamageEffect());
    }

    IEnumerator CPlayLoopDamageEffect()
    {
        while(true)
        {
            Vector3 ranPos = GetRanPos();

            int ranDamage = UnityEngine.Random.Range(20, 100);

            float ranCritical = UnityEngine.Random.Range(0f, 1f);
            if(ranCritical < 0.2f)
            {
                GenerateCriticalText(ranDamage, Color.red, new Vector3(0, 0.5f, 0), 0.1f);
            }
            else 
                GenerateText(ranDamage, Color.red, ranPos, 1f, 1f);

            float ranInterval = UnityEngine.Random.Range(0.1f, 0.3f);
            yield return new WaitForSeconds(ranInterval);
        }
    }

    private void GenerateText(int value, Color color, Vector3 ranPos, float moveDistance, float duration)
    {
        GameObject box = GameObject.Instantiate(normalPrefab, transform);
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

    private void GenerateCriticalText(int value, Color color, Vector3 ranPos, float duration)
    {
        GameObject box = GameObject.Instantiate(ciriticalPrefab, transform);
        box.transform.localPosition = ranPos;

        TextMeshProUGUI textBox = box.GetComponentInChildren<TextMeshProUGUI>();
        textBox.text = value.ToString();
        textBox.color = color;

        Vector3 dest = transform.localScale * 0.02f;
        box.transform.DOScale(dest, duration)
            .SetEase(Ease.OutSine)
            .onComplete += () =>
            {
                GameObject.Destroy(box, 0.3f);
            };
    }

    private Vector3 GetRanPos()
    {
        float x = UnityEngine.Random.Range(-collider.size.x / 2, collider.size.x / 2);
        float y = UnityEngine.Random.Range(-collider.size.y / 2, collider.size.y / 2);

        Debug.Log($"{x}, {y}");
        return new Vector3(x, y, 0);
    }
}
