using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryUI : MonoBehaviour
{
   
    private VisualElement _main;
    private Label _label;
    private VisualElement _fadeElement;
    private float _fadeDuration = 3f;
    public VisualElement root { get; private set; }
    public StoryManager storyManager;
    public CameraController cameraController;
    private Button _screenButton;
    private int _currentIndex = 1;
    private bool _isWaitingForClick = false;
    private Button _renderTextureimage;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _main = root.Q<VisualElement>("Main");
        _label = root.Q<Label>("TextLabel");
       
        _fadeElement = root.Q<VisualElement>("FadeElement");
        _screenButton=root.Q<Button>("ScreenButton");
        _screenButton.clicked += OnScreenButtonClick;

    }
    private void OnScreenButtonClick()
    {
        if (_isWaitingForClick)
        {
            int storyIndex = storyManager.GetCurrentStoryIndex();
            int key = storyIndex * 1000 + _currentIndex++;
            storyManager.NextStorySegment(key);
        }
    }
    public void ResetStoryUI()
    {
        _currentIndex = 1;
        _isWaitingForClick = false;
    }
    public void RegisterNextButtonClick(Action onClick)
    {
        _isWaitingForClick = true;  
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
            cameraController.SwitchToCamera(true);
            BattleBroker.SwitchToBattle();
        }
    }
    public void SetImage(RenderTexture renderTexture)
    {
        var Image = root.Q<Button>("Image");
        Image.style.backgroundImage = Background.FromRenderTexture(renderTexture);

    }
    public void SetStoryText(string talker, string text, Color color)
    {
        StopAllCoroutines(); 
        StartCoroutine(TypeText(talker, text, color));
    }
    private IEnumerator TypeText(string talker, string text, Color color)
    {
        _label.text = ""; 
        _label.style.color = color;

        string fullText = $"{talker}: {text}";

        foreach (char letter in fullText)
        {
            _label.text += letter;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
