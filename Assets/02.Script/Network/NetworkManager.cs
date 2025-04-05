using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//��Ʈ��ũ(Firebase:Realtime)�� ������ ����Ѵ�.
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    private DatabaseReference _fbRef;//firebase ��Ʈ ���۷�Ʈ
    private string _userId;//���� ���̵�
    private string _deviceId;//��� ���̵�
    private bool _isNetworkAvailable = true;//��Ʈ��ũ�� ����� �ִ°�
    private Coroutine _networkCheckCoroutine;//��Ʈ��ũ�� ������ִ��� ���������� �˻��ϴ� �ڷ�ƾ
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        _deviceId = SystemInfo.deviceUniqueIdentifier;
        _fbRef = FirebaseDatabase.DefaultInstance.RootReference;
        StartBroker.OnAuthenticationComplete += OnAutenticationComplete;
        StartBroker.OnMoveBattleScene += ListenForDeviceChange;
    }
    //���� ������ �ƴٸ� �ݹ�Ǿ� userId�� ������� Firebase�� �����Ѵ�.
    private void OnAutenticationComplete()
    {
        _userId = GameManager.instance.userId;
        ListenForDeviceChange();
        UpdateLoginStatus();
        _networkCheckCoroutine ??= StartCoroutine(CheckNetworkStatusCoroutine());
    }
    //������ ���� ���̵��� ��Ⱚ�� �������ؼ� �ش� ���� ����Ǹ� �Ͼ ���� ����
    private void ListenForDeviceChange()
    {
        _fbRef.Child("Users").Child(_userId).Child("deviceId").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Error!!");
                return;
            }

            if (args.Snapshot.Exists && args.Snapshot.Value.ToString() != SystemInfo.deviceUniqueIdentifier)
            {
                StartBroker.OnDetectDuplicateLogin?.Invoke();
            }
        };
    }
    //�α��� ����(1. ���� ������ 2. ��� ID, 3. ���� �α��� �ߴ���)�� ����
    private async void UpdateLoginStatus()
    {
        DatabaseReference userRef = _fbRef.Child("Users").Child(_userId);

        // ���� Ÿ�ӽ����� ��û
        var timestampRef = _fbRef.Database.GetReference(".info/serverTimeOffset");
        await timestampRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error retrieving server time.");
                return;
            }

            if (task.IsCompleted)
            {
                // ����� ���� �� �ִ� �������� ���� �ð��� ����
                var updates = new Dictionary<string, object>
                {
                    { "isLoggedIn", true },
                    { "deviceId",  _deviceId},
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
    //��Ʈ��ũ�� ������ �� �Ͼ �ϵ��� ����
    private IEnumerator CheckNetworkStatusCoroutine()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable && _isNetworkAvailable)
            {
                Debug.Log("Network connection lost!");
                _isNetworkAvailable = false;
            }
            else if (Application.internetReachability != NetworkReachability.NotReachable && !_isNetworkAvailable)
            {
                Debug.Log("Network connection restored!");
                _isNetworkAvailable = true;
            }

            yield return new WaitForSeconds(1f);
        }
    }
    [ContextMenu("SetTest")]
    public void SetTest() => SetUserData("TestKey", "TestValue");
    public void SetUserData(string key, object value)
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Debug.LogWarning("User ID is null. Make sure authentication is complete.");
            return;
        }

        DatabaseReference userRef = _fbRef.Child("Users").Child(_userId);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { key, value }
        };

        userRef.UpdateChildrenAsync(data).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to update {key}: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"Successfully updated {key} to {value}");
            }
        });
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        _fbRef.Child("Users").Child(_userId).Child("isLoggedIn").SetValueAsync(false);
    }
#endif
}
