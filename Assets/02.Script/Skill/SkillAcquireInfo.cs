using UnityEngine;

[CreateAssetMenu(fileName = "SkillAcquireInfo", menuName = "Scriptable Objects/SkillAcquireInfo")]
public class SkillAcquireInfo : ScriptableObject
{
    public int acquireLevel;
    [Header("PlayerSkill")]
    public SkillData playerSkillData;
    [Header("CompanionSkill")]
    public SkillData companionSkillData;
}
