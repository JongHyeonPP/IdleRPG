using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool isShuttingDown = false;
    private static readonly object lockObject = new object();
    private static T innerInstance;

    public static T Instance
    {
        get
        {
            if (isShuttingDown)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (lockObject)
            {
                if (innerInstance != null)
                {
                    return innerInstance;
                }

                // ������ �����ϴ� �ν��Ͻ� �˻�
                innerInstance = FindObjectOfType<T>();
                if (innerInstance != null)
                {
                    DontDestroyOnLoad(innerInstance.gameObject);
                    return innerInstance;
                }

                // ���ο� �ν��Ͻ� ����
                GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                innerInstance = singletonObject.AddComponent<T>();
                DontDestroyOnLoad(singletonObject);
                return innerInstance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (isShuttingDown)
        {
            Destroy(gameObject);
            return;
        }

        lock (lockObject)
        {
            if (innerInstance == null)
            {
                innerInstance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (innerInstance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of '{typeof(T).Name}' found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    private void OnDestroy()
    {
        lock (lockObject)
        {
            if (innerInstance == this)
            {
                innerInstance = null;
                isShuttingDown = true;
            }
        }
    }

    /// <summary>
    /// �̱��� �ν��Ͻ��� �����ϴ��� Ȯ��
    /// </summary>
    public static bool HasInstance => innerInstance != null && !isShuttingDown;

    /// <summary>
    /// �����ϰ� �̱��� �ν��Ͻ��� �ı�
    /// </summary>
    public static void DestroyInstance()
    {
        lock (lockObject)
        {
            if (innerInstance != null)
            {
                isShuttingDown = true;
                if (Application.isPlaying)
                {
                    Destroy(innerInstance.gameObject);
                }
                innerInstance = null;
            }
        }
    }
}