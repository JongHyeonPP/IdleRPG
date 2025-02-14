using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryManager : MonoBehaviour
{
    
    private StoryPlayerController _playercontroller;
    private VisualElement _root;
    private VisualElement _main;
    private Label _label;
    private Button _skipButton;
    private PigController pigcontroller;
    public CameraController cameracontroller;
    private VisualElement _fadeElement;
    private float _fadeDuration = 2f;
    public List<StoryPrefabData> storyPrefabsList; 
    private List<GameObject> activePrefabs = new List<GameObject>();
    public CameraController cameraController;
    public Transform spawnPoint;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _main=_root.Q<VisualElement>("Main");
        _label=_root.Q<Label>("TextLabel");
        _skipButton = _root.Q<Button>("SkipButton");
        _fadeElement = _root.Q<VisualElement>("FadeElement");
        _skipButton.clickable.clicked += () => Skip();
        TextReader.LoadData();
    }
    private void OnEnable()
    {
        StoryBroker.StoryModeStart += StartFadeEffect;
        BattleBroker.SwitchBattle += HideStoryUI;
    }

    private void StartFadeEffect(int i)
    {
        StartCoroutine(FadeEffect(i)); 
    }

    public IEnumerator FadeEffect(int i)//√πΩ√¿€
    {
        _fadeElement.style.display = DisplayStyle.Flex;
        yield return StartCoroutine(Fade(1, 0));
        cameracontroller.SwitchToCamera(false);
        LoadStoryPrefabs(i);
        if (i == 1)
        {
            yield return StartCoroutine(FirstStoryStart());//¿Œµ¶Ω∫
        }
       
        yield return StartCoroutine(Fade(0, 1)); 
        _fadeElement.style.display = DisplayStyle.None;

        _fadeElement.style.display = DisplayStyle.Flex;
        yield return StartCoroutine(Fade(1, 0));
        _fadeElement.style.display = DisplayStyle.None;
        BattleBroker.SwitchBattle();
        cameracontroller.SwitchToCamera(true);
        _main.style.display = DisplayStyle.None;
        ClearStoryPrefabs();
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
    private void LoadStoryPrefabs(int storyIndex)
    {

        ClearStoryPrefabs();

        StoryPrefabData storyData = storyPrefabsList.Find(x => x.storyIndex == storyIndex);
        GameObject background = null; 

        foreach (var prefab in storyData.storyPrefabs)
        {
            GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            activePrefabs.Add(obj);

            if (obj.CompareTag("StoryBackground"))
            {
                background = obj;
            }
        }

        if (background != null)
        {
            cameraController.SetStoryBackground(background.transform); 
        }
    }

    private void ClearStoryPrefabs()
    {
        foreach (var obj in activePrefabs)
        {
            Destroy(obj);
        }
        activePrefabs.Clear();
    }
    private IEnumerator FirstStoryStart()
    {
        foreach (var playercontroller in FindObjectsOfType<StoryPlayerController>())
        {
            _playercontroller = playercontroller;
            StartCoroutine(_playercontroller.TranslatePlayerCoroutine());
        }

        //StartCoroutine(_playercontroller.TranslatePlayerCoroutine());
        yield return new WaitForSeconds(2);
        for (int i=1;i<7; i++)
        {
            TextData textData = TextReader.GetTextData(i);
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
            
            yield return new WaitForSeconds(TextReader.GetTextData(i).Term);
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
        if (_main != null&&_main.style.display== DisplayStyle.Flex)
        {
            _main.style.display = DisplayStyle.None;
        }
       
    }
}
