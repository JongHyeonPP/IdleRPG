using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
using static WeaponData;

public class WeaponEffectManager : MonoBehaviour
{
    private List<WeaponEffect> _activeEffects = new List<WeaponEffect>();

    //public void ActivateWeaponEffects(WeaponData weaponData)
    //{
    //    // 이전 효과 초기화
    //    _activeEffects.Clear();

    //    if (weaponData?.weaponEffects == null || weaponData.weaponEffects.Length == 0)
    //        return;

    //    // 새 무기의 효과 추가
    //    _activeEffects.AddRange(weaponData.weaponEffects);
    //}

    /// <summary>
    /// 모든 효과 초기화 (무기 해제 시 호출)
    /// </summary>
    public void DeactivateAllEffects()
    {
        _activeEffects.Clear();
    }

    /// <summary>
    /// 특정 효과가 있는지 확인
    /// </summary>
    //public bool HasEffect(WeaponEffectType type)
    //{
    //    return _activeEffects.Exists(e => e.type == type);
    //}

    ///// <summary>
    ///// 효과 값 가져오기 (예: Reflect 비율 등)
    ///// </summary>
    //public float GetEffectValue(WeaponEffectType type)
    //{
    //    var effect = _activeEffects.Find(e => e.type == type);
    //    return effect != null ? effect.value : 0f;
    //}

    ///// <summary>
    ///// 한 번만 발동되는 효과(예: 부활) 제거
    ///// </summary>
    //public void ConsumeEffect(WeaponEffectType type)
    //{
    //    _activeEffects.RemoveAll(e => e.type == type);
    //}
}
