using UnityEngine;
using EnumCollection;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//�̱��� ����
    [SerializeField] GameData _gameData;//���� �����Ϳ� �ֱ������� ����ȭ�� ����� ������ Ŭ����

    private float _saveInterval = 10f;//���� �����Ϳ� �ڵ� �����ϴ� ����(��)
    public string userId { get; private set; }//���� ������ ���� ���� ������ ���̵�
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //���� ������ �Ϸ�Ǹ� �����͸� �ε��Ѵ�
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

    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    
    //Controller�� ã�� ������ �ʿ��� �������� �����Ѵ�.

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

    
    public async Task<bool> LoadGameData()
    {
        _gameData = await DataManager.LoadFromCloudAsync<GameData>("GameData");
        if (_gameData == null)
        {
            Debug.Log("No saved game data found. Initializing default values.");
            _gameData = new()
            {
                currentStageNum = 1,
                //maxStageNum = 1,//���丮 �����Ǹ� ���־���.
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