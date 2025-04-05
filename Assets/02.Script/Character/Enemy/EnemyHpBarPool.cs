using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHpBarPool : MonoBehaviour
{
    [SerializeField] private GameObject enemyHpBarPrefab; // ������ (EnemyHpBar ������Ʈ�� ������ �־�� ��)
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<EnemyHpBar> pool = new Queue<EnemyHpBar>();

    private void Awake()
    {
        // �ʱ� Ǯ ����
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewHpBar();
        }
    }

    private EnemyHpBar CreateNewHpBar()
    {
        GameObject obj = Instantiate(enemyHpBarPrefab, transform);
        EnemyHpBar hpBar = obj.GetComponent<EnemyHpBar>();
        hpBar.SetHpRatio(1f);
        hpBar.pool = this;
        hpBar.SetDisplay(false);
        pool.Enqueue(hpBar);
        return hpBar;
    }

    public EnemyHpBar GetFromPool()
    {
        EnemyHpBar hpBar;

        if (pool.Count > 0)
        {
            hpBar = pool.Dequeue();
            hpBar.SetHpRatio(1f);
        }
        else
        {
            hpBar = CreateNewHpBar();
        }

        return hpBar;
    }

    public void ReturnToPool(EnemyHpBar hpBar)
    {
        hpBar.SetDisplay(false);
        pool.Enqueue(hpBar);
    }
}
