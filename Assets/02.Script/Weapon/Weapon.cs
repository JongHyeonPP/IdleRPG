using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
public class Weapon : MonoBehaviour
{
    public WeaponData _weaponData;
    private SpriteRenderer weaponRenderer;

    private void Start()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
        string loadedWeaponId = StartBroker.GetGameData().weaponId;
        WeaponData loadedWeapon = WeaponManager.instance.GetPlayerWeapon(loadedWeaponId);
        if (loadedWeapon != null)
            EquipWeapon(loadedWeapon);
    }
    private void OnEquipWeapon(object obj) => EquipWeapon((WeaponData)obj);
    private void EquipWeapon(WeaponData obj)
    {
        WeaponData weaponData = obj;
        _weaponData = weaponData;
        weaponRenderer.sprite = weaponData.weaponSprite;
    }
    //»ö±òÀÌÆåÆ® ºÎ¿©





}
