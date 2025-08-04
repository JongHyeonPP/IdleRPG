using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

        _gameData.statPoint = _gameData.level - 1 - _gameData.statLevel_StatPoint.Values.Sum();

        string serializedData = JsonConvert.SerializeObject(_gameData);
        Debug.Log("Game data loaded:\n" + serializedData);
        return true;
    }
}