using UnityEngine;

public class Chapter0 : StoryChapter
{
    [SerializeField] GameObject protagonist;
    [SerializeField] GameObject pig;

    public override void BuildActions(StoryRunner runner)
    {
        runner.AddAction(runner.FadeInOut());
        runner.AddAction(runner.Dialogue("³ª", "¿©±ä ¾îµðÁö...?"));
        runner.AddAction(runner.Move(protagonist, Vector3.right, 2f, 2f));
        runner.AddAction(runner.Dialogue("µÅÁö", "²ôÀ¹... ³Í ´©±¸¾ß?"));
        runner.AddAction(runner.Move(pig, Vector3.left, 1.5f, 2f));

    }
}
