using System;
using System.Collections.Generic;
using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private GameData gameData;
    private int dia;
    private int emerald;
    private float saveInterval = 10f;
    public static int mainStageNum;
    public static Action OnDataLoadComplete;
    public static event Action OnAuthenticationComplete;
    public PlayerContoller controller;
    public static string userId { get; private set; }
    public static string userName { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            OnAuthenticationComplete += () =>
            {
                LoadGameData();
                StartCoroutine(WaitAndInvokeOnDataLoadComplete());
            };
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (!controller && scene.name == "Battle")
                {
                    InitPlayer();
                }
            };
        }
        else
        {
            Destroy(this);
        }
    }
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    private void InitPlayer()
    {
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerContoller>();
        foreach (var x in gameData.statLevel_0)
        {
        }
        foreach (var x in gameData.statLevel_1)
        {
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
        await DataManager.SaveToCloudAsync("Currency", new Dictionary <string,object>() { {"Dia",dia },{"Emerald", emerald } });
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
                skillLevel = new Dictionary<string, int>(),
                weaponLevel = new Dictionary<string, int>(),
                statLevel_0 = new Dictionary<StatusType, int>(),
                statLevel_1 = new Dictionary<StatusType, int>()
            };
        }

        string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }
    public void ChangeGold(int amount, bool increase=true)
    {
        gameData.gold = increase ? gameData.gold + amount : Mathf.Max(0, gameData.gold - amount);
        Debug.Log($"Gold {(increase ? "increased" : "decreased")} by {amount}. New total: {gameData.gold}");
    }

    public void ChangeDia(int amount, bool increase=true)
    {
    }

    public void ChangeSkillLevel(string skillName, int amount, bool increase = true)
    {
        if (!gameData.skillLevel.ContainsKey(skillName))
        {
            gameData.skillLevel[skillName] = 0;
        }

        gameData.skillLevel[skillName] = increase ? gameData.skillLevel[skillName] + amount : Mathf.Max(0, gameData.skillLevel[skillName] - amount);
        Debug.Log($"Skill level for {skillName} {(increase ? "increased" : "decreased")} by {amount}. New level: {gameData.skillLevel[skillName]}");
    }

    public void ChangeWeaponLevel(string weaponName, int amount, bool increase)
    {
        if (!gameData.weaponLevel.ContainsKey(weaponName))
        {
            gameData.weaponLevel[weaponName] = 0;
        }

        gameData.weaponLevel[weaponName] = increase ? gameData.weaponLevel[weaponName] + amount : Mathf.Max(0, gameData.weaponLevel[weaponName] - amount);
        Debug.Log($"Weapon level for {weaponName} {(increase ? "increased" : "decreased")} by {amount}. New level: {gameData.weaponLevel[weaponName]}");
    }

    public void ChangeStatLevel(StatusType statusType, int statIndex, bool increase = true)
    {
        var statDictionary = statIndex == 0 ? gameData.statLevel_0 : gameData.statLevel_1;

        if (!statDictionary.ContainsKey(statusType))
        {
            statDictionary[statusType] = 0;
        }

        statDictionary[statusType] = increase ? statDictionary[statusType] + 1 : Mathf.Max(0, statDictionary[statusType] - 1);
        Debug.Log($"Stat level for {statusType} (Index {statIndex}) {(increase ? "increased" : "decreased")} by 1. New level: {statDictionary[statusType]}");
    }
    public async void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            userId = PlayGamesPlatform.Instance.GetUserId();
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(userId);

        }
        else
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            userId = AuthenticationService.Instance.PlayerId;
        }
        userName = PlayerPrefs.GetString("Name");
        await UnityServices.InitializeAsync();
        Debug.Log($"UserName: {userName}, UserID: {userId}");
        OnAuthenticationComplete?.Invoke();
    }
}
