using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AbilityData
{
    public string AbilityName;
    public StatusType statusType;
    public List<float> Values;
    public List<float> Probabilities; 
}
[CreateAssetMenu(fileName = "AbilityTable", menuName = "Scriptable Objects/AbilityTable")]
public class AbilityTable : ScriptableObject
{
    public List<AbilityData> Abilities;
}
