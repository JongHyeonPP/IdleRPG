using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool isShuttingDown = false;
    private static object lockObject = new object();
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
                GameObject singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                innerInstance = singletonObject.AddComponent<T>();

                DontDestroyOnLoad(singletonObject);

                return innerInstance;
            }
        }
    }

    protected virtual void Awake()
    {
        lock (lockObject)
        {
            if (innerInstance == null)
            {
                innerInstance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (innerInstance != this)
            {
                Destroy(gameObject); // 중복된 인스턴스가 생성되면 파괴
            }
        }
    }

    private void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
    }
}
