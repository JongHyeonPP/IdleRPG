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
    private void Awake()//Start보다 먼저여야 함
    {
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
    }
    private void OnEquipWeapon(object obj, WeaponType weaponType) => EquipWeapon((WeaponData)obj, weaponType);
    private void EquipWeapon(WeaponData weaponData, WeaponType weaponType)
    {
        if (weaponData == null)
        {
            weaponRenderer.sprite = defaultWeaponSprite;
            return;
        }
        if (this.weaponType != weaponType)
            return;
        this.weaponData = weaponData;
        weaponRenderer.sprite = weaponData.WeaponSprite;
    }
    //색깔이펙트 부여
    [ContextMenu("Test")]
    public void TestWeapon()
    {
        string uid = "Melee_501";
        WeaponData weapon = WeaponManager.instance.weaponDict[uid];
        EquipWeapon(weapon, weapon.WeaponType);
    }
}
