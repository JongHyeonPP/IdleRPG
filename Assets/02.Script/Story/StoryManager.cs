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
        BattleBroker.SwitchBattle += ClearStoryPrefabs;
    }
   

    public void StoryStart(int i)//√πΩ√¿€
    {
       
        LoadStoryPrefabs(i);
       
    }
  
    private void LoadStoryPrefabs(int storyIndex)
    {
       

        StoryPrefabData storyData = storyPrefabsList.Find(x => x.storyIndex == storyIndex);

        foreach (var prefab in storyData.storyPrefabs)
        {
            GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            activePrefabs.Add(obj);

            
        }

        cameracontroller.SwitchToCamera(false);
        if (storyIndex == 1)
        {
            StartCoroutine(FirstStoryStart());//¿Œµ¶Ω∫
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
            Color textColor = (textData.Talker == "µ≈¡ˆ") ? Color.red : Color.black;
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
      
        StartCoroutine(_storyUI.FadeEffect(false));
       
        yield return new WaitForSeconds(4f);
      
        
    }
   
    
}
