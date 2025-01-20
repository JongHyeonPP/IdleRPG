using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneControlManager : MonoBehaviour
{
    public static SceneControlManager instance { get; private set; } // �̱��� �ν��Ͻ�

    private VisualElement _root;
    private VisualElement _rootChild;
    private bool _isSceneLoaded = false; // �� �ε� �Ϸ� ����
    private bool _isFadeComplete = false; // fade-in �ִϸ��̼� �Ϸ� ����

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject); // �ߺ� �ν��Ͻ� ����
            return;
        }

        // UIDocument���� VisualElement ��������
        _root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = _root.Q<VisualElement>("FadeInSceneChange");
        _root.style.display = DisplayStyle.None;
        
    }

    public static void SceneChangeWithFade(string sceneName)
    {
        instance._root.style.display = DisplayStyle.Flex;
        // ���� �ʱ�ȭ
        instance._isSceneLoaded = false;
        instance._isFadeComplete = false;

        // fade-in Ŭ���� �߰�
        instance._rootChild.AddToClassList("fade-in");

        // �� �񵿱� �ε�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.completed += (operation) =>
        {
            instance._isSceneLoaded = true;
            instance.CheckAndRemoveFade();
        };

        // TransitionEndEvent�� ����Ͽ� �ִϸ��̼� �Ϸ� �� �۾� ó��
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
        // �� �ε�� fade-in �ִϸ��̼� ��� �Ϸ� �� ó��
        if (_isSceneLoaded && _isFadeComplete)
        {
            _rootChild.RemoveFromClassList("fade-in");
            //fade-in�� ������°� ���� ���� �Ʒ��� ȣ�� �ǵ���
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
