using System;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MenuChangeUI : MonoBehaviour
{
    //Controlled Componenet

    [SerializeField] StatUI statUI;
    [SerializeField] WeaponUI weaponUI;
    [SerializeField] SkillUI skillUI;
    [SerializeField] WeaponBookUI weaponBookUI;
    [SerializeField] CompanionUI companionUI;
    //[SerializeField] AdventureUI adventureUI;
    [SerializeField] StoreUI storeUI;
    //Etc
    public VisualElement root { private get; set; }
    [SerializeField] EquipedSkillUI equipedSkillUI;
    private readonly NoticeDot[] _noticeDotArr = new NoticeDot[6];
    private VisualElement[] buttonArr;
    private int currentIndex = -1;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        var rootChild = root.Q<VisualElement>("MenuControlUI");
        rootChild.Insert(0, equipedSkillUI.root);
        equipedSkillUI.root.style.position = Position.Relative;
        var mainElement = root.Q<VisualElement>("ButtonParent");
        var menuParent = root.Q<VisualElement>("MenuParent");
        buttonArr = new VisualElement[mainElement.childCount];
        UIBroker.OnMenuUINotice += OnMenuUINotice;
        for (int i = 0; i < mainElement.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = mainElement.ElementAt(localIndex);
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => ChangeUI(localIndex));
            _noticeDotArr[i] = new(menuButton, this);
            _noticeDotArr[i].SetParentToRoot();
            switch (i)
            {
                case 0:
                    button.text = "스탯";
                    break;
                case 1:
                    button.text = "무기";
                    break;
                case 2:
                    button.text = "스킬";
                    break;
                case 3:
                    button.text = "동료";
                    break;
                case 4:
                    button.text = "모험";
                    break;
                case 5:
                    button.text = "상점";
                    break;
            }
            buttonArr[i] = button;
        }
        ChangeUI(0);
        menuParent.Add(statUI.root);
        menuParent.Add(weaponUI.root);
        menuParent.Add(skillUI.root);
        menuParent.Add(weaponBookUI.root);
        menuParent.Add(companionUI.root);
        menuParent.Add(storeUI.root);
        //menuParent.Add(adventureUI.root);
        //MenuControlUI의 크기에 맞춰서 크기 세팅
        statUI.root.ElementAt(0).style.height = Length.Percent(100);
        skillUI.root.ElementAt(0).style.height = Length.Percent(100);
        companionUI.root.ElementAt(0).style.height = Length.Percent(100);
        storeUI.root.ElementAt(0).style.height = Length.Percent(100);
    }

    private void OnMenuUINotice(int menuIndex, bool isActive)
    {
        if (isActive)
        {
            _noticeDotArr[menuIndex].StartNotice();
        }
        else
        {
            _noticeDotArr[menuIndex].StopNotice();
        }
    }

    private void ChangeUI(int index)
    {
        if (currentIndex == index)
            return;


        UIBroker.OnMenuUIChange?.Invoke(index);

        buttonArr[index].style.top = -30f;
        if (currentIndex > -1)
        {
            _noticeDotArr[index].OnPositionChange(0, -30);
            buttonArr[currentIndex].style.top = 0f;
            _noticeDotArr[currentIndex].OnPositionChange(0, 30);
        }
        currentIndex = index;
    }
}