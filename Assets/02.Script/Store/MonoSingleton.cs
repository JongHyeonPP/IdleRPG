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

                // 기존에 존재하는 인스턴스 검색
                innerInstance = FindObjectOfType<T>();
                if (innerInstance != null)
                {
                    DontDestroyOnLoad(innerInstance.gameObject);
                    return innerInstance;
                }

                // 새로운 인스턴스 생성
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
    /// 싱글톤 인스턴스가 존재하는지 확인
    /// </summary>
    public static bool HasInstance => innerInstance != null && !isShuttingDown;

    /// <summary>
    /// 안전하게 싱글톤 인스턴스를 파괴
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