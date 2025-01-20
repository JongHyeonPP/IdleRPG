using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneControlManager : MonoBehaviour
{
    public static SceneControlManager instance { get; private set; } // 싱글턴 인스턴스

    private VisualElement _root;
    private VisualElement _rootChild;
    private bool _isSceneLoaded = false; // 씬 로드 완료 여부
    private bool _isFadeComplete = false; // fade-in 애니메이션 완료 여부

    private void Awake()
    {
        // 싱글턴 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 제거
            return;
        }

        // UIDocument에서 VisualElement 가져오기
        _root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = _root.Q<VisualElement>("FadeInSceneChange");
        _root.style.display = DisplayStyle.None;
        
    }

    public static void SceneChangeWithFade(string sceneName)
    {
        instance._root.style.display = DisplayStyle.Flex;
        // 상태 초기화
        instance._isSceneLoaded = false;
        instance._isFadeComplete = false;

        // fade-in 클래스 추가
        instance._rootChild.AddToClassList("fade-in");

        // 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.completed += (operation) =>
        {
            instance._isSceneLoaded = true;
            instance.CheckAndRemoveFade();
        };

        // TransitionEndEvent를 사용하여 애니메이션 완료 후 작업 처리
        instance._rootChild.RegisterCallback<TransitionEndEvent>((e) =>
        {
            if (instance._rootChild.ClassListContains("fade-in"))
            {
                instance._isFadeComplete = true;
                instance.CheckAndRemoveFade();
            }
        });
    }

    private void CheckAndRemoveFade()
    {
        // 씬 로드와 fade-in 애니메이션 모두 완료 시 처리
        if (_isSceneLoaded && _isFadeComplete)
        {
            _rootChild.RemoveFromClassList("fade-in");
            //fade-in이 사라지는게 끝난 다음 아래가 호출 되도록
            instance._root.style.display = DisplayStyle.Flex;
        }
    }
    [ContextMenu("GoToTest")]
    public void GoToTest()
    {
        SceneChangeWithFade("Test");
    }
    [ContextMenu("GoToBattle")]
    public void GoToBattle()
    {
        SceneChangeWithFade("Battle");
    }
}
