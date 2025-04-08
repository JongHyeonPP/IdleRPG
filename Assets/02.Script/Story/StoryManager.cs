using EnumCollection;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryManager : MonoBehaviour
{
    public CameraController cameracontroller;
    public List<StoryPrefabData> storyPrefabsList; 
    private List<GameObject> activePrefabs = new List<GameObject>();
    public Transform spawnPoint;
    public StoryUI _storyUI;
    public PlayerRendercamera _renderCamera;
    private bool _isNextTriggered = false;
    private AbstractStoryObjectController currentStoryController;
    private Dictionary<int, AbstractStoryObjectController> _storyControllers = new Dictionary<int, AbstractStoryObjectController>();
    private int _currentStoryIndex = 0;

    private void Awake()
    {
        TextReader.LoadData();
     
    }
   
    private void OnEnable()
    {
        BattleBroker.ChallengeRank += OnChallengeRank;
        BattleBroker.SwitchToBattle += ClearStoryPrefabs;
    }
    private void OnChallengeRank(Rank rank)
    {
       

    }

    public void StoryStart(int i)
    {
       
        LoadStoryPrefabs(i);
       
    }
    private void ClearStoryPrefabs()
    {

        foreach (var obj in activePrefabs)
        {
            Destroy(obj);
        }
        activePrefabs.Clear();
    }
    private void LoadStoryPrefabs(int storyIndex)
    {
        StoryPrefabData storyData = storyPrefabsList.Find(x => x.storyIndex == storyIndex);

        foreach (var prefab in storyData.storyPrefabs)
        {
            GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            activePrefabs.Add(obj);

            if (storyIndex == 1)
            {
                currentStoryController = obj.GetComponent<FirstStoryController>();
            }
        }

        cameracontroller.SwitchToCamera(false);
        
        int startIndex = storyIndex * 1000 + 1;
        _currentStoryIndex = storyIndex;
        if (storyIndex == 1)
        {
            _storyControllers.Add(0, currentStoryController);
            StartCoroutine(FirstStoryStart(startIndex));
        }
        if (storyIndex == 2)
        {
            _storyControllers.Add(0, currentStoryController);
            StartCoroutine(SecondStoryStart(startIndex));
        }

    }

    private IEnumerator FirstStoryStart(int storyIndex)
    {
        RenderTexture renderTexture = _renderCamera.GetRenderTexture();
        _storyUI.SetImage(renderTexture);

        if (currentStoryController is FirstStoryController firstStory)
        {
            StartCoroutine(firstStory.Run(firstStory.protagonist));
        }

        _storyUI.SetStoryText("나", "어디지...?", Color.black);
        _renderCamera.SetCharacterDisplayTarget("나");
        _storyUI.RegisterNextButtonClick(() => _isNextTriggered = true);

        yield return new WaitUntil(() => _isNextTriggered);

        _isNextTriggered = false;
    }
    private IEnumerator SecondStoryStart(int storyIndex)
    {
        RenderTexture renderTexture = _renderCamera.GetRenderTexture();
        _storyUI.SetImage(renderTexture);

        if (currentStoryController is FirstStoryController firstStory)
        {
            StartCoroutine(firstStory.Run(firstStory.protagonist));
        }

        _storyUI.SetStoryText("나", "어디지...?", Color.black);
        _renderCamera.SetCharacterDisplayTarget("나");
        _storyUI.RegisterNextButtonClick(() => _isNextTriggered = true);

        yield return new WaitUntil(() => _isNextTriggered);

        _isNextTriggered = false;
    }
    public void NextStorySegment(int index)
    {
        int storyIndex = index / 1000 -1;
        if (!_storyControllers.ContainsKey(storyIndex))
            return;

        AbstractStoryObjectController currentStory = _storyControllers[storyIndex];

        TextData textData = TextReader.GetTextData(index);
        Color textColor = (textData.Talker == "돼지") ? Color.red : Color.black;
        _storyUI.SetStoryText(textData.Talker, textData.Text, textColor);
        _renderCamera.SetCharacterDisplayTarget(textData.Talker);

        if (currentStory is FirstStoryController firstStory)
        {
            if (index == 1003)
            {
                GameObject bigPig = firstStory.GetTargetObject("BigPig_Pink");
                if (bigPig != null)
                {
                    StartCoroutine(firstStory.Run(bigPig));
                }
            }
            else if (index == 1006)
            {
                GameObject pig = firstStory.GetTargetObject("Pig_Pink");
                if (pig != null)
                {
                    StartCoroutine(firstStory.Run(pig));
                }

                StartCoroutine(firstStory.RunAway(firstStory.protagonist));
                StartCoroutine(_storyUI.FadeEffect(false));

                EndStory();
            }
        }
        else if(currentStory is SecondStoryController secondstory)
        {

            if (index == 2003)
            {
                GameObject bigPig = secondstory.GetTargetObject("BigPig_Pink");
                if (bigPig != null)
                {
                    StartCoroutine(secondstory.Run(bigPig));
                }
            }
            else if (index == 2006)
            {
                GameObject pig = secondstory.GetTargetObject("Pig_Pink");
                if (pig != null)
                {
                    StartCoroutine(secondstory.Run(pig));
                }

                StartCoroutine(secondstory.RunAway(secondstory.protagonist));
                StartCoroutine(_storyUI.FadeEffect(false));

                EndStory();
            }
        }
       
    }
    private void EndStory()
    {
        _storyUI.ResetStoryUI();
        _currentStoryIndex++; 
    }
    public int GetCurrentStoryIndex()
    {
        return _currentStoryIndex;
    }
}
    



