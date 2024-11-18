using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//네트워크(Firebase:Realtime)의 역할을 담당한다.
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static event Action OnDetectDuplicateLogin;//중복 로그인 탐지했을 때 발생시킬 델리게이트
    private DatabaseReference _fbRef;//firebase 루트 레퍼런트
    private string _userId;//구글 아이디
    private string _deviceId;//기기 아이디
    private bool _isNetworkAvailable = true;//네트워크가 연결돼 있는가
    private Coroutine _networkCheckCoroutine;//네트워크가 연결돼있는지 지속적으로 검사하는 코루틴
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
    //구글 인증이 됐다면 콜백되어 userId를 기반으로 Firebase에 연동한다.
    private void OnAutenticationComplete()
    {
        _userId = GameManager.userId;
        ListenForDeviceChange();
        UpdateLoginStatus();
        _networkCheckCoroutine ??= StartCoroutine(CheckNetworkStatusCoroutine());
    }
    //본인의 구글 아이디의 기기값을 리스닝해서 해당 값이 변경되면 일어날 일을 정의
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
    //로그인 정보(1. 게임 중인지 2. 기기 ID, 3. 언제 로그인 했는지)를 갱신
    private async void UpdateLoginStatus()
    {
        DatabaseReference userRef = _fbRef.Child("Users").Child(_userId);

        // 서버 타임스탬프 요청
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
                // 사람이 읽을 수 있는 형식으로 서버 시간을 저장
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
    //네트워크가 끊겼을 때 일어날 일들을 정의
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
