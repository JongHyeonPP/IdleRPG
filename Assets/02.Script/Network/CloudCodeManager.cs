using UnityEngine;
using Unity.Services.CloudCode;
using Newtonsoft.Json;
using System.Collections.Generic;
using Unity.Services.Core.Environments;
using UnityEditor;
using Unity.Services.Economy;
using Unity.Services.Core;
using Unity.Services.Authentication;
public class CloudCodeManager : MonoBehaviour
{
    public static CloudCodeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
#if UNITY_EDITOR
    
    [ContextMenu("Test Log")]
    public async void TestLog()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync("GoldVerification", "LogTest");
    }
    [ContextMenu("TestAssignQuest")]
    public async void TestAssignQuest()
    {
        string result = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(
            "QuestExample", // ��� �̸�
            "AssignQuest",          // CloudCodeFunction �̸�
            new Dictionary<string, object>() // �Ķ���� ����
        );

        Debug.Log($"���� ����: {result}");
    }
    [ContextMenu("Temp")]
    public async void Temp()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync(
            "AutoLoginRecorder", // ��� �̸�
            "AutoRecordLogin",   // CloudCodeFunction �̸�
            new Dictionary<string, object>() // �Ķ���� ����
        );
    }
    [ContextMenu("PerformAction")]
    public async void PerformAction()
    {
        string result = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(
            "QuestExample", // ��� �̸�
            "PerformAction",          // CloudCodeFunction �̸�
            new Dictionary<string, object>() // �Ķ���� ����
        );

        Debug.Log($"���� ����: {result}");
    }
    [ContextMenu("TestJs")]
    public async void TestJs()
    {
        // ������ ������ �Ķ���� �غ�
        Dictionary<string, object> args = new Dictionary<string, object>
        {
            { "name", "JongHyeon" }
        };

        try
        {
            // Cloud Code SayHello ��������Ʈ ȣ��
            string jsonResult = await CloudCodeService.Instance.CallEndpointAsync("Test", args);
            Debug.Log("���� ���� ���: " + jsonResult);
        }
        catch (CloudCodeException e)
        {
            Debug.LogError("Cloud Code ȣ�� ����: " + e.Message);
        }
    }
    [ContextMenu("SetGold")]
    public async void SetGold()
    {
        await EconomyService.Instance.PlayerBalances.SetBalanceAsync("GOLD", 100);
    }
    [ContextMenu("ForceLogOutIn")]
    public async void ForceLogOutIn()
    {
        AuthenticationService.Instance.SignOut();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
#endif
}
