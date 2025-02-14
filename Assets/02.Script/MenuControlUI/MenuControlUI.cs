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
    [SerializeField] CompanionUI companionUI;
    //[SerializeField] AdventureUI adventureUI;
    //Etc
    [SerializeField] EquipedSkillUI equipedSkillUI;
    private VisualElement[] buttonArr;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        var rootChild = root.Q<VisualElement>("MenuControlUI");
        rootChild.Insert(0,equipedSkillUI.root);
        equipedSkillUI.root.style.position = Position.Relative;
        var mainElement = root.Q<VisualElement>("ButtonParent");
        var menuParent = root.Q<VisualElement>("MenuParent");
        buttonArr = new VisualElement[mainElement.childCount];
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
                case 3:
                    button.text = "����";
                    break;
                case 4:
                    button.text = "����";
                    break;
                case 5:
                    button.text = "����";
                    break;
            }
            buttonArr[i] = menuButton;
        }
        ChangeUI(0);
        menuParent.Add(statUI.root);
        menuParent.Add(weaponUI.root);
        menuParent.Add(skillUI.root);
        menuParent.Add(weaponBookUI.root);
        menuParent.Add(companionUI.root);
        //menuParent.Add(adventureUI.root);
        //MenuControlUI�� ũ�⿡ ���缭 ũ�� ����
        statUI.root.ElementAt(0).style.height = Length.Percent(100);
        skillUI.root.ElementAt(0).style.height = Length.Percent(100);
        companionUI.root.ElementAt(0).style.height = Length.Percent(100);
    }
    private void ChangeUI(int index)
    {
        UIBroker.OnMenuUIChange?.Invoke(index);
        for (int i = 0; i < buttonArr.Length; i++)
        {
            if (i == index)
                buttonArr[i].style.top = -30;
            else
                buttonArr[i].style.top = 0;
        }
    }
}