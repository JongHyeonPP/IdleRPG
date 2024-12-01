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

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//�̱��� ����
    public GameData gameData { get; private set; }//���� �����Ϳ� �ֱ������� ����ȭ�� ����� ������ Ŭ����
    public int dia;//���� ��ȭ - �̱�
    public int emerald;//���� ��ȭ - ��ȭ
    private float _saveInterval = 10f;//���� �����Ϳ� �ڵ� �����ϴ� ����(��)
    public static PlayerContoller controller { get; private set; }//�����ϴ� �÷��̾�, GameManager.controller�� �̱���ó�� ����ϱ� ����
    public static string userId { get; private set; }//���� ������ ���� ���� ������ ���̵�
    public static string userName { get; private set; }//�ΰ��ӿ��� ���� ������ �÷��̾��� �̸�
    private int _mainStageNum;//���� ���� ���������� ������
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
        BattleBroker.OnGoldGain += GetGoldByDrop;
        BattleBroker.OnExpGain += GetExpByDrop;
        BattleBroker.OnMainStageChange += ChangeMainStage;
    }

    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller�� ã�� ������ �ʿ��� �������� �����Ѵ�.
    private void InitPlayer()
    {
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerContoller>();
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
            DataManager.SaveToPlayerPrefs("GameData", gameData);
        }
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
            gameData = new GameData
            {
                gold = 0,
                skillLevel = new Dictionary<string, int>(),
                weaponNum = new Dictionary<string, int>(),
                statLevel_Gold = new Dictionary<StatusType, int>(),
                statLevel_StatPoint = new Dictionary<StatusType, int>()

            };
        }
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
    private void GetGoldByDrop()
    {
        //mainStageNum
        gameData.gold += 10 * (_mainStageNum + 1);
    }
    private void GetExpByDrop()
    {
        //mainStageNum
        gameData.exp += 10 * (_mainStageNum + 1);
        if (gameData.exp >= GetNeedExp())
        {
            gameData.exp = 0;
            gameData.level++;
            BattleBroker.OnLevelUp();
        }
    }
    public float GetExpPercent()
    {
        return Math.Clamp(gameData.exp / GetNeedExp(), 0f, 1f);
    }

    private float GetNeedExp()
    {
        return gameData.level * 100f;
    }
    //MainStageNum�� �����ϰ� �ű⿡ �´� ����� ����� �����Ѵ�.
    private void ChangeMainStage(int stageNum)
    {
        _mainStageNum = stageNum;
        gameData.currentStageNum = stageNum;
    }
}