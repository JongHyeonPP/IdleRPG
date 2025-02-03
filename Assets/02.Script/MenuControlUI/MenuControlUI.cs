using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MenuChangeUI : MonoBehaviour
{
    public VisualElement root { private get; set; }
    [SerializeField] StatUI statUI;
    [SerializeField] WeaponUI weaponUI;
    [SerializeField] SkillUI skillUI;
    [SerializeField] WeaponBookUI weaponBookUI;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
    }
    private void Start()
    {
        var mainElement = root.Q<VisualElement>("ButtonParent");
        var menuParent = root.Q<VisualElement>("MenuParent");
        
        for (int i = 0; i < mainElement.childCount; i++)
        {
            int localIndex = i;
            var menuButton = mainElement.ElementAt(localIndex);
            menuButton.RegisterCallback<ClickEvent>(evt => ChangeUI(localIndex));
        }

        menuParent.Add(statUI.root);
        menuParent.Add(weaponUI.root);
        menuParent.Add(skillUI.root);
        menuParent.Add(weaponBookUI.root);
    }

    private void ChangeUI(int i)
    {
        BattleBroker.OnMenuUIChange?.Invoke(i);
    }


}
