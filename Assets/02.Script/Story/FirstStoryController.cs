using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstStoryController : AbstractStoryObjectController
{
    public override IEnumerator Run(GameObject target)
    {
        if (target == null) yield break;
        if (target == protagonist) 
        {
            SetAnimatorState(protagonist, "RunState", 0.5f);
            yield return MoveObject(protagonist, Vector3.left, 4.5f, 2f);
            SetAnimatorState(protagonist, "RunState", 0f);

            for (int i = 0; i < 2; i++)
            {
                yield return RotateObject(protagonist, 1f, 1f);
                yield return RotateObject(protagonist, 180f, 1f);
            }
        }
        else if (target.name == "BigPig_Pink")
        {
            yield return MoveObject(target, Vector3.left, 4f, 1f);
        }
        else if (target.name == "Pig_Pink")
        {
            yield return MoveObject(target, Vector3.left, 4f, 1f);
        }
    }

    public override IEnumerator RunAway(GameObject target)
    {
        if (protagonist != null)
        {
            
            yield return RotateObject(protagonist, 1f, 1f);
            SetAnimatorState(protagonist, "RunState", 0.5f);
            yield return MoveObject(protagonist, Vector3.left, 2f, 2f);
        }
    }
}
