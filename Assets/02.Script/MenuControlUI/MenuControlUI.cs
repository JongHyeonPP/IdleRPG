using EnumCollection;
using System;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MenuControlUI : MonoBehaviour
{
    //Controlled Componenet
    [SerializeField] AutoRewardReceive _autoRewardReceive;
    [SerializeField] EquipedSkillUI _equipedSkillUI;
    [SerializeField] StatUI _statUI;
    [SerializeField] WeaponUI _weaponUI;
    [SerializeField] SkillUI _skillUI;
    [SerializeField] WeaponBookUI _weaponBookUI;
    [SerializeField] CompanionUI _companionUI;

    //[SerializeField] AdventureUI adventureUI;
   // [SerializeField] StoreUI _storeUI;
    [SerializeField] float[] _menuHeight;
    //Etc
    public VisualElement root { private set; get; }
    public UIDocument notice;
    private readonly NoticeDot[] _noticeDotArr = new NoticeDot[6];
    
    private VisualElement[] buttonArr;
    private int currentIndex = -1;
    private VisualElement menuParent;
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
        menuParent = root.Q<VisualElement>("MenuParent");
        for (int i = 0; i < mainElement.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = mainElement.ElementAt(localIndex);
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => OnButtonClick(localIndex));
            UIBroker.OnMenuUIChange += ChangeUI;
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

    private static void OnButtonClick(int index)
    {
        if (BattleBroker.GetBattleType != null)
        {
            switch (BattleBroker.GetBattleType())
            {
                case BattleType.Boss:
                case BattleType.CompanionTech:
                    if (index == 3 || index == 4)
                    {
                        UIBroker.ShowInable();
                        return;
                    }
                    break;
            }

        }
        UIBroker.OnMenuUIChange(index);
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
        rootChild.Insert(0, _equipedSkillUI.root);
        rootChild.Insert(0, _autoRewardReceive.root);
        _equipedSkillUI.root.style.position = Position.Relative;
        _autoRewardReceive.root.style.position = Position.Relative;
        menuParent.Add(_statUI.root);
        menuParent.Add(_weaponUI.root);
        menuParent.Add(_skillUI.root);
        menuParent.Add(_weaponBookUI.root);
        menuParent.Add(_companionUI.root);
     //   menuParent.Add(_storeUI.root);
        //menuParent.Add(adventureUI.root);
        _statUI.root.ElementAt(0).style.height = Length.Percent(100);
        _skillUI.root.ElementAt(0).style.height = Length.Percent(100);
        _weaponUI.root.ElementAt(0).style.height = Length.Percent(100);
        _companionUI.root.ElementAt(0).style.height = Length.Percent(100);
       // _storeUI.root.ElementAt(0).style.height = Length.Percent(100);
        ChangeUI(0);
    }

    private void ChangeUI(int index)
    {
        if (currentIndex != index)
        {
            if (currentIndex > -1)
            {
                buttonArr[currentIndex].style.top = 0f;
                _noticeDotArr[currentIndex].OnPositionSet(0, 0);
            }
            menuParent.style.height = _menuHeight[index];
            buttonArr[index].style.top = -30f;
            _noticeDotArr[index].OnPositionSet(0, -30);
            
            currentIndex = index;
        }
    }
}