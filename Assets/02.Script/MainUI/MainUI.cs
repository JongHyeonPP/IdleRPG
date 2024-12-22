using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MainUI : MonoBehaviour
{
   
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        BattleBroker.OnUIChange?.Invoke(1);

        var statButton = root.Q<Button>("StatUI");
        statButton.RegisterCallback<ClickEvent>(evt => BattleBroker.OnUIChange?.Invoke(1)); 

        var weaponButton = root.Q<Button>("WeaponUI");
        weaponButton.RegisterCallback<ClickEvent>(evt => BattleBroker.OnUIChange?.Invoke(2)); 

        var weaponBookButton = root.Q<Button>("WeaponBook");
        weaponBookButton.RegisterCallback<ClickEvent>(evt => BattleBroker.OnUIChange?.Invoke(3));
    }

   

}
