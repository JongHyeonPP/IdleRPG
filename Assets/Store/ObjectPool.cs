using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��ü Ǯ ���� Ŭ����
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

        // ��ü Ǯ �ʱ�ȭ
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _parent);
            obj.SetActive(false); // ó������ ��Ȱ��ȭ
            _pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Ǯ���� ��ü�� ������
    /// </summary>
    public GameObject GetObject()
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);  // Ǯ���� ������ Ȱ��ȭ
            return obj;
        }
        else
        {
            // Ǯ�� �� �̻� ��ü�� ������ ���ο� ��ü�� �����ϰ� ��ȯ
            GameObject obj = Instantiate(_prefab, _parent);
            return obj;
        }
    }

    /// <summary>
    /// ��ü�� Ǯ�� ��ȯ
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);  // ��ü�� ��Ȱ��ȭ�Ͽ� Ǯ���� ����
        _pool.Enqueue(obj);
    }
}