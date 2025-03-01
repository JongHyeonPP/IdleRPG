using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
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
    //��������Ʈ �ο�





}
