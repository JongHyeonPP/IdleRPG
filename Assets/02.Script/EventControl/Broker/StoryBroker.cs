using System;
using UnityEngine;

public static class StoryBroker
{
    public static Action<int> StoryModeStart;
    public static Action HideStoryMode;
    public static Action EndStoryMode;
}
