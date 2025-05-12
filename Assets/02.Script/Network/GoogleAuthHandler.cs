using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.RemoteConfig;
using UnityEngine;
using static RemoteConfigManager;

public class GoogleAuthHandler : MonoBehaviour
{
    private void Awake()
    {
        StartBroker.OnAuthenticationComplete += OnAuthenticationComplete;
        StartBroker.LoadGoogleAuth += LoadGoogleAuth;
    }
    private void OnAuthenticationComplete()
    {
        string userId = GameManager.instance.userId;
        FirebaseManager.instance.Initialize(userId);
    }



    public void LoadGoogleAuth() => PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    public async void ProcessAuthentication(SignInStatus status)
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName("develop");
        await UnityServices.InitializeAsync(options);
        if (status == SignInStatus.Success)
        {
            // Google Play Games 인증 성공 시
            PlayGamesPlatform.Activate();
            StartBroker.SetUserId(PlayGamesPlatform.Instance.GetUserId());
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
                StartBroker.OnAuthenticationComplete?.Invoke();
            });
        }
        else
        {
            // Google 인증 실패 시 익명 로그인
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            StartBroker.SetUserId(AuthenticationService.Instance.PlayerId);
            StartBroker.OnAuthenticationComplete?.Invoke();
        }
        
    }


    private void OnDestroy()
    {
        StartBroker.LoadGoogleAuth -= LoadGoogleAuth;
        StartBroker.OnAuthenticationComplete -= OnAuthenticationComplete;
    }
}
