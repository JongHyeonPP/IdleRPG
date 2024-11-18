using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//��Ʈ��ũ(Firebase:Realtime)�� ������ ����Ѵ�.
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static event Action OnDetectDuplicateLogin;//�ߺ� �α��� Ž������ �� �߻���ų ��������Ʈ
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
        GameManager.OnAuthenticationComplete += OnAutenticationComplete;
    }
    //���� ������ �ƴٸ� �ݹ�Ǿ� userId�� ������� Firebase�� �����Ѵ�.
    private void OnAutenticationComplete()
    {
        _userId = GameManager.userId;
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
                OnDetectDuplicateLogin?.Invoke();
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
}
