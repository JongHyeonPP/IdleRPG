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
using System.Numerics;
using UnityEditor.Overlays;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//�̱��� ����
    [SerializeField] GameData _gameData;//���� �����Ϳ� �ֱ������� ����ȭ�� ����� ������ Ŭ����

    private float _saveInterval = 10f;//���� �����Ϳ� �ڵ� �����ϴ� ����(��)
    public static PlayerController controller { get; private set; }//�����ϴ� �÷��̾�, GameManager.controller�� �̱���ó�� ����ϱ� ����
    public string userId { get; private set; }//���� ������ ���� ���� ������ ���̵�
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
        StartBroker.GetGameData += () => _gameData;
    }
    private void Start()
    {
        BattleBroker.OnStageChange += OnStageChange;
        StartBroker.SaveLocal += SaveLocalData;
    }

    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller�� ã�� ������ �ʿ��� �������� �����Ѵ�.
    private void InitPlayer()
    {
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        controller.InitDefaultStatus();
        controller.SetStatus(_gameData.statLevel_Gold);
        controller.SetStatus(_gameData.statLevel_StatPoint);
    }
    //������ �ε� �ð��� �ʹ� ª���� ���� ������ �ָ������� �ּ� �ð� �Ҵ�����.
    private IEnumerator WaitAndInvokeOnDataLoadComplete()
    {
        float elapsedTime = 0f;
        float minWaitTime = 1f;

        while (_gameData == null || elapsedTime < minWaitTime)
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
        DataManager.SaveToPlayerPrefs("GameData", _gameData);
    }


    //Ŭ���忡 ���� ���� ���� ������ ������ ��� �����ϴ� �޼���. �����ϴ� �����͵��� ������� ���� ��Ȳ�� ������ ������ �� �־�� �Ѵ�.
    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", _gameData);
    }
    //���� �����͸� �ҷ��ͼ� ���� ��Ȳ�� �����Ѵ�. ���� ��ȭ�� ���� �����Ϳ� �������� �ʴ´�.
    public void LoadGameData()
    {
        _gameData = DataManager.LoadFromPlayerPrefs<GameData>("GameData");

        if (_gameData == null)
        {
            Debug.Log("No saved game data found. Initializing default values.");
            _gameData = new()
            {
                currentStageNum = 1
            };
        }
        if (_gameData.level < 1)
        {
            _gameData.level = 1;
        }
        _gameData.skillLevel ??= new();
        _gameData.weaponCount ??= new();
        _gameData.statLevel_Gold ??= new();
        _gameData.statLevel_StatPoint ??= new();
        _gameData.weaponCount ??= new();
        _gameData.weaponLevel ??= new();
        _gameData.skillFragment ??= new();
        _gameData.equipedSkillArr ??= new string[5];
        string serializedData = JsonConvert.SerializeObject(_gameData, Formatting.Indented);
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
        int value = 10 * (_gameData.currentStageNum + 1) + Random.Range(0, 3);
        _gameData.gold += value;
        BattleBroker.OnGoldSet();
        BattleBroker.OnCurrencyInBattle?.Invoke(DropType.Gold, value);
    }
    public void GetExpByDrop()
    {
        int value = 10 * (_gameData.currentStageNum + 1);
        //mainStageNum
        _gameData.exp += value;
        if (_gameData.exp >= GetNeedExp())
        {
            _gameData.exp = 0;
            _gameData.level++;
        }
        BattleBroker.OnLevelExpSet();
        BattleBroker.OnCurrencyInBattle(DropType.Exp, value);
    }
    public float GetExpPercent()
    {
        BigInteger needExp = GetNeedExp();
        BigInteger exp = _gameData.exp;

        if (needExp == 0)
            return 0f; // 0���� ������ ���� ����

        return (float)((double)exp / (double)needExp);
    }

    private BigInteger GetNeedExp()
    {
        return _gameData.level * 100;
    }

    private void OnStageChange(int stageNum)
    {
        _gameData.currentStageNum = stageNum;
        SaveLocalData();
    }
    [ContextMenu("ClearGameData")]
    public void ClearGameData()
    {
        _gameData = null;
        SaveLocalData();
    }
}