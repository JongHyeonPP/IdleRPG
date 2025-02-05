using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryManager : MonoBehaviour
{
    public TextReader _textReader;
    public StoryPlayerController _playercontroller;
    private VisualElement _root;
    private VisualElement _main;
    private Label _label;
    private Button _skipButton;
    public PigController pigcontroller;
    public CameraController cameracontroller;
    private VisualElement _fadeElement;
    private float _fadeDuration = 2f;
    public TransitionManager transitionmanager;
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _main=_root.Q<VisualElement>("Main");
        _label=_root.Q<Label>("TextLabel");
        _skipButton = _root.Q<Button>("SkipButton");
        _fadeElement = _root.Q<VisualElement>("FadeElement");
      
        _skipButton.clickable.clicked += () => Skip();
       
    }
    public IEnumerator FadeEffect()//√πΩ√¿€
    {
        _fadeElement.style.display = DisplayStyle.Flex;
        yield return StartCoroutine(Fade(1, 0));
        cameracontroller.SwitchToCamera(false);
        yield return StartCoroutine(FirstStoryStart());
        yield return StartCoroutine(Fade(0, 1)); 
        _fadeElement.style.display = DisplayStyle.None;
        transitionmanager.SwitchToBattleMode();
        cameracontroller.SwitchToCamera(true);
    }
    private IEnumerator Fade(float startOpacity, float endOpacity)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float opacity = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / _fadeDuration);
            _fadeElement.style.opacity = opacity;

            yield return null;
        }

        _fadeElement.style.opacity = endOpacity;
    }
    private IEnumerator FirstStoryStart()
    {
        StartCoroutine(_playercontroller.TranslatePlayerCoroutine());
        yield return new WaitForSeconds(2);
        for (int i=1;i<7; i++)
        {
            TextData textData = _textReader.GetTextData(i);
            if (textData.Talker == "µ≈¡ˆ")
            {
                _label.style.color = Color.red;
            }
            else
            {
                _label.style.color = Color.black;
            }
            _label.text = $"{textData.Talker}: {textData.Text}";
            if (i == 3)
            {
                foreach (var pig in FindObjectsOfType<PigController>())
                {
                    if (pig.gameObject.name == "BigPig_Pink")
                    {
                        StartCoroutine(pig.TranslateBigPigs());
                    }
                }
               
            }
            
            yield return new WaitForSeconds(_textReader.GetTextData(i).Term);
            if (i == 6)
            {
                foreach (var pig in FindObjectsOfType<PigController>())
                {
                    if (pig.gameObject.name == "Pig_Pink")
                    {
                        StartCoroutine(pig.TranslatePigs());
                    }
                }
                StartCoroutine(_playercontroller.Run());
            }
        }
        
    }
    private void Skip()
    {
       // SceneManager.LoadScene("Battle");
    }
    public void HideStoryUI()
    {
        _main.style.display = DisplayStyle.None;
    }
}
