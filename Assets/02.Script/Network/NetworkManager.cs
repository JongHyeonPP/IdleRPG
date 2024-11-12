using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static event Action OnDetectDuplicateLogin;
    private DatabaseReference fbRef;
    private string userId;
    private string deviceId;
    private bool isNetworkAvailable = true;
    private Coroutine networkCheckCoroutine;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        deviceId = SystemInfo.deviceUniqueIdentifier;
        fbRef = FirebaseDatabase.DefaultInstance.RootReference;
        GameManager.OnAuthenticationComplete += OnAutenticationComplete;
    }

    private void OnAutenticationComplete()
    {
        userId = GameManager.userId;
        ListenForDeviceChange();
        UpdateLoginStatus();
        networkCheckCoroutine ??= StartCoroutine(CheckNetworkStatusCoroutine());
    }

    private void ListenForDeviceChange()
    {
        fbRef.Child("Users").Child(userId).Child("deviceId").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Error!!");
                return;
            }

            if (args.Snapshot.Exists && args.Snapshot.Value.ToString() != SystemInfo.deviceUniqueIdentifier)
            {
                OnDetectDuplicateLogin?.Invoke();
            }
        };
    }

    private async void UpdateLoginStatus()
    {
        DatabaseReference userRef = fbRef.Child("Users").Child(userId);

        // 서버 타임스탬프 요청
        var timestampRef = fbRef.Database.GetReference(".info/serverTimeOffset");
        await timestampRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error retrieving server time.");
                return;
            }

            if (task.IsCompleted)
            {
                // 사람이 읽을 수 있는 형식으로 서버 시간을 저장
                var updates = new Dictionary<string, object>
                {
                    { "isLoggedIn", true },
                    { "deviceId",  deviceId},
                    { "lastLoginTime", ServerValue.Timestamp }
                };
                userRef.UpdateChildrenAsync(updates);
            }
        });

        var disconnect = new Dictionary<string, object>
        {
            { "isLoggedIn", false },
            { "deviceId", null }
        };
        await userRef.Child("isLoggedIn").OnDisconnect().SetValue(disconnect);
        
    }

    private IEnumerator CheckNetworkStatusCoroutine()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable && isNetworkAvailable)
            {
                Debug.Log("Network connection lost!");
                isNetworkAvailable = false;
            }
            else if (Application.internetReachability != NetworkReachability.NotReachable && !isNetworkAvailable)
            {
                Debug.Log("Network connection restored!");
                isNetworkAvailable = true;
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
