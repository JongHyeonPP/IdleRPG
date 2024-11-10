using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    private AsyncOperation asyncLoad;
    //[SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text tapToStartText;
    [SerializeField] private TMP_Text underText;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject tapToStartImage;
    Coroutine blinkCoroutine;
    private void Awake()
    {
        DataManager.OnAuthenticationComplete += UnderTextLoadGameData;
        GameManager.OnDataLoadComplete += PrepareMainScene;
        //progressBar.fillAmount = 0f;
        blinkCoroutine = StartCoroutine(BlinkTapToStart());
        loadingText.gameObject.SetActive(false);
        underText.gameObject.SetActive(false);
    }
    private void Start()
    {
        
    }
    private void PrepareMainScene()
    {
        loadingText.gameObject.SetActive(true);
        underText.gameObject.SetActive(false);
        StartCoroutine(LoadMainScene());
    }
    private void UnderTextLoadGameData()
    {
        underText.text = "정보를 불러오는 중";
    }
    public void GoogleAuthLoad()
    {
        underText.gameObject.SetActive(true);
        underText.text = "구글 로그인 중";
        DataManager.instance.LoadGoogleAuth();
    }
    private IEnumerator LoadMainScene()
    {
        // 메인 씬을 비동기적으로 로드하되, 활성화는 보류합니다.
        asyncLoad = SceneManager.LoadSceneAsync("Battle");
        asyncLoad.allowSceneActivation = false;

        // 로딩이 완료될 때까지 진행 상태를 갱신
        while (asyncLoad.progress < 0.9f)
        {
            yield return null; // 다음 프레임까지 대기
        }
        GoToMainScene();
    }
    public void OnClickedStartImage()
    {
        tapToStartImage.gameObject.SetActive(false);
        StopCoroutine(blinkCoroutine);
        GoogleAuthLoad();
    }
    private void GoToMainScene()
    {
        if (DataManager.isLogin)
        {
            if (asyncLoad != null)
            {
                asyncLoad.allowSceneActivation = true;
            }
        }
    }

    private IEnumerator BlinkTapToStart()
    {
        while (true)
        {
            yield return StartCoroutine(UtilityManager.FadeUi(tapToStartText, 1.5f, false, 0f));
            yield return StartCoroutine(UtilityManager.FadeUi(tapToStartText, 0.7f, true, 1f));
            yield return new WaitForSeconds(0.4f);
        }
    }

}
