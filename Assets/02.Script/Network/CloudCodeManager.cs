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
            "QuestExample", // 모듈 이름
            "AssignQuest",          // CloudCodeFunction 이름
            new Dictionary<string, object>() // 파라미터 없음
        );

        Debug.Log($"서버 응답: {result}");
    }
    [ContextMenu("Temp")]
    public async void Temp()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync(
            "AutoLoginRecorder", // 모듈 이름
            "AutoRecordLogin",   // CloudCodeFunction 이름
            new Dictionary<string, object>() // 파라미터 없음
        );
    }
    [ContextMenu("PerformAction")]
    public async void PerformAction()
    {
        string result = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(
            "QuestExample", // 모듈 이름
            "PerformAction",          // CloudCodeFunction 이름
            new Dictionary<string, object>() // 파라미터 없음
        );

        Debug.Log($"서버 응답: {result}");
    }
    [ContextMenu("TestJs")]
    public async void TestJs()
    {
        // 서버에 전달할 파라미터 준비
        Dictionary<string, object> args = new Dictionary<string, object>
        {
            { "name", "JongHyeon" }
        };

        try
        {
            // Cloud Code SayHello 엔드포인트 호출
            string jsonResult = await CloudCodeService.Instance.CallEndpointAsync("Test", args);
            Debug.Log("서버 응답 결과: " + jsonResult);
        }
        catch (CloudCodeException e)
        {
            Debug.LogError("Cloud Code 호출 실패: " + e.Message);
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
