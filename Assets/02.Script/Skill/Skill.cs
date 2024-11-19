using UnityEngine;

public class Skill : MonoBehaviour
{
    public SkillData data;
    public float currentCooltime;
    public int currentCoolAttack;
    public bool IsSkillAble()
    {
        if (data.byTime)
        {
            return currentCooltime == 0f;
        }
        else
        {
            return currentCoolAttack == 0;
        }
    }
}
