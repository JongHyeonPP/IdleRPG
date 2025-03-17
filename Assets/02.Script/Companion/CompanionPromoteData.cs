using UnityEngine;

[CreateAssetMenu(fileName = "CompanionPromoteData", menuName = "Scriptable Objects/CompanionPromoteData")]
public class CompanionPromoteData : ScriptableObject
{
    public float[] power;
    public float[] criticalDamage;
    public float[] maxHp;
    public float[] hpRecover;
    public float[] maxMp;
    public float[] mpRecover;
    public float[] goldAscend;
    public float[] resist;
    public float[] penetration;
    public float[] expAscend;

    public float[] probabilityInRarity;
}
