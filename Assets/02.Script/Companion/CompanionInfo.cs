using EnumCollection;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "CompanionStatus", menuName = "Scriptable Objects/CompanionStatus")]
public class CompanionStatus : ScriptableObject
{
    public SkillData[] companionSkillArr;
    public CompanionTechData companionTechData_0; 
    public CompanionTechData companionTechData_1_0; 
    public CompanionTechData companionTechData_1_1; 
    public CompanionTechData companionTechData_2_0; 
    public CompanionTechData companionTechData_2_1; 
    public CompanionTechData companionTechData_3_0; 
    public CompanionTechData companionTechData_3_1; 
    public string companionName;
}
