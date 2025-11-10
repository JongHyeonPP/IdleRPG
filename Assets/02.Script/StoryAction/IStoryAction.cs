using System.Collections;

public interface IStoryAction
{
    IEnumerator Execute(StoryRunner runner);
}
