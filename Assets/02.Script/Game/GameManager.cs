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
    public static GameManager instance;//싱글톤 변수
    private GameData gameData;//로컬 데이터에 주기적으로 동기화할 값들로 구성된 클래스
    private int _dia;//유료 재화 - 뽑기
    private int _emerald;//유료 재화 - 강화
    private float _saveInterval = 10f;//로컬 데이터에 자동 저장하는 간격(초)
    public static int mainStageNum;//메인 전투 스테이지가 몇인지
    public static Action OnDataLoadComplete;//필요한 데이터의 로드가 끝나면 Battle로 넘어가기 위한 델리게이트
    public static event Action OnAuthenticationComplete;//구글 인증이 끝나면 인증 정보를 바탕으로 데이터를 로드하기 위한 델리게이트
    public static PlayerContoller controller { get; private set; }//전투하는 플레이어, GameManager.controller를 싱글톤처럼 사용하기 위함
    public static string userId { get; private set; }//구글 인증을 통해 나온 유저의 아이디
    public static string userName { get; private set; }//인게임에서 변경 가능한 플레이어의 이름

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //구글 인증이 완료되면 데이터를 로드한다
            OnAuthenticationComplete += () =>
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
    }
    //ProcessAuthentication 과정은 비동기적으로 실행된다.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller를 찾고 전투에 필요한 정보들을 세팅한다.
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
    //데이터 로드 시간이 너무 짧으면 과정 연출이 애매해져서 최소 시간 할당했음.
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
            DataManager.SaveToPlayerPrefs("GameData", gameData);
            string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
            Debug.Log("Game data saved:\n" + serializedData);
        }
    }
    //클라우드에 현재 진행 중인 게임의 정보를 모두 저장하는 메서드. 저장하는 데이터들을 기반으로 진행 상황을 온전히 복원할 수 있어야 한다.
    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", gameData);
        await DataManager.SaveToCloudAsync("Currency", new Dictionary <string,object>() { {"Dia",_dia },{"Emerald", _emerald } });
    }
    //로컬 데이터를 불러와서 진행 상황을 적용한다. 유료 재화는 로컬 데이터에 저장하지 않는다.
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
                statLevel_0 = new Dictionary<StatusType, int>(),
                statLevel_1 = new Dictionary<StatusType, int>()
            };
        }

        string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }
    //amount만큼 현재 골드의 값을 올리거나 내리는 메서드
    public void ChangeGold(int amount, bool increase=true)
    {
        gameData.gold = increase ? gameData.gold + amount : Mathf.Max(0, gameData.gold - amount);
        Debug.Log($"Gold {(increase ? "increased" : "decreased")} by {amount}. New total: {gameData.gold}");
    }
    //amount만큼 현재 다이아의 값을 올리거나 내리는 메서드
    public void ChangeDia(int amount, bool increase=true)
    {
    }
    //amount만큼 현재 에메랄드의 값을 올리거나 내리는 메서드
    public void ChangeEmerald(int amount, bool increase=true)
    {
    }
    //해당 스킬의 레벨을 1 올리는 메서드
    public void SkillLevelUp(string skillName)
    {
        if (!gameData.skillLevel.ContainsKey(skillName))
        {
            gameData.skillLevel[skillName] = 0;
        }

        gameData.skillLevel[skillName]++;
        Debug.Log($"Skill {skillName} : {gameData.skillLevel[skillName]}");
    }
    //해당 무기의 개수를 Amount만큼 올린다.
    public void ChangeWeaponLevel(string weaponName, int amount)
    {
        if (!gameData.weaponNum.ContainsKey(weaponName))
        {
            gameData.weaponNum[weaponName] = 0;
        }

        gameData.weaponNum[weaponName]+=amount;
        Debug.Log($"Weapon {weaponName} : {gameData.weaponNum[weaponName]}");
    }
    //해당 스킬의 레벨을 1 올리는 메서드, statIndex 0은 골드로 강화한 스탯, statIndex 1은 레벨업으로 강화한 스탯
    public void ChangeStatLevel(StatusType statusType, int statIndex)
    {
        var statDictionary = statIndex == 0 ? gameData.statLevel_0 : gameData.statLevel_1;

        if (!statDictionary.ContainsKey(statusType))
        {
            statDictionary[statusType] = 0;
        }

        statDictionary[statusType]++;
        Debug.Log($"Stat {statusType}, Index {statIndex}){statDictionary[statusType]}");
    }
    //구글 인증을 진행한다.
    public async void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            //만약 인증이 성공했다면 구글 User ID를 가져와서 userId 변수에 Set
            userId = PlayGamesPlatform.Instance.GetUserId();
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(userId);

        }
        else
        {
            //구글 인증에 실패했을 경우 - 지금은 에디터용으로 디바이스에 대한 임의 ID를 할당했음
            //빌드본에서 구글 인증이 실패했을 경우 내용이 바뀔 수 있음.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            userId = AuthenticationService.Instance.PlayerId;
        }
        //얻은 userID를 기반으로 필요한 값들을 Set한다.
        userName = PlayerPrefs.GetString("Name");
        await UnityServices.InitializeAsync();
        Debug.Log($"UserName: {userName}, UserID: {userId}");
        //인증이 끝났을 경우 발동해야 하는 메서드들 일괄 호출
        OnAuthenticationComplete?.Invoke();
    }
}
