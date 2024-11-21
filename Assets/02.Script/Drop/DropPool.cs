using System;
using System.Collections.Generic;
using UnityEngine;

public class DropPool : MonoBehaviour
{
    // ������ ��� Ÿ�Ժ� Ǯ
    public Queue<GoldDrop> goldPool { get; private set; } = new();
    public Queue<ExpDrop> expPool { get; private set; } = new();

    // Ǯ�� �ִ� ������Ʈ���� �θ� Ʈ������
    public Transform goldParent;
    public Transform expParent;

    // Ǯ�� ũ��
    private int _poolSize = 10;

    // ��� Ÿ�Ժ� �������� ��� ��ųʸ�
    private Dictionary<Type, GameObject> prefabDict = new();
    public GameObject goldPrefab;
    public GameObject expPrefab;

    private void Start()
    {
        prefabDict.Add(typeof(GoldDrop), goldPrefab);
        prefabDict.Add(typeof(ExpDrop), expPrefab);
        InitializePool(); // �ʱ�ȭ
    }

    // Ǯ �ʱ�ȭ
    public void InitializePool()
    {
        // ��� Ǯ �ʱ�ȭ
        for (int i = 0; i < _poolSize; i++)
        {
            goldPool.Enqueue(InstantiateDrop<GoldDrop>(goldParent));
        }

        // ����ġ Ǯ �ʱ�ȭ
        for (int i = 0; i < _poolSize; i++)
        {
            expPool.Enqueue(InstantiateDrop<ExpDrop>(expParent));
        }
    }

    // ��� ������Ʈ�� �����ؼ� ��ȯ
    private T InstantiateDrop<T>(Transform parent) where T : DropBase
    {
        if (!prefabDict.ContainsKey(typeof(T)))
        {
            Debug.LogError($"{typeof(T).Name}�� ���� �������� ��ϵ��� �ʾҽ��ϴ�.");
            return null;
        }

        GameObject obj = Instantiate(prefabDict[typeof(T)], parent);
        obj.transform.SetParent(parent);
        T drop = obj.GetComponent<T>();

        if (drop == null)
        {
            Debug.LogError($"{typeof(T).Name}�� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        obj.SetActive(false);
        drop.InitDropBase(this);
        return drop;
    }

    // Ǯ���� ������Ʈ �ϳ� ��������
    public T GetFromPool<T>() where T : DropBase
    {
        Queue<T> pool = GetPool<T>();

        if (pool == null)
        {
            Debug.LogError($"{typeof(T).Name}�� ���� Ǯ�� ã�� �� �����ϴ�.");
            return null;
        }

        if (pool.Count > 0)
        {
            T drop = pool.Dequeue();
            drop.gameObject.SetActive(true);
            return drop;
        }
        else
        {
            return InstantiateDrop<T>(GetParent<T>());
        }
    }

    // Ǯ�� ������Ʈ ��ȯ
    public void ReturnToPool<T>(T drop) where T : DropBase
    {
        Queue<T> pool = GetPool<T>();

        if (pool == null)
        {
            Debug.LogError($"{typeof(T).Name}�� ���� Ǯ�� ã�� �� �����ϴ�.");
            Destroy(drop.gameObject); // Ǯ�� ������ �ı�
            return;
        }

        drop.gameObject.SetActive(false);
        drop.transform.SetParent(GetParent<T>());
        pool.Enqueue(drop);
    }

    // ��� Ÿ�Ժ� Ǯ ��ȯ
    private Queue<T> GetPool<T>() where T : Component
    {
        if (typeof(T) == typeof(GoldDrop))
        {
            return goldPool as Queue<T>;
        }
        else if (typeof(T) == typeof(ExpDrop))
        {
            return expPool as Queue<T>;
        }
        return null;
    }

    // ��� Ÿ�Ժ� �θ� ��ȯ
    private Transform GetParent<T>() where T : Component
    {
        if (typeof(T) == typeof(GoldDrop))
        {
            return goldParent;
        }
        else if (typeof(T) == typeof(ExpDrop))
        {
            return expParent;
        }
        return null;
    }
}
