using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

//Start씬에서 진행돼야하는 기능들을 담당하는 클래스
public class StartManager : MonoBehaviour
{
    private AsyncOperation _asyncLoad;//비동기 씬 로드의 진행 상황
    [SerializeField] StartMainUI startMainUI;//S
    private void Awake()
    {
        StartBroker.OnAuthenticationComplete += UnderTextLoadGameData;
        StartBroker.OnDataLoadComplete += PrepareBattleScene;
    }
    //Battle 씬을 로드 중이라고 띄우고 Battle 씬을 로드하는 코루틴을 호출한다.
    private void PrepareBattleScene()
    {
        startMainUI.LoadingLebelSet();
        StartCoroutine(LoadMainScene());
    }
    //구글 로그인을 한 이후에 정보를 불러오는 중이라고 띄운다.
    private void UnderTextLoadGameData()
    {
        startMainUI.SetBottomLabelText("정보를 불러오는 중");
    }
    //구글 로그인을 해야하는 시점에서 발동하는 메서드
    public void GoogleAuthLoad()
    {
        startMainUI.SetBottomLabelText("구글 로그인 중");
        GameManager.instance.LoadGoogleAuth();
    }
    //Main 씬을 로드하는 코루틴. 비동기여도 Freeze 현상이 있어서 굳이 코루틴으로 해야하나 싶음.
    private IEnumerator LoadMainScene()
    {
        _asyncLoad = SceneManager.LoadSceneAsync("Battle");
        _asyncLoad.allowSceneActivation = false;

        while (_asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        GoToBattleScene();
    }
    //화면을 터치했을 때 구글 로그인을 유도한다.
    public void OnClickedStartImage()
    {
        GoogleAuthLoad();
    }
    //Battle 씬으로 이동한다.
    private void GoToBattleScene()
    {
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
    }
}
