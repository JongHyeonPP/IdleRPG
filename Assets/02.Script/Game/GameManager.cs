using System;
using System.Collections.Generic;
using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private GameData gameData;
    private float saveInterval = 10f;
    public static int mainStageNum;
    public static StageType stageType;
    public static Action OnDataLoadComplete;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            DataManager.OnAuthenticationComplete += () =>
            {
                LoadGameData();
                StartCoroutine(WaitAndInvokeOnDataLoadComplete());
            };
        }
        else
        {
            Destroy(this);
        }
    }

    private IEnumerator WaitAndInvokeOnDataLoadComplete()
    {
        float elapsedTime = 0f;
        float minWaitTime = 1f;

        while (gameData==null || elapsedTime < minWaitTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        OnDataLoadComplete?.Invoke();
    }


    public void AutoSaveStart()
    {
        StartCoroutine(AutoSaveCoroutine());
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(saveInterval);
            DataManager.SaveToPlayerPrefs("GameData", gameData);
            string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
            Debug.Log("Game data saved:\n" + serializedData);
        }
    }

    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", gameData);
    }

    public void LoadGameData()
    {
        gameData = DataManager.LoadFromPlayerPrefs<GameData>("GameData");

        if (gameData == null)
        {
            Debug.Log("No saved game data found. Initializing default values.");
            gameData = new GameData
            {
                gold = 0,
                dia = 0,
                skillLevel = new Dictionary<string, int>(),
                weaponLevel = new Dictionary<string, int>(),
                statLevel_0 = new Dictionary<string, int>(),
                statLevel_1 = new Dictionary<string, int>()
            };
        }

        string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }
}
