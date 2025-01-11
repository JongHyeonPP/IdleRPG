using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 객체 풀 관리 클래스
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private GameObject _prefab;
    private Transform _parent;
    private Queue<GameObject> _pool;

    public ObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        _prefab = prefab;
        _parent = parent;
        _pool = new Queue<GameObject>();

        // 객체 풀 초기화
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _parent);
            obj.SetActive(false); // 처음에는 비활성화
            _pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 풀에서 객체를 가져옴
    /// </summary>
    public GameObject GetObject()
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);  // 풀에서 꺼내면 활성화
            return obj;
        }
        else
        {
            // 풀에 더 이상 객체가 없으면 새로운 객체를 생성하고 반환
            GameObject obj = Instantiate(_prefab, _parent);
            return obj;
        }
    }

    /// <summary>
    /// 객체를 풀에 반환
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);  // 객체를 비활성화하여 풀에서 관리
        _pool.Enqueue(obj);
    }
}