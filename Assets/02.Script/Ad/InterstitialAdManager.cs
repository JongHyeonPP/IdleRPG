using EnumCollection;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // AdsInitializer 쪽 필드
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode;
    private string _gameId;

    // InterstitialAdManager 쪽 필드
    [SerializeField] string _androidAdUnitId;
    [SerializeField] string _iOsAdUnitId;
    private string _adUnitId;

    (Resource, int)? currentAdReward;
    void Awake()
    {

        
        // AdsInitializer 내용
#if UNITY_IOS
        _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
        _gameId = _androidGameId;
#endif
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
            Debug.Log("Ad Initialize Success");
        }
        else
        {
            Debug.LogError("Ad Initialize Fail");
        }

        // InterstitialAdManager 내용
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;

        NetworkBroker.LoadAd += LoadAd;
        DontDestroyOnLoad(gameObject);
    }

    // AdsInitializer 인터페이스
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    // InterstitialAdManager 원본 메서드들
    public void LoadAd((Resource, int) reward)
    {
        Debug.Log("Loading Ad: " + _adUnitId);
        currentAdReward = reward;
        Advertisement.Load(_adUnitId, this);
    }
    //광고 띄우는 메서드
    public void ShowAd()
    {
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        ShowAd();
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string _adUnitId)
    {
        Debug.Log("OnUnityAdsShowStart");
    }

    public void OnUnityAdsShowClick(string _adUnitId)
    {
        Debug.Log("OnUnityAdsShowClick");
    }
    //광고 끝나면 뜨는 메서드
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("OnUnityAdsShowComplete");
        NetworkBroker.QueueResourceReport(currentAdReward.Value.Item2, null, currentAdReward.Value.Item1, Source.Advertise);
        GameData gameData = StartBroker.GetGameData();
        switch (currentAdReward.Value.Item1)
        {
            case Resource.Dia:
                gameData.dia += currentAdReward.Value.Item2;
                PlayerBroker.OnDiaSet();
                break;
            case Resource.Clover:
                gameData.clover += currentAdReward.Value.Item2;
                PlayerBroker.OnCloverSet();
                break;
        }
        currentAdReward = null;
        NetworkBroker.SaveServerData();
    }
}
