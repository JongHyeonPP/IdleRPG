//using System.Collections;
//using UnityEngine;

//public class DialogueAction : IStoryAction
//{
//    private string talker, text;
//    private Color color;

//    public DialogueAction(string talker, string text, Color color)
//    {
//        this.talker = talker;
//        this.text = text;
//        this.color = color;
//    }

//    public IEnumerator Execute(StoryRunner runner)
//    {
//        runner.ui.DisplayLine(talker, text, color);
//        yield return runner.WaitForClick();
//    }
//}
