using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StoryChapter : MonoBehaviour
{

    public abstract void BuildActions(StoryRunner runner);
}
