using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
using UnityEngine.Playables;
public class WeaponController : MonoBehaviour
{
    [HideInInspector]public WeaponData weaponData;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//아무 무기도 안 꼈을 시 들고 있을 몽둥이
    public WeaponType weaponType;
   
    private string _currentAncientUID;
    private WeaponType _currentAncientType;
    private bool _hasAncientEffect;
    private void Awake()//Start보다 먼저여야 함
    {
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
    }
    private void OnEquipWeapon(object obj, WeaponType weaponType) => EquipWeapon((WeaponData)obj, weaponType);
   
    private void EquipWeapon(WeaponData weaponData, WeaponType weaponType)
    {
        if (this.weaponType != weaponType)
            return;

        this.weaponData = weaponData;

        if (weaponData == null)
        {
            weaponRenderer.sprite = defaultWeaponSprite;
            ClearAncientEffect(); 
            return;
        }

        weaponRenderer.sprite = weaponData.WeaponSprite;

        if (weaponData.IsAncientWeapon)
        {
            if (_hasAncientEffect &&
                _currentAncientUID == weaponData.UID &&
                _currentAncientType == weaponData.WeaponType)
                return;

            if (_hasAncientEffect)
                ClearAncientEffect();

            _hasAncientEffect = true;
            _currentAncientUID = weaponData.UID;
            _currentAncientType = weaponData.WeaponType;

            PlayerBroker.OnEquipAncientWeapon?.Invoke(_currentAncientUID, _currentAncientType);
        }
        else
        {
            ClearAncientEffect();
        }
    }
    private void ClearAncientEffect()
    {
        if (!_hasAncientEffect) return;
        PlayerBroker.OnUnequipAncientWeapon?.Invoke(_currentAncientUID, _currentAncientType);

        _hasAncientEffect = false;
        _currentAncientUID = null;
    }
    //색깔이펙트 부여
    [ContextMenu("600")]
    public void TestWeapon600()
    {
        string uid = "Melee_600";
        WeaponData weapon = WeaponManager.instance.weaponDict[uid];
        EquipWeapon(weapon, weapon.WeaponType);
    }
    [ContextMenu("601")]
    public void TestWeapon601()
    {
        string uid = "Melee_601";
        WeaponData weapon = WeaponManager.instance.weaponDict[uid];
        EquipWeapon(weapon, weapon.WeaponType);
    }
}