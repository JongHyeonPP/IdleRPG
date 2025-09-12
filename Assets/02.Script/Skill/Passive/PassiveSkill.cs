using EnumCollection;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PassiveSkill : MonoBehaviour
{
    public SkillData healOnHit;
    [Range(0, 100)] public float healProc = 20f;

    public SkillData damagePlus;

    public SkillData doubleHit;
    public SkillData expPlus;
    private GameData _gd;

    void Awake()
    {
        _gd = StartBroker.GetGameData();
    }
    
 
    public bool TryGetDamagePlus(out float percent, out int level)
    {
        percent = 0f;
        if (!TryGetLevel(damagePlus, out level)) return false;
        percent = damagePlus.value[level];                
        return percent > 0f;
    }

    public bool TryGetHealOnHit(out float healPercent, out int level)
    {
        healPercent = 0f;
        if (!TryGetLevel(healOnHit, out level)) return false;
      //  if (!TryProc(healProc)) return false;            
        healPercent = healOnHit.value[level];           
        return healPercent > 0f;
    }

    public bool TryGetDoubleHit(out float extraPercent, out int level)
    {
        extraPercent = 0f;
        if (!TryGetLevel(doubleHit, out level)) return false;    
        extraPercent = doubleHit.value[level];           
        return extraPercent > 0f;
    }
    public bool TryGetPlusExp(out float percent, out int level)
    {
        percent = 0f;
        if (!TryGetLevel(expPlus, out level)) return false;
        percent = expPlus.value[level];
        return percent > 0f;
    }
    private bool TryGetLevel(SkillData sd, out int level)
    {
        level = 0;
        if (sd == null || _gd?.skillLevel == null) return false;
        if (!_gd.skillLevel.TryGetValue(sd.uid, out level)) return false;
        return (level >= 1) && (level < sd.value.Count);
    }

    private bool TryProc(float percent)
    {
        if (percent >= 100f) return true;
        return percent > 0f && UnityEngine.Random.value < (percent / 100f);
    }
    [ContextMenu("Force Passive Skills to Level 5")]
    private void ForcePassiveLevel5()
    {
        if (_gd?.skillLevel == null)
            return;

        if (healOnHit != null)
            _gd.skillLevel[healOnHit.uid] = 5;

        if (damagePlus != null)
            _gd.skillLevel[damagePlus.uid] = 5;

        if (doubleHit != null)
            _gd.skillLevel[doubleHit.uid] = 5;
        if (expPlus != null)
            _gd.skillLevel[expPlus.uid] = 5;

        Debug.Log("5레벨설정");
    }
}
