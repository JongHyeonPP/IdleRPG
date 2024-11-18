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
    public static GameManager instance;//�̱��� ����
    private GameData gameData;//���� �����Ϳ� �ֱ������� ����ȭ�� ����� ������ Ŭ����
    private int _dia;//���� ��ȭ - �̱�
    private int _emerald;//���� ��ȭ - ��ȭ
    private float _saveInterval = 10f;//���� �����Ϳ� �ڵ� �����ϴ� ����(��)
    public static int mainStageNum;//���� ���� ���������� ������
    public static Action OnDataLoadComplete;//�ʿ��� �������� �ε尡 ������ Battle�� �Ѿ�� ���� ��������Ʈ
    public static event Action OnAuthenticationComplete;//���� ������ ������ ���� ������ �������� �����͸� �ε��ϱ� ���� ��������Ʈ
    public static PlayerContoller controller { get; private set; }//�����ϴ� �÷��̾�, GameManager.controller�� �̱���ó�� ����ϱ� ����
    public static string userId { get; private set; }//���� ������ ���� ���� ������ ���̵�
    public static string userName { get; private set; }//�ΰ��ӿ��� ���� ������ �÷��̾��� �̸�

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            //���� ������ �Ϸ�Ǹ� �����͸� �ε��Ѵ�
            OnAuthenticationComplete += () =>
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
    //ProcessAuthentication ������ �񵿱������� ����ȴ�.
    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //Controller�� ã�� ������ �ʿ��� �������� �����Ѵ�.
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
        OnDataLoadComplete?.Invoke();
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
            string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
            Debug.Log("Game data saved:\n" + serializedData);
        }
    }
    //Ŭ���忡 ���� ���� ���� ������ ������ ��� �����ϴ� �޼���. �����ϴ� �����͵��� ������� ���� ��Ȳ�� ������ ������ �� �־�� �Ѵ�.
    public async void SaveGameDataToCloud()
    {
        await DataManager.SaveToCloudAsync("GameData", gameData);
        await DataManager.SaveToCloudAsync("Currency", new Dictionary <string,object>() { {"Dia",_dia },{"Emerald", _emerald } });
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
                statLevel_0 = new Dictionary<StatusType, int>(),
                statLevel_1 = new Dictionary<StatusType, int>()
            };
        }

        string serializedData = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        Debug.Log("Game data loaded:\n" + serializedData);
    }
    //amount��ŭ ���� ����� ���� �ø��ų� ������ �޼���
    public void ChangeGold(int amount, bool increase=true)
    {
        gameData.gold = increase ? gameData.gold + amount : Mathf.Max(0, gameData.gold - amount);
        Debug.Log($"Gold {(increase ? "increased" : "decreased")} by {amount}. New total: {gameData.gold}");
    }
    //amount��ŭ ���� ���̾��� ���� �ø��ų� ������ �޼���
    public void ChangeDia(int amount, bool increase=true)
    {
    }
    //amount��ŭ ���� ���޶����� ���� �ø��ų� ������ �޼���
    public void ChangeEmerald(int amount, bool increase=true)
    {
    }
    //�ش� ��ų�� ������ 1 �ø��� �޼���
    public void SkillLevelUp(string skillName)
    {
        if (!gameData.skillLevel.ContainsKey(skillName))
        {
            gameData.skillLevel[skillName] = 0;
        }

        gameData.skillLevel[skillName]++;
        Debug.Log($"Skill {skillName} : {gameData.skillLevel[skillName]}");
    }
    //�ش� ������ ������ Amount��ŭ �ø���.
    public void ChangeWeaponLevel(string weaponName, int amount)
    {
        if (!gameData.weaponNum.ContainsKey(weaponName))
        {
            gameData.weaponNum[weaponName] = 0;
        }

        gameData.weaponNum[weaponName]+=amount;
        Debug.Log($"Weapon {weaponName} : {gameData.weaponNum[weaponName]}");
    }
    //�ش� ��ų�� ������ 1 �ø��� �޼���, statIndex 0�� ���� ��ȭ�� ����, statIndex 1�� ���������� ��ȭ�� ����
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
    //���� ������ �����Ѵ�.
    public async void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            //���� ������ �����ߴٸ� ���� User ID�� �����ͼ� userId ������ Set
            userId = PlayGamesPlatform.Instance.GetUserId();
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(userId);

        }
        else
        {
            //���� ������ �������� ��� - ������ �����Ϳ����� ����̽��� ���� ���� ID�� �Ҵ�����
            //���庻���� ���� ������ �������� ��� ������ �ٲ� �� ����.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            userId = AuthenticationService.Instance.PlayerId;
        }
        //���� userID�� ������� �ʿ��� ������ Set�Ѵ�.
        userName = PlayerPrefs.GetString("Name");
        await UnityServices.InitializeAsync();
        Debug.Log($"UserName: {userName}, UserID: {userId}");
        //������ ������ ��� �ߵ��ؾ� �ϴ� �޼���� �ϰ� ȣ��
        OnAuthenticationComplete?.Invoke();
    }
}
