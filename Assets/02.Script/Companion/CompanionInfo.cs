using EnumCollection;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "CompanionStatus", menuName = "Scriptable Objects/CompanionStatus")]
public class CompanionStatus : ScriptableObject
{
    public SkillData[] companionSkillArr;
    public SkillData companionEffect; 
    public string companionName;
    public string companionJob;
}
