using UnityEngine;
using EnumCollection;
using UnityEngine.VFX;
public class Weapon : MonoBehaviour
{
    [HideInInspector] public WeaponData _weaponData;
    private SpriteRenderer weaponRenderer;
    [SerializeField] Sprite defaultWeaponSprite;//�ƹ� ���⵵ �� ���� �� ��� ���� ������
    private void Start()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
        string loadedWeaponId = StartBroker.GetGameData().weaponId;
        if (!string.IsNullOrEmpty(loadedWeaponId))
        {
            WeaponData loadedWeapon = WeaponManager.instance.weaponDict[loadedWeaponId];
            EquipWeapon(loadedWeapon);
        }
        else
        {
            weaponRenderer.sprite = defaultWeaponSprite;
        }
    }
    private void OnEquipWeapon(object obj) => EquipWeapon((WeaponData)obj);
    private void EquipWeapon(WeaponData obj)
    {
        WeaponData weaponData = obj;
        _weaponData = weaponData;
        weaponRenderer.sprite = weaponData.WeaponSprite;
    }
    //��������Ʈ �ο�





}
