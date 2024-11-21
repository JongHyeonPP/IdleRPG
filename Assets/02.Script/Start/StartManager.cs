using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

//Start������ ����ž��ϴ� ��ɵ��� ����ϴ� Ŭ����
public class StartManager : MonoBehaviour
{
    private AsyncOperation _asyncLoad;//�񵿱� �� �ε��� ���� ��Ȳ
    [SerializeField] StartMainUI startMainUI;//S
    private void Awake()
    {
        StartBroker.OnAuthenticationComplete += UnderTextLoadGameData;
        StartBroker.OnDataLoadComplete += PrepareBattleScene;
    }
    //Battle ���� �ε� ���̶�� ���� Battle ���� �ε��ϴ� �ڷ�ƾ�� ȣ���Ѵ�.
    private void PrepareBattleScene()
    {
        startMainUI.LoadingLebelSet();
        StartCoroutine(LoadMainScene());
    }
    //���� �α����� �� ���Ŀ� ������ �ҷ����� ���̶�� ����.
    private void UnderTextLoadGameData()
    {
        startMainUI.SetBottomLabelText("������ �ҷ����� ��");
    }
    //���� �α����� �ؾ��ϴ� �������� �ߵ��ϴ� �޼���
    public void GoogleAuthLoad()
    {
        startMainUI.SetBottomLabelText("���� �α��� ��");
        GameManager.instance.LoadGoogleAuth();
    }
    //Main ���� �ε��ϴ� �ڷ�ƾ. �񵿱⿩�� Freeze ������ �־ ���� �ڷ�ƾ���� �ؾ��ϳ� ����.
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
    //ȭ���� ��ġ���� �� ���� �α����� �����Ѵ�.
    public void OnClickedStartImage()
    {
        GoogleAuthLoad();
    }
    //Battle ������ �̵��Ѵ�.
    private void GoToBattleScene()
    {
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
    }
}
