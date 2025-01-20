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
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//�̱��� ����
    public GameData gameData { get; private set; }//���� �����Ϳ� �ֱ������� ����ȭ�� ����� ������ Ŭ����
    public int dia;//���� ��ȭ - �̱�
    public int emerald;//���� ��ȭ - ��ȭ
    private float _saveInterval = 10f;//���� �����Ϳ� �ڵ� �����ϴ� ����(��)
    public static PlayerController controller { get; private set; }//�����ϴ� �÷��̾�, GameManager.controller�� �̱���ó�� ����ϱ� ����
    public string userId { get; private set; }//���� ������ ���� ���� ������ ���̵�
    public string userName { get; private set; }//�ΰ��ӿ��� ���� ������ �÷��̾��� �̸�
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //���� ������ �Ϸ�Ǹ� �����͸� �ε��Ѵ�
            StartBroker.OnAuthenticationComplete += () =>
            {
                LoadGameData();
                StartCoroutine(WaitAndInvokeOnDataLoadComplete());
            };
            //Battle�� ���� �����ϸ� �÷��̾��� ���� ���� �������� �ʱ�ȭ�Ѵ�.
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
    private void Start()
    {
        StartBroker.GetGameData += () => gameData;
        BattleBroker.OnStageChange += OnStageChange;
    }

    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller�� ã�� ������ �ʿ��� �������� �����Ѵ�.
    private void InitPlayer()
    {
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        controller.InitDefaultStatus();
        controller.SetStatus(gameData.statLevel_Gold);
        controller.SetStatus(gameData.statLevel_StatPoint);
    }
    //������ �ε� �ð��� �ʹ� ª���� ���� ������ �ָ������� �ּ� �ð� �Ҵ�����.
    private IEnumerator WaitAndInvokeOnDataLoadComplete()
    {
        float elapsedTime = 0f;
        float minWaitTime = 1f;

        while (gameData==null || elapsedTime < minWaitTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        StartBroker.OnDataLoadComplete?.Invoke();
    }
    //_saveInterval �������� �����ϴ� �ڷ�ƾ�� �����Ѵ�.
    public void AutoSaveStart()
    {
        StartCoroutine(AutoSaveCoroutine());
    }
    //GameData�� _saveInterval ���ݸ��� ���� �����ͷ� �����Ѵ�.
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_saveInterval);
            SaveLocalData();
        }
    }

    public void SaveLocalData()
    {
        DataManager.SaveToPlayerPrefs("GameData", gameData);
    }

    //Ŭ���忡 ���� ���� ���� ������ ������ ��� �����ϴ� �޼���. �����ϴ� �����͵��� ������� ���� ��Ȳ�� ������ ������ �� �־�� �Ѵ�.
    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", gameData);
        await DataManager.SaveToCloudAsync("Currency", new Dictionary <string,object>() { {"Dia",dia },{"Emerald", emerald } });
    }
    //���� �����͸� �ҷ��ͼ� ���� ��Ȳ�� �����Ѵ�. ���� ��ȭ�� ���� �����Ϳ� �������� �ʴ´�.
    public void LoadGameData()
    {
        gameData = DataManager.LoadFromPlayerPrefs<GameData>("GameData");

        if (gameData == null)
        {
            Debug.Log("No saved game data found. Initializing default values.");
            gameData = new();
        }
        if (gameData.level < 1)
        {
            gameData.level = 1;
        }
        gameData.skillLevel ??= new();
        gameData.weaponNum ??= new();
        gameData.statLevel_Gold ??= new();
        gameData.statLevel_StatPoint ??= new();
        gameData.equipedSkillArr ??= new string[10];
        userName = PlayerPrefs.GetString("Name");
        string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }

    //���� ������ �����Ѵ�.
    public async void ProcessAuthentication(SignInStatus status)
    {
        await UnityServices.InitializeAsync();
        if (status == SignInStatus.Success)
        {
            // Google Play Games ���� ���� ��
            PlayGamesPlatform.Activate();
            userId = PlayGamesPlatform.Instance.GetUserId();
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
            {
                Debug.Log("Authorization code: " + code);
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
                StartBroker.OnAuthenticationComplete?.Invoke();
            });
        }
        else
        {
            // Google ���� ���� �� �͸� �α���
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            userId = AuthenticationService.Instance.PlayerId;
            StartBroker.OnAuthenticationComplete?.Invoke();
        }       
    }
    public void GetGoldByDrop()
    {
        int value = 10 * (gameData.currentStageNum + 1) + Random.Range(0,3);
        gameData.gold += value;
        BattleBroker.OnGoldSet();
        BattleBroker.OnCurrencyInBattle?.Invoke(DropType.Gold, value);
    }
    public void GetExpByDrop()
    {
        int value = 10 * (gameData.currentStageNum + 1);
        //mainStageNum
        gameData.exp += value;
        if (gameData.exp >= GetNeedExp())
        {
            gameData.exp = 0;
            gameData.level++;
        }
        BattleBroker.OnLevelExpSet();
        BattleBroker.OnCurrencyInBattle(DropType.Exp, value);
    }
    public float GetExpPercent()
    {
        return Math.Clamp((float)(gameData.exp / GetNeedExp()), 0, 1);
    }

    private int GetNeedExp()
    {
        return gameData.level * 100;
    }
    
    private void OnStageChange(int stageNum)
    {
        gameData.currentStageNum = stageNum;
        SaveLocalData();
    }
    public void GoToNextStage()
    {
        gameData.currentStageNum++;
        gameData.maxStageNum = Mathf.Max(gameData.currentStageNum, gameData.maxStageNum);
        SaveLocalData();
    }
}