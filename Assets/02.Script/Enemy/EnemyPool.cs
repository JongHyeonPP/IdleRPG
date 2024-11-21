using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public Queue<EnemyController> pool { get; private set; } = null;//���� �����ϴ� Ǯ
    public Transform poolParent { private get; set; }//Ǯ�� �ִ� ������Ʈ�� �θ� Ʈ������
    private EnemyStatus _enemyStatus;//���� ����ϰ� �ִ� ���� ����
    private int _poolSize;//Ǯ�� ũ��
    //���� EnemyStatus�� ������� Pool�� �Ҵ��ϰ� ����� 
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
    //�� ������Ʈ�� �����ؼ� pool���� ��ȯ�Ѵ�.
    private EnemyController InstantiateEnemy()
    {
        GameObject obj = Instantiate(_enemyStatus.prefab);
        obj.transform.SetParent(poolParent);
        EnemyController controller = obj.AddComponent<EnemyController>();
        obj.SetActive(false);
        controller.SetEnemyInfo(this, _enemyStatus);
        return controller;
    }
    //Ǯ���� ������Ʈ �ϳ� ��ȯ�Ѵ�.
    public EnemyController GetFromPool()
    {
        if (pool.Count > 0)
        {
            EnemyController enemy = pool.Dequeue();
            enemy.gameObject.SetActive(true);
            enemy.hp = _enemyStatus.MaxHp;
            return enemy;
        }
        else
        {
            return InstantiateEnemy();
        }
    }
    //Ǯ�� ������Ʈ ��ȯ�Ѵ�.
    public void ReturnToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolParent);
        pool.Enqueue(enemy);
    }
    //�������� �̵��ϴ� �����̳� ó�� Ǯ�� ����� �� Ǯ�� ����.
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
