using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public Queue<EnemyController> pool { get; private set; } = null;
    private EnemyStatus enemyStatus;
    protected int poolSize;

    public void InitializePool(EnemyStatus _poolObject, int _poolSize = 10)
    {
        pool = new();
        enemyStatus = _poolObject;
        poolSize = _poolSize;
        for (int i = 0; i < poolSize; i++)
        {
            pool.Enqueue(InstantiateEnemy());
        }
    }

    private EnemyController InstantiateEnemy()
    {
        GameObject obj = Instantiate(enemyStatus.prefab);
        EnemyController controller = obj.AddComponent<EnemyController>();
        controller.status = enemyStatus;
        obj.SetActive(false);
        controller.pool = this;
        return controller;
    }

    public EnemyController GetFromPool()
    {
        if (pool.Count > 0)
        {
            EnemyController enemy = pool.Dequeue();
            enemy.gameObject.SetActive(true);
            return enemy;
        }
        else
        {
            return InstantiateEnemy();
        }
    }

    public void ReturnToPool(EnemyController obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
    public void ClearPool()
    {
        if (pool == null)
            return;
        enemyStatus = null;
        while (pool.Count > 0)
        {
            EnemyController enemy = pool.Dequeue();
            Destroy(enemy);
        }
        pool = null;
    }

}
