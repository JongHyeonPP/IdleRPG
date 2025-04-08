using UnityEngine;

[CreateAssetMenu(fileName = "CompanionTechData", menuName = "Scriptable Objects/CompanionTechData")]
public class CompanionTechData : ScriptableObject
{

    [Header("AppearanceData")]
    public AppearanceData appearanceData;
    [Header("Info")]
    public string techName;
    public SkillData techSkill;
    [Header("Reward")]
    public int dia;
    public int clover;
}
