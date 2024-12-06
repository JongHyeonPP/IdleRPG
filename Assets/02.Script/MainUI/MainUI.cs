using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MainUI : MonoBehaviour
{
    [SerializeField]private WeaponUI _weaponUI;
    [SerializeField]private StatUI _statUI;
    [SerializeField]private WeaponBook _weaponBook;


    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        ShowStatUI();

        var statButton = root.Q<Button>("StatUI");
        statButton.RegisterCallback<ClickEvent>(evt => ShowStatUI());

        var weaponButton = root.Q<Button>("WeaponUI");
        weaponButton.RegisterCallback<ClickEvent>(evt => ShowWeaponUI());

        var weaponBookButton = root.Q<Button>("WeaponBook");
        weaponBookButton.RegisterCallback<ClickEvent>(evt => ShowWeaponBook());
    }

    private void ShowStatUI()
    {
       _statUI.ShowStatUI();
       _weaponUI.HideWeaponUI();
       _weaponBook.HideWeaponBook();
    }

    private void ShowWeaponUI()
    {
        _weaponUI.ShowWeaponUI();
        _statUI.HideStatUI();
        _weaponBook.HideWeaponBook();
    }
    private void ShowWeaponBook()
    {
        _weaponUI.HideWeaponUI();
        _statUI.HideStatUI();
        _weaponBook.ShowWeaponBook();
    }

}
