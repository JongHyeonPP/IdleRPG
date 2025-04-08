using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudCode;
using Unity.Services.Authentication; // 로그인용
using Newtonsoft.Json;
using System.Collections.Generic;

public class CloudCodeTest : MonoBehaviour
{
    [ContextMenu("Call Cloud Code")]
    public async void CallCloudCode()
    {
        //await UnityServices.InitializeAsync();

        //if (!AuthenticationService.Instance.IsSignedIn)
        //{
        //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //    Debug.Log("익명 로그인 완료");
        //}

        Dictionary<string, object> args = new Dictionary<string, object>
        {
            { "num", 10 }
        };

        try
        {
            string jsonResult = await CloudCodeService.Instance.CallEndpointAsync("Test", args);
            Debug.Log("JSON 응답: " + jsonResult);

            Dictionary<string, object> resultDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResult);

            if (resultDict.ContainsKey("num"))
            {
                int num = System.Convert.ToInt32(resultDict["num"]);
                Debug.Log("파싱된 num 값: " + num);
            }
        }
        catch (CloudCodeException e)
        {
            Debug.LogError("Cloud Code 호출 실패: " + e.Message);
        }
    }
}
