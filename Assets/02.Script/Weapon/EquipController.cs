using UnityEngine;

public class EquipController : MonoBehaviour
{
    private WeaponData _equippedWeapon;

    private void Start()
    {
        PlayerBroker.OnEquipWeapon += EquipWeapon;
    }

    private void EquipWeapon(object weaponData)
    {
        if (weaponData is WeaponData data)
        {
            _equippedWeapon = data;
            
        }

    }
}
