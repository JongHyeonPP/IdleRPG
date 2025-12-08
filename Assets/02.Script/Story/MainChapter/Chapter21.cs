using UnityEngine;

public class Chapter21 : StoryChapter
{
    public override StoryTalker[] LocalTalkers => new StoryTalker[]{};

    public override void BuildActions(StoryRunner runner)
    {
        throw new System.NotImplementedException();
    }
}
