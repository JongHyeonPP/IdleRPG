using UnityEngine;

[CreateAssetMenu(fileName = "CompanionPromoteData", menuName = "Scriptable Objects/CompanionPromoteData")]
public class CompanionPromoteData : ScriptableObject
{
    [Header("기본 스탯 승급 배율")]
    public float[] power;              // 공격력
    public float[] criticalDamage;     // 치명타 피해량
    public float[] maxHp;              // 체력
    public float[] hpRecover;          // 체력 회복
    public float[] maxMp;              // 마나
    public float[] mpRecover;          // 마나 회복

    [Header("수익/보상 상승")]
    public float[] goldAscend;         // 골드 획득량
    public float[] expAscend;          // 경험치 획득량

    [Header("추가 옵션(정수/확률/버프 등)")]
    public float[] attBuff;            // 공격력 증가 버프
    public float[] defBuff;            // 피해 감소 버프


    [Header("확률 분포")]
    public float[] probabilityInRarity;
}
