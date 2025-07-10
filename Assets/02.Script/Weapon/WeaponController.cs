using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
using UnityEngine.Playables;
public class WeaponController : MonoBehaviour
{
    [HideInInspector]public WeaponData weaponData;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//�ƹ� ���⵵ �� ���� �� ��� ���� ������
    public WeaponType weaponType;
    private void Awake()//Start���� �������� ��
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
    //��������Ʈ �ο�
    [ContextMenu("Test")]
    public void TestWeapon()
    {
        string uid = "Melee_501";
        WeaponData weapon = WeaponManager.instance.weaponDict[uid];
        EquipWeapon(weapon, weapon.WeaponType);
    }
}
