using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryManager : MonoBehaviour
{
    
    private StoryPlayerController _playercontroller;
    private PigController pigcontroller;
    public CameraController cameracontroller;
    public List<StoryPrefabData> storyPrefabsList; 
    private List<GameObject> activePrefabs = new List<GameObject>();
    public Transform spawnPoint;
    public StoryUI _storyUI;
    private void Awake()
    {
        TextReader.LoadData();
    }
    private void OnEnable()
    {
        StoryBroker.StoryModeStart += StartStoryCoroutine;
    }

    private void StartStoryCoroutine(int i)
    {
        StartCoroutine(StoryStart(i)); 
    }

    public IEnumerator StoryStart(int i)//Ã¹½ÃÀÛ
    {
         _storyUI.ShowStoryUI();
        yield return StartCoroutine(_storyUI.FadeEffect(true));
        cameracontroller.SwitchToCamera(false);
        LoadStoryPrefabs(i);
        if (i == 1)
        {
            yield return StartCoroutine(FirstStoryStart());//ÀÎµ¦½º
        }

        yield return StartCoroutine(_storyUI.FadeEffect(false));
        BattleBroker.SwitchBattle();
        cameracontroller.SwitchToCamera(true);
        _storyUI.HideStoryUI();
        ClearStoryPrefabs();
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

            Transform backgroundTransform = obj.transform.Find("StoryBackground");
            if (backgroundTransform != null)
            {
                background = backgroundTransform.gameObject;
            }
        }

        if (background != null)
        {
            cameracontroller.SetStoryBackground(background.transform); 
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

        yield return new WaitForSeconds(2);
        for (int i=1;i<7; i++)
        {
            TextData textData = TextReader.GetTextData(i);
            Color textColor = (textData.Talker == "µÅÁö") ? Color.red : Color.black;
            _storyUI.SetStoryText(textData.Talker, textData.Text, textColor);
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
        BattleBroker.SwitchBattle();
    }
    
}
