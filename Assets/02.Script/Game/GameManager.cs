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
    public static GameManager instance;//싱글톤 변수
    [SerializeField] GameData _gameData;//로컬 데이터에 주기적으로 동기화할 값들로 구성된 클래스

    private float _saveInterval = 10f;//로컬 데이터에 자동 저장하는 간격(초)
    public static PlayerController controller { get; private set; }//전투하는 플레이어, GameManager.controller를 싱글톤처럼 사용하기 위함
    public string userId { get; private set; }//구글 인증을 통해 나온 유저의 아이디
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //구글 인증이 완료되면 데이터를 로드한다
            StartBroker.OnAuthenticationComplete += () =>
            {
                LoadGameData();
                StartCoroutine(WaitAndInvokeOnDataLoadComplete());
            };
            //Battle에 최초 진입하면 플레이어의 스탯 등의 정보값을 초기화한다.
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

    //ProcessAuthentication 과정은 비동기적으로 실행된다.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller를 찾고 전투에 필요한 정보들을 세팅한다.
    private void InitPlayer()
    {
        controller = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        controller.InitDefaultStatus();
        controller.SetStatus(_gameData.statLevel_Gold);
        controller.SetStatus(_gameData.statLevel_StatPoint);
    }
    //데이터 로드 시간이 너무 짧으면 과정 연출이 애매해져서 최소 시간 할당했음.
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
    //_saveInterval 간격으로 저장하는 코루틴을 시작한다.
    public void AutoSaveStart()
    {
        StartCoroutine(AutoSaveCoroutine());
    }
    //GameData를 _saveInterval 간격마다 로컬 데이터로 저장한다.
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


    //클라우드에 현재 진행 중인 게임의 정보를 모두 저장하는 메서드. 저장하는 데이터들을 기반으로 진행 상황을 온전히 복원할 수 있어야 한다.
    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", _gameData);
    }
    //로컬 데이터를 불러와서 진행 상황을 적용한다. 유료 재화는 로컬 데이터에 저장하지 않는다.
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

    //구글 인증을 진행한다.
    public async void ProcessAuthentication(SignInStatus status)
    {
        await UnityServices.InitializeAsync();
        if (status == SignInStatus.Success)
        {
            // Google Play Games 인증 성공 시
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
            // Google 인증 실패 시 익명 로그인
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
            return 0f; // 0으로 나누는 오류 방지

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