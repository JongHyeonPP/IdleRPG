using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryUI : MonoBehaviour
{
   
    private VisualElement _main;
    private Label _label;
    private Button _skipButton;
    private VisualElement _fadeElement;
    private float _fadeDuration = 3f;
    public VisualElement root { get; private set; }
    public StoryManager storyManager;
    public CameraController cameracontroller;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _main = root.Q<VisualElement>("Main");
        _label = root.Q<Label>("TextLabel");
        _skipButton = root.Q<Button>("SkipButton");
        _fadeElement = root.Q<VisualElement>("FadeElement");

        _skipButton.clickable.clicked += () => Skip();
    }

    public void UpdateText(string talker, string text, Color color)
    {
        _label.style.color = color;
        _label.text = $"{talker}: {text}";
    }

    public IEnumerator FadeEffect(bool storymode,int index=0)
    {
        
        float elapsedTime = 0f;


        if (storymode)
        {
            _main.style.display = DisplayStyle.Flex;
            _fadeElement.style.display = DisplayStyle.Flex;
            _fadeElement.style.opacity = 1f; 
            yield return new WaitForSeconds(3.0f);

            if (index == 1)
            {
                storyManager.StoryStart(index);
               
            }

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float opacity = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, elapsedTime / _fadeDuration));
                _fadeElement.style.opacity = opacity;
                yield return null;
            }
            _fadeElement.style.opacity = 0f; 
        }
        else
        {
            _fadeElement.style.opacity = 1f; 
          

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float opacity = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0f, 1f, elapsedTime / _fadeDuration));
                _fadeElement.style.opacity = opacity;
                yield return null;
            }

            _fadeElement.style.opacity = 0f;
            _main.style.display = DisplayStyle.None;
            cameracontroller.SwitchToCamera(true);
            BattleBroker.SwitchBattle();
        }
    }
    public void SetStoryText(string talker, string text, Color color)
    {
        _label.text = $"{talker}: {text}";
        _label.style.color = color;
    }
    private void Skip()
    {
        BattleBroker.SwitchBattle();
    }
}
