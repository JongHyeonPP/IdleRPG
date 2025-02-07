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
    private VisualElement[] buttonUIs;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
    }
    private void Start()
    {
        var mainElement = root.Q<VisualElement>("ButtonParent");
        var menuParent = root.Q<VisualElement>("MenuParent");
        buttonUIs = new VisualElement[mainElement.childCount];
        for (int i = 0; i < mainElement.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = mainElement.ElementAt(localIndex);
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => ChangeUI(localIndex));
            switch (i)
            {
                case 0:
                    button.text = "����";
                    break;
                case 1:
                    button.text = "����";
                    break;
                case 2:
                    button.text = "��ų";
                    break;
            }
        }
        menuParent.Add(statUI.root);
        menuParent.Add(weaponUI.root);
        menuParent.Add(skillUI.root);
        menuParent.Add(weaponBookUI.root);
        //MenuControlUI�� ũ�⿡ ���缭 ũ�� ����
        statUI.root.ElementAt(0).style.height = Length.Percent(100);
        skillUI.root.ElementAt(0).style.height = Length.Percent(100);
    }

    private void ChangeUI(int i)
    {
        BattleBroker.OnMenuUIChange?.Invoke(i);
    }
}
