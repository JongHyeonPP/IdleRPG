using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
public class WeaponController : MonoBehaviour
{
    [HideInInspector]public WeaponData weaponData;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//아무 무기도 안 꼈을 시 들고 있을 몽둥이
    public WeaponType weaponType;
    private void Awake()//Start보다 먼저여야 함
    {
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
    }
    private void OnEquipWeapon(object obj,WeaponType weaponType) => EquipWeapon((WeaponData)obj, weaponType);
    private void EquipWeapon(WeaponData weaponData, WeaponType weaponType)
    {
        if (weaponType != this.weaponType)
            return;
        this.weaponData = weaponData;
        if (weaponData != null)
        {
            weaponRenderer.sprite = weaponData.WeaponSprite;
        }
        else
        {
            weaponRenderer.sprite = defaultWeaponSprite;
        }
    }
    //색깔이펙트 부여





}
