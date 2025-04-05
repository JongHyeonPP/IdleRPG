using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageTextPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    private int poolSize = 3; // 초기 풀 크기
    private Queue<DamageText> pool = new Queue<DamageText>();
    private int currentIndex;
    [SerializeField] Transform _textParent;
    private void Awake()
    {
        InitializePool();
        BattleBroker.ShowDamageText += ShowDamageText;
    }

    private void ShowDamageText(Vector3 screenPos, string text)
    {
        DamageText damageText = GetFromPool();
        damageText.StartShowText(screenPos, text);
    }

    // 초기화
    public void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            DamageText damageText = CreateDamageText();
            pool.Enqueue(damageText);
        }
    }

    // 새로운 데미지 텍스트 생성
    private DamageText CreateDamageText()
    {
        GameObject obj = Instantiate(damageTextPrefab, _textParent);
        DamageText damageText = obj.GetComponent<DamageText>();
        damageText.index = currentIndex++;
        damageText.returnAction = (damageText)=> ReturnToPool(damageText);
        damageText.SetActive(false);
        return obj.GetComponent<DamageText>();
    }

    // 풀에서 가져오기
    public DamageText GetFromPool()
    {
        DamageText damageText;

        if (pool.Count > 0)
        {
            damageText = pool.Dequeue();
        }
        else
        {
            damageText = CreateDamageText();
        }
        damageText.SetActive(true);
        damageText.SetOpacity(0f);
        return damageText;
    }

    // 풀로 반환
    public void ReturnToPool(DamageText damageText)
    {
        damageText.SetActive(false);
        //damageText.transform.SetParent(transform);
        pool.Enqueue(damageText);
    }

    // 풀 비우기
    //public void ClearPool()
    //{
    //    while (pool.Count > 0)
    //    {
    //        DamageText dt = pool.Dequeue();
    //        Destroy(dt.gameObject);
    //    }
    //    pool.Clear();
    //}
}
