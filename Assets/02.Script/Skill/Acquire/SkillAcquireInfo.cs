using UnityEngine;

[CreateAssetMenu(fileName = "SkillAcquireInfo", menuName = "Scriptable Objects/SkillAcquireInfo")]
public class SkillAcquireInfo : ScriptableObject
{
    public int acquireLevel;
    public SkillData SkillData;
}
