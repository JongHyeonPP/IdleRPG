using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartManager : MonoBehaviour
{
    private AsyncOperation asyncLoad;
    [SerializeField] StartMainUI startMainUI;
    private void Awake()
    {
        GameManager.OnAuthenticationComplete += UnderTextLoadGameData;
        GameManager.OnDataLoadComplete += PrepareMainScene;
    }

    private void PrepareMainScene()
    {
        startMainUI.InitLabel();
        StartCoroutine(LoadMainScene());
    }

    private void UnderTextLoadGameData()
    {
        startMainUI.SetBottomLabelText("������ �ҷ����� ��");
    }

    public void GoogleAuthLoad()
    {
        startMainUI.SetBottomLabelText("���� �α��� ��");
        GameManager.instance.LoadGoogleAuth();
    }

    private IEnumerator LoadMainScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync("Battle");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        GoToMainScene();
    }

    public void OnClickedStartImage()
    {
        
        GoogleAuthLoad();
    }

    private void GoToMainScene()
    {
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        }
    }
}
