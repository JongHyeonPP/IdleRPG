//using System.Collections;
//using UnityEngine;

//public class FadeAction : IStoryAction
//{
//    private readonly bool fadeIn;

//    public FadeAction(bool fadeIn)
//    {
//        this.fadeIn = fadeIn;
//    }

//    public IEnumerator Execute(StoryRunner runner)
//    {
//        if (fadeIn)
//            yield return runner.ui.FadeIn();
//        else
//            yield return runner.ui.FadeOut();
//    }
//}
