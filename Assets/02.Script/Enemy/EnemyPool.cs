using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public Queue<EnemyController> pool { get; private set; } = null;//적을 보관하는 풀
    private EnemyStatus _enemyStatus;//현재 사용하고 있는 적의 정보
    private int _poolSize;//풀의 크기
    //받은 EnemyStatus를 기반으로 Pool을 할당하고 사용할 
    public void InitializePool(EnemyStatus enemyStatus, int poolSize = 10)
    {
        pool = new();
        _enemyStatus = enemyStatus;
        _poolSize = poolSize;
        for (int i = 0; i < _poolSize; i++)
        {
            pool.Enqueue(InstantiateEnemy());
        }
    }
    //적 오브젝트를 생성해서 pool에게 반환한다.
    private EnemyController InstantiateEnemy()
    {
        GameObject obj = Instantiate(_enemyStatus.prefab);
        EnemyController controller = obj.AddComponent<EnemyController>();
        obj.SetActive(false);
        controller.SetEnemyInfo(this, _enemyStatus);
        return controller;
    }
    //풀에서 오브젝트 하나 반환한다.
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
    //풀에 오브젝트 반환한다.
    public void ReturnToPool(EnemyController obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
    //스테이지 이동하는 시점이나 처음 풀을 사용할 때 풀을 비운다.
    public void ClearPool()
    {
        if (pool == null)
            return;
        _enemyStatus = null;
        while (pool.Count > 0)
        {
            EnemyController enemy = pool.Dequeue();
            Destroy(enemy);
        }
        pool = null;
    }
}
