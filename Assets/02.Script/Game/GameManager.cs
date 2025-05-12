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

    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
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
        string serializedData = JsonConvert.SerializeObject(_gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }

    //���� ������ �����Ѵ�.
    public async void ProcessAuthentication(SignInStatus status)
    {
        var options = new InitializationOptions().SetOption("environment-name", "develop");
        await UnityServices.InitializeAsync(options);
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




    [ContextMenu("ClearGameData")]
    public void ClearGameData()
    {
        _gameData = null;
        SaveLocalData();
    }
    // Context Menu�� �̿��Ͽ� companionPromote�� ������ �� �߰�
    [ContextMenu("Fill Companion Promote")]
    public void FillCompanionPromote()
    {
        if (_gameData == null)
        {
            Debug.LogError("_gameData�� �������� �ʾҽ��ϴ�.");
            return;
        }

        _gameData.companionPromoteEffect[0].Clear();
        _gameData.companionPromoteEffect[1].Clear();
        _gameData.companionPromoteEffect[2].Clear();

        // 1�� ����
        _gameData.companionPromoteEffect[0][0] = (StatusType.MaxHp, Rarity.Common);
        _gameData.companionPromoteEffect[0][1] = (StatusType.CriticalDamage, Rarity.Rare);

        // 2�� ����
        _gameData.companionPromoteEffect[1][0] = (StatusType.HpRecover, Rarity.Legendary);
        _gameData.companionPromoteEffect[1][1] = (StatusType.ExpAscend, Rarity.Unique);

        // 3�� ����
        _gameData.companionPromoteEffect[2][0] = (StatusType.Penetration, Rarity.Mythic);
        _gameData.companionPromoteEffect[2][1] = (StatusType.GoldAscend, Rarity.Uncommon);

        SaveLocalData();
        Debug.Log("Companion Promote �����Ͱ� ä�������ϴ�!");
    }

    // Context Menu�� �̿��Ͽ� companionPromote �� ���
    [ContextMenu("Print Companion Promote")]
    public void PrintCompanionPromote()
    {
        if (_gameData == null)
        {
            Debug.LogError("GameData�� �������� �ʾҽ��ϴ�.");
            return;
        }

        for (int i = 0; i < _gameData.companionPromoteEffect.Length; i++)
        {
            Debug.Log($"�� ���� {i + 1}:");
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