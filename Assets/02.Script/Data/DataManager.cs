using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using Newtonsoft.Json;
public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            UnityServices.InitializeAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 클라우드 저장
    public static async Task SaveToCloudAsync<T>(Dictionary<string, T> dataDict)
    {
        try
        {
            // 모든 데이터를 JSON 형식으로 변환하여 저장할 수 있도록 구성
            var serializedDataDict = new Dictionary<string, object>();

            foreach (var entry in dataDict)
            {
                serializedDataDict[entry.Key] = JsonConvert.SerializeObject(entry.Value, Formatting.Indented);
            }

            // 클라우드 저장 호출
            await CloudSaveService.Instance.Data.Player.SaveAsync(serializedDataDict);
            Debug.Log("All data saved to cloud successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data to cloud: {e.Message}");
        }
    }
    public static async Task SaveToCloudAsync(string key, object data)
    {
        await SaveToCloudAsync(new Dictionary<string, object>() { { key, data } });
    }

    //클라우드의 데이터를 일괄 로드하는 메서드. 한 번에 로드하는 것이 비용적으로 저렴함.
    public static async Task<Dictionary<string, T>> LoadFromCloudAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>(keys));
            var result = new Dictionary<string, T>();

            foreach (var key in keys)
            {
                if (data.ContainsKey(key))
                {
                    string jsonData = data[key].ToString();
                    T loadedData = JsonConvert.DeserializeObject<T>(jsonData);
                    result[key] = loadedData;
                    Debug.Log($"Data loaded from cloud successfully: {key}");
                }
                else
                {
                    Debug.LogWarning($"No data found in cloud for key: {key}");
                    result[key] = default;
                }
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data from cloud: {e.Message}");
            return null;
        }
    }
    //클라우드의 데이터를 하나만 로드하는 메서드
    public static async Task<T> LoadFromCloudAsync<T>(string key)
    {
        var result = await LoadFromCloudAsync<T>(new List<string> { key });

        if (result != null && result.ContainsKey(key))
        {
            return result[key];
        }
        return default;
    }


    // 로컬 저장 (PlayerPrefs)
    public static void SaveToPlayerPrefs(string key, object data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
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
    public static T LoadFromPlayerPrefs<T>(string key)
    {
        try
        {
            string jsonData = PlayerPrefs.GetString(key, string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                T loadedData = JsonConvert.DeserializeObject<T>(jsonData);
                Debug.Log($"Data loaded from PlayerPrefs successfully: {key}");
                return loadedData;
            }
            else
            {
                Debug.LogWarning($"No data found in PlayerPrefs for key: {key}");
                return default;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data from PlayerPrefs: {e.Message}");
            return default;
        }
    }

}
