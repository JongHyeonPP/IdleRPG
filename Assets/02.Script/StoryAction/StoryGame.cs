using System;
using UnityEngine;

public class StoryGame : MonoBehaviour
{
    [SerializeField] private StoryRunner _runner;
    [SerializeField] private StoryChapter[] _chapters;
    [SerializeField] private GameObject _storyCamera;
    private void Start()
    {
        BattleBroker.SwitchToStory += RunStory;
        BattleBroker.SwitchToBattle += SwitchToBattle;
        SwitchToBattle();
    }

    private void SwitchToBattle()
    {
        Camera.main.gameObject.SetActive(true);
         _storyCamera.SetActive(false);
    }

    private void RunStory(int index)
    {
        Camera.main.gameObject.SetActive(false);
        _storyCamera.SetActive(true);
        _chapters[index].BuildActions(_runner);
        _runner.Run();
    }

    [ContextMenu("Test Chapter 0")]
    private void Chapter0Test() => BattleBroker.SwitchToStory(0);
}
