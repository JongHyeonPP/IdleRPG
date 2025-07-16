using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Services.Authentication;
public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Ŭ������ �����͸� �ϰ� �ε��ϴ� �޼���. �� ���� �ε��ϴ� ���� ��������� ������.
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
                    var jsonStr = data["GameData"].Value.GetAs<string>();
                    var loadedData = JsonConvert.DeserializeObject<T>(jsonStr);
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
    //Ŭ������ �����͸� �ϳ��� �ε��ϴ� �޼���
    public static async Task<T> LoadFromCloudAsync<T>(string key)
    {
        var result = await LoadFromCloudAsync<T>(new List<string> { key });

        if (result != null && result.ContainsKey(key))
        {
            return result[key];
        }
        return default;
    }
    // ���� ���� (PlayerPrefs)
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

    // ���� �ε� (PlayerPrefs)
    public static T LoadFromPlayerPrefs<T>(string key)
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
}
