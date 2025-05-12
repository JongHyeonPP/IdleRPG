using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//싱글톤 변수
    [SerializeField] GameData _gameData;//로컬 데이터에 주기적으로 동기화할 값들로 구성된 클래스

    private float _saveInterval = 10f;//로컬 데이터에 자동 저장하는 간격(초)
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
        }
        else
        {
            Destroy(this);
        }
        StartBroker.GetGameData += () => _gameData;
    }
    private void Start()
    {
        StartBroker.SaveLocal += SaveLocalData;
    }

    //ProcessAuthentication 과정은 비동기적으로 실행된다.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller를 찾고 전투에 필요한 정보들을 세팅한다.

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
        string serializedData = JsonConvert.SerializeObject(_gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }

    //구글 인증을 진행한다.
    public async void ProcessAuthentication(SignInStatus status)
    {
        var options = new InitializationOptions().SetOption("environment-name", "develop");
        await UnityServices.InitializeAsync(options);
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




    [ContextMenu("ClearGameData")]
    public void ClearGameData()
    {
        _gameData = null;
        SaveLocalData();
    }
    // Context Menu를 이용하여 companionPromote에 고정된 값 추가
    [ContextMenu("Fill Companion Promote")]
    public void FillCompanionPromote()
    {
        if (_gameData == null)
        {
            Debug.LogError("_gameData가 설정되지 않았습니다.");
            return;
        }

        _gameData.companionPromoteEffect[0].Clear();
        _gameData.companionPromoteEffect[1].Clear();
        _gameData.companionPromoteEffect[2].Clear();

        // 1번 동료
        _gameData.companionPromoteEffect[0][0] = (StatusType.MaxHp, Rarity.Common);
        _gameData.companionPromoteEffect[0][1] = (StatusType.CriticalDamage, Rarity.Rare);

        // 2번 동료
        _gameData.companionPromoteEffect[1][0] = (StatusType.HpRecover, Rarity.Legendary);
        _gameData.companionPromoteEffect[1][1] = (StatusType.ExpAscend, Rarity.Unique);

        // 3번 동료
        _gameData.companionPromoteEffect[2][0] = (StatusType.Penetration, Rarity.Mythic);
        _gameData.companionPromoteEffect[2][1] = (StatusType.GoldAscend, Rarity.Uncommon);

        SaveLocalData();
        Debug.Log("Companion Promote 데이터가 채워졌습니다!");
    }

    // Context Menu를 이용하여 companionPromote 값 출력
    [ContextMenu("Print Companion Promote")]
    public void PrintCompanionPromote()
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData가 설정되지 않았습니다.");
            return;
        }

        for (int i = 0; i < _gameData.companionPromoteEffect.Length; i++)
        {
            Debug.Log($"ㅇ 동료 {i + 1}:");
            foreach (var kvp in _gameData.companionPromoteEffect[i])
            {
                Debug.Log($"   - Key {kvp.Key}: {kvp.Value.Item1} ({kvp.Value.Item2})");
            }
        }
    }
    [ContextMenu("Fill Stat Promote")]
    public void FillStatPromote()
    {
        _gameData.stat_Promote.Clear();

        _gameData.stat_Promote[1] = (StatusType.MaxHp, 5);
        _gameData.stat_Promote[2] = (StatusType.Power, 10);
        _gameData.stat_Promote[3] = (StatusType.HpRecover, 7);
        _gameData.stat_Promote[4] = (StatusType.Critical, 3);
        _gameData.stat_Promote[5] = (StatusType.CriticalDamage, 8);
        _gameData.stat_Promote[6] = (StatusType.Resist, 4);
        _gameData.stat_Promote[7] = (StatusType.Penetration, 6);
        _gameData.stat_Promote[8] = (StatusType.GoldAscend, 2);
        _gameData.stat_Promote[9] = (StatusType.ExpAscend, 1);
        _gameData.stat_Promote[10] = (StatusType.MaxMp, 0);
        _gameData.stat_Promote[11] = (StatusType.MpRecover, 0);

        SaveLocalData();
    }
    [ContextMenu("Print Stat Promote")]
    public void PrintStatPromote()
    {
        foreach (var kvp in _gameData.stat_Promote)
        {
            Debug.Log($"ID: {kvp.Key}, Type: {kvp.Value.Item1}, Value: {kvp.Value.Item2}");
        }
    }
    [ContextMenu("Temp")]
    public void Temp()
    {
        _gameData.currentStageNum = _gameData.maxStageNum = 2;
    }
}