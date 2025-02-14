using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//아무 무기도 안 꼈을 시 들고 있을 몽둥이
    private void Awake()//BattleManager.Start보다 먼저여야 함
    {
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
    }
    private void OnEquipWeapon(object obj) => EquipWeapon((WeaponData)obj);
    private void EquipWeapon(WeaponData obj)
    {
        WeaponData weaponData = obj;
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
