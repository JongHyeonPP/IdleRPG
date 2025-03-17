using System;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MenuControlUI : MonoBehaviour
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
    public VisualElement root { private set; get; }
    public UIDocument notice;
    private readonly NoticeDot[] _noticeDotArr = new NoticeDot[6];
    [SerializeField] EquipedSkillUI equipedSkillUI;
    private VisualElement[] buttonArr;
    private int currentIndex = -1;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var mainElement = root.Q<VisualElement>("ButtonParent");
        var noticeParent = notice.rootVisualElement.Q<VisualElement>("Parent");
        for (int i = 0; i < 6; i++)
        {
            _noticeDotArr[i] = new NoticeDot(noticeParent.ElementAt(i), this);
        }
        buttonArr = new VisualElement[mainElement.childCount];
        for (int i = 0; i < mainElement.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = mainElement.ElementAt(localIndex);
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => ChangeUI(localIndex));
            UIBroker.OnMenuUINotice += OnMenuUINotice;
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
        

    }

    private void OnMenuUINotice(int dotIndex, bool isActive)
    {
        if (isActive)
            _noticeDotArr[dotIndex].StartNotice();
        else
            _noticeDotArr[dotIndex].StopNotice();
    }

    private void Start()
    {
        var rootChild = root.Q<VisualElement>("MenuControlUI");
        rootChild.Insert(0, equipedSkillUI.root);
        equipedSkillUI.root.style.position = Position.Relative;
        var menuParent = root.Q<VisualElement>("MenuParent");
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
        ChangeUI(0);
    }

    private void ChangeUI(int index)
    {
        if (currentIndex == index)
            return;
        UIBroker.OnMenuUIChange?.Invoke(index);
        buttonArr[index].style.top = -30f;
        if (currentIndex > -1)
        {
            _noticeDotArr[index].OnPositionSet(0, -30);
            buttonArr[currentIndex].style.top = 0f;
            _noticeDotArr[currentIndex].OnPositionSet(0, 0);
        }
        currentIndex = index;
    }
}