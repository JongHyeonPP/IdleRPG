using UnityEngine;

[CreateAssetMenu(fileName = "SkillAcquireInfo", menuName = "Scriptable Objects/SkillAcquireInfo")]
public class SkillAcquireInfo : ScriptableObject
{
    public int acquireLevel;
    [Header("PlayerSkill")]
    public SkillData playerSkillData;
    [Header("PartySkill")]
    public SkillData partySkillData;
}
