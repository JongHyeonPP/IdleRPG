using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            StartBroker.OnAuthenticationComplete += async () =>
            {
                bool isValid = await LoadGameData();
                if (isValid)
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
        StartBroker.SetUserId += (userId) => this.userId = userId;
    }

    //ProcessAuthentication 과정은 비동기적으로 실행된다.
    
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

    
    public async Task<bool> LoadGameData()
    {
        _gameData = await DataManager.LoadFromCloudAsync<GameData>("GameData");
        if (_gameData == null)
        {
            Debug.Log("No saved game data found. Initializing default values.");
            _gameData = new()
            {
                currentStageNum = 1,
                //maxStageNum = 1,//스토리 복구되면 없애야함.
                level = 1
            };
        }
        if (3 <=_gameData.invalidCount)
        {
            StartBroker.OnDetectInvalidAct();
            return false;
        }
        if (_gameData.level < 1)
        {
            
        }
        string serializedData = JsonConvert.SerializeObject(_gameData);
        Debug.Log("Game data loaded:\n" + serializedData);
        return true;
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

        NetworkBroker.SaveServerData();
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

        NetworkBroker.SaveServerData();
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