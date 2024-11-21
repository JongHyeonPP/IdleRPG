using System;
using System.Collections.Generic;
using UnityEngine;

public class DropPool : MonoBehaviour
{
    // 각각의 드롭 타입별 풀
    public Queue<GoldDrop> goldPool { get; private set; } = new();
    public Queue<ExpDrop> expPool { get; private set; } = new();

    // 풀에 있는 오브젝트들의 부모 트랜스폼
    public Transform goldParent;
    public Transform expParent;

    // 풀의 크기
    private int _poolSize = 10;

    // 드롭 타입별 프리팹을 담는 딕셔너리
    private Dictionary<Type, GameObject> prefabDict = new();
    public GameObject goldPrefab;
    public GameObject expPrefab;

    private void Start()
    {
        prefabDict.Add(typeof(GoldDrop), goldPrefab);
        prefabDict.Add(typeof(ExpDrop), expPrefab);
        InitializePool(); // 초기화
    }

    // 풀 초기화
    public void InitializePool()
    {
        // 골드 풀 초기화
        for (int i = 0; i < _poolSize; i++)
        {
            goldPool.Enqueue(InstantiateDrop<GoldDrop>(goldParent));
        }

        // 경험치 풀 초기화
        for (int i = 0; i < _poolSize; i++)
        {
            expPool.Enqueue(InstantiateDrop<ExpDrop>(expParent));
        }
    }

    // 드롭 오브젝트를 생성해서 반환
    private T InstantiateDrop<T>(Transform parent) where T : DropBase
    {
        if (!prefabDict.ContainsKey(typeof(T)))
        {
            Debug.LogError($"{typeof(T).Name}에 대한 프리팹이 등록되지 않았습니다.");
            return null;
        }

        GameObject obj = Instantiate(prefabDict[typeof(T)], parent);
        obj.transform.SetParent(parent);
        T drop = obj.GetComponent<T>();

        if (drop == null)
        {
            Debug.LogError($"{typeof(T).Name}에 대한 컴포넌트를 찾을 수 없습니다.");
        }

        obj.SetActive(false);
        drop.InitDropBase(this);
        return drop;
    }

    // 풀에서 오브젝트 하나 가져오기
    public T GetFromPool<T>() where T : DropBase
    {
        Queue<T> pool = GetPool<T>();

        if (pool == null)
        {
            Debug.LogError($"{typeof(T).Name}에 대한 풀을 찾을 수 없습니다.");
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

    // 풀에 오브젝트 반환
    public void ReturnToPool<T>(T drop) where T : DropBase
    {
        Queue<T> pool = GetPool<T>();

        if (pool == null)
        {
            Debug.LogError($"{typeof(T).Name}에 대한 풀을 찾을 수 없습니다.");
            Destroy(drop.gameObject); // 풀이 없으면 파괴
            return;
        }

        drop.gameObject.SetActive(false);
        drop.transform.SetParent(GetParent<T>());
        pool.Enqueue(drop);
    }

    // 드롭 타입별 풀 반환
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

    // 드롭 타입별 부모 반환
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
