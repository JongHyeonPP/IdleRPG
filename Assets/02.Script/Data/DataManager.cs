using Firebase.Database;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public static string userName { get; private set; }
    public static string userId { get; private set; }
    public static DataManager instance;
    public static bool isLogin { get; private set; } = false;
    private string deviceId;

    // 인증 완료 시 호출될 공용 이벤트
    public static event Action OnAuthenticationComplete;
    public static event Action OnDetectDuplicateLogin;
    private DatabaseReference fbRef;

    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            deviceId = SystemInfo.deviceUniqueIdentifier;
            fbRef = FirebaseDatabase.DefaultInstance.RootReference;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

    public async void ProcessAuthentication(SignInStatus status)
    {
        isLogin = true;
        if (status == SignInStatus.Success)
        {
            userName = PlayGamesPlatform.Instance.GetUserDisplayName();
            userId = PlayGamesPlatform.Instance.GetUserId();
        }
        else
        {
            userName = "Unknown Name";
            userId = "Unknown ID";
        }

        await UpdateLoginStatus();
        ListenForDeviceChange();
        OnAuthenticationComplete?.Invoke();
    }

    private async Task UpdateLoginStatus()
    {
        DatabaseReference userRef = fbRef.Child("Users").Child(userId);
        var updates = new Dictionary<string, object>
        {
            { "isLoggedIn", true },
            { "deviceId", deviceId },
            { "lastLoginTime", ServerValue.Timestamp }
        };
        await userRef.SetValueAsync(updates);

        var disconnect = new Dictionary<string, object>
        {
            { "isLoggedIn", false },
            { "deviceId", null }
        };
        await userRef.Child("isLoggedIn").OnDisconnect().SetValue(disconnect);
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

            if (args.Snapshot.Exists && args.Snapshot.Value.ToString() != deviceId)
            {
                OnDetectDuplicateLogin?.Invoke();
            }
        };
    }
    // 클라우드 저장
    public static async Task SaveToCloudAsync<T>(string key, T data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            var dataDict = new Dictionary<string, object> { { key, jsonData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataDict);
            Debug.Log($"Data saved to cloud successfully: {key}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data to cloud: {e.Message}");
        }
    }

    // 클라우드 로드
    public static async Task<T> LoadFromCloudAsync<T>(string key) where T : class
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (data.ContainsKey(key))
            {
                string jsonData = data[key].ToString();
                T loadedData = JsonConvert.DeserializeObject<T>(jsonData);
                Debug.Log($"Data loaded from cloud successfully: {key}");
                return loadedData;
            }
            else
            {
                Debug.LogWarning($"No data found in cloud for key: {key}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data from cloud: {e.Message}");
            return null;
        }
    }

    // 로컬 저장 (PlayerPrefs)
    public static void SaveToPlayerPrefs<T>(string key, T data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, jsonData);
            PlayerPrefs.Save();
            Debug.Log($"Data saved to PlayerPrefs successfully: {key}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data to PlayerPrefs: {e.Message}");
        }
    }

    // 로컬 로드 (PlayerPrefs)
    public static T LoadFromPlayerPrefs<T>(string key) where T : class
    {
        try
        {
            string jsonData = PlayerPrefs.GetString(key, string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                T loadedData = JsonUtility.FromJson<T>(jsonData);
                Debug.Log($"Data loaded from PlayerPrefs successfully: {key}");
                return loadedData;
            }
            else
            {
                Debug.LogWarning($"No data found in PlayerPrefs for key: {key}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data from PlayerPrefs: {e.Message}");
            return null;
        }
    }

}
