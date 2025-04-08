using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageTextPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // ������ �ؽ�Ʈ ������
    private int poolSize = 3; // �ʱ� Ǯ ũ��
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

    // �ʱ�ȭ
    public void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            DamageText damageText = CreateDamageText();
            pool.Enqueue(damageText);
        }
    }

    // ���ο� ������ �ؽ�Ʈ ����
    private DamageText CreateDamageText()
    {
        GameObject obj = Instantiate(damageTextPrefab, _textParent);
        DamageText damageText = obj.GetComponent<DamageText>();
        damageText.index = currentIndex++;
        damageText.returnAction = (damageText)=> ReturnToPool(damageText);
        damageText.SetActive(false);
        return obj.GetComponent<DamageText>();
    }

    // Ǯ���� ��������
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

    // Ǯ�� ��ȯ
    public void ReturnToPool(DamageText damageText)
    {
        damageText.SetActive(false);
        //damageText.transform.SetParent(transform);
        pool.Enqueue(damageText);
    }

    // Ǯ ����
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
