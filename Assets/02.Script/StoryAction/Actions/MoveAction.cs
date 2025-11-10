using System.Collections;
using UnityEngine;

public class MoveAction : IStoryAction
{
    private readonly GameObject target;
    private readonly Vector3 dir;
    private readonly float speed;
    private readonly float time;

    public MoveAction(GameObject target, Vector3 dir, float speed, float time)
    {
        this.target = target;
        this.dir = dir;
        this.speed = speed;
        this.time = time;
    }

    public IEnumerator Execute(StoryRunner runner)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            target.transform.Translate(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
