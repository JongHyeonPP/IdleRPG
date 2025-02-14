using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//�ƹ� ���⵵ �� ���� �� ��� ���� ������
    private void Awake()//BattleManager.Start���� �������� ��
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
    //��������Ʈ �ο�





}
