using EnumCollection;
using UnityEngine;

public class WeaponEffectManager : MonoBehaviour
{
    private Coroutine _effectRoutine;
    
    private bool _isReflecting;
    public bool IsMelee600Active { get; private set; }
    public bool IsMelee601Active { get; private set; }

    public bool IsRevivePossible { get; private set; }
    public void ActivateAncientEffect(string uid, WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Melee:
                switch (uid)
                {
                    case "Melee_600": ActivateMelee600(); break;
                    case "Melee_601": ActivateMelee601(); break;
                }
                break;

            case WeaponType.Staff:
                switch (uid)
                {
                    case "Staff_600": ActivateStaff600(); break;
                }
                switch (uid)
                {
                    case "Staff_601": ActivateStaff601(); break;
                }
                break;

            case WeaponType.Bow:
                ActivateBow600();
                break;

            case WeaponType.Shield:
                ActivateShield600();
                break;
        }
    }

    public void DeactivateAncientEffect(string uid, WeaponType type)
    {
        DeactivateAll();
    }

    public void DeactivateAll()
    {
        if (_effectRoutine != null)
        {
            StopCoroutine(_effectRoutine);
            _effectRoutine = null;
        }

        _isReflecting = false;
        IsMelee601Active = false;
        IsRevivePossible = false;
    }
    #region 600(드리아드)
    private void ActivateMelee600()
    {
        IsMelee600Active = true;
        IsRevivePossible = true;
    }
    public void ConsumeRevive()
    {
        IsRevivePossible = false;
    }
    public void ResetReviveIfMelee600Equipped()
    {
        if (IsMelee600Active)
            IsRevivePossible = true;
        else
            IsRevivePossible = false;
    }
    #endregion
    private void ActivateMelee601()
    {
        IsMelee601Active = true;
    }

    private void ActivateStaff600()
    {
        
    }
    private void ActivateStaff601()
    {
       
    }
    private void ActivateBow600()
    {
        
    }

    private void ActivateShield600()
    {

    }
}
