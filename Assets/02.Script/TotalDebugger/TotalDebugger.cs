using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Runtime.InteropServices;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
public class TotalDebugger : EditorWindow
{
    private GameData _gameData;
    private VisualElement currentPanel;//���� Ȱ��ȭ �� �г�
    //Ȱ��ȭ ������ �гε�
    private VisualElement currencyPanel;
    private VisualElement statusPanel;
    private VisualElement weaponPanel;
    private VisualElement skillPanel;
    private VisualElement materialPanel;
    //�������� �Ҵ��� ������ �г�
    private VisualTreeAsset dataPanel_0;
    private VisualTreeAsset separatePanel;
    //ī�װ� ������
    private enum Categori
    {
        Currency, Status, Weapon, Skill, Material
    }
    // �ݺ� ���� ����
    private float nextActionTime;
    private string currentDataName;
    private bool currentIsPlus;
    private Label currentValueLabel;
    private Categori currentCategori;
    private bool currentIsBigInteger;
    private bool isPressingVe;
    //��ư�� �ڿ������� ���̱� ����
    private bool isPressedChange;
    private VisualElement pressedVe;
    private Color pressedOriginColor;
    private bool isHoverOnPressed;
    //WinApi
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private readonly int VK_LBUTTON = 0x01;
    //Weapon DropDown Variable
    private string currentWeaponType;
    private string currentWeaponValue;
    private ScrollView[] weaponScrollViewArr;
    //Skill DropDown Variable
    private string currentSkillType;
    private ScrollView[] skillScrollViewArr;

    [MenuItem("Window/Total Debugger")]

    public static void ShowWindow()
    {
        // EditorWindow�� ���� ���� ����
        var window = GetWindow<TotalDebugger>("Total Debugger");
        window.minSize = new Vector2(600, 800); // â�� �ּ� ũ�� ����
        window.position = new Rect(600, 100, 600, 800); // â�� �ʱ� ��ġ�� ũ�� ����
    }
    public void CreateGUI()
    {
        dataPanel_0 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/DataPanel_0.uxml");
        separatePanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/SeparatePanel.uxml");
        // UXML ���� �ε�
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/TotalDebugger.uxml");

        if (visualTree != null)
        {
            visualTree.CloneTree(rootVisualElement);
        }
        else
        {
            Debug.LogWarning("UXML file not found. Ensure it exists at the correct path.");
        }
        if (StartBroker.GetGameData == null)
        {
            Debug.LogError("���� ������ ȣ�� ����");
            ToggleActiveUI(false);
            return;
        }
        _gameData = StartBroker.GetGameData();
        if (_gameData == null)
        {
            Debug.LogError("���� ������ ȣ�� ����");
            ToggleActiveUI(false);
            return;
        }
        ToggleActiveUI(true);
        InitCategoriPanel();
        InitCategoriButtonEvent();
        CurrencyAtStart();
    }
    private void Update()
    {
        if (!isPressingVe && pressedVe != null)//�� ���� ���
        {
            if (isHoverOnPressed)
            {
                pressedVe.style.backgroundColor = new Color(pressedOriginColor.r * 0.8f, pressedOriginColor.g * 0.8f, pressedOriginColor.b * 0.8f, 1f);
            }
            else
            {
                pressedVe.style.backgroundColor = pressedOriginColor;
            }
            isPressingVe = false;
            pressedVe = null;
            StartBroker.SaveLocal();
        }
        if (!isPressingVe)//������ ���� ���� ��
            return;
        else
        {
            if (isPressedChange)
                ChangeEvent();
            MouseUpEventByWinApi();
        }
    }

    private void ChangeEvent()
    {
        float interval = 0.1f; // 0.1�� �ֱ�
        if (EditorApplication.timeSinceStartup >= nextActionTime)
        {
            // ����� �Ű������� �̿��Ͽ� ValueChangeRoop ȣ��
            ValueChangeRoop(currentDataName, currentIsPlus, currentValueLabel, currentCategori);
            nextActionTime = (float)EditorApplication.timeSinceStartup + interval;
        }
    }

    private void CurrencyAtStart()
    {
        currencyPanel.style.display = DisplayStyle.Flex;
        statusPanel.style.display = DisplayStyle.None;
        weaponPanel.style.display = DisplayStyle.None;
        skillPanel.style.display = DisplayStyle.None;
        materialPanel.style.display = DisplayStyle.None;
        currentPanel = currencyPanel;
    }
    private void InitCategoriButtonEvent()
    {
        VisualElement categoriPanel = rootVisualElement.Q<VisualElement>("CategoriPanel");
        Button currencyButton = categoriPanel.Q<Button>("CurrencyButton");
        Button statusButton = categoriPanel.Q<Button>("StatusButton");
        Button weaponButton = categoriPanel.Q<Button>("WeaponButton");
        Button skillButton = categoriPanel.Q<Button>("SkillButton");
        Button materialButton = categoriPanel.Q<Button>("MaterialButton");
        currencyButton.RegisterCallback<ClickEvent>(evt => ChangeCategori("Currency"));
        statusButton.RegisterCallback<ClickEvent>(evt => ChangeCategori("Status"));
        weaponButton.RegisterCallback<ClickEvent>(evt => ChangeCategori("Weapon"));
        skillButton.RegisterCallback<ClickEvent>(evt => ChangeCategori("Skill"));
        materialButton.RegisterCallback<ClickEvent>(evt => ChangeCategori("Material"));
    }

    private void InitCategoriPanel()
    {
        currencyPanel = rootVisualElement.Q<VisualElement>("CurrencyCategoriPanel");
        statusPanel = rootVisualElement.Q<VisualElement>("StatusCategoriPanel");
        weaponPanel = rootVisualElement.Q<VisualElement>("WeaponCategoriPanel");
        skillPanel = rootVisualElement.Q<VisualElement>("SkillCategoriPanel");
        materialPanel = rootVisualElement.Q<VisualElement>("MaterialCategoriPanel");
        InitCurrency();
        InitStatus();
        InitWeapon();
        InitSkillData();
        InitMaterial();
    }

    private void ToggleActiveUI(bool isActive)
    {
        VisualElement loadedPanel = rootVisualElement.Q<VisualElement>("LoadedPanel");
        VisualElement unloadedNotify = rootVisualElement.Q<VisualElement>("UnloadedNotify");
        if (isActive)
        {
            loadedPanel.style.display = DisplayStyle.Flex;
            unloadedNotify.style.display = DisplayStyle.None;
        }
        else
        {
            loadedPanel.style.display = DisplayStyle.None;
            unloadedNotify.style.display = DisplayStyle.Flex;
        }
    }

    private void OnSetButtonClick(string dataName, TextField textField, Label valueLabel, Categori categori, bool isBiginteger = false)
    {
        if (textField.value == string.Empty)
            return;
        try
        {
            if (isBiginteger)
            {
                ValueEvent(dataName, BigInteger.Parse(textField.value), valueLabel, true, categori);
            }
            else
            {
                ValueEvent(dataName, int.Parse(textField.value), valueLabel, true, categori);
            }

        }
        catch
        {
            Debug.LogError("Invalid Input");
        }
        textField.value = string.Empty;
        StartBroker.SaveLocal();
    }
    private void OnChangeButtonDown(string dataName, bool isPlus, Label valueLabel, Categori categori, bool isBigInteger)
    {
        nextActionTime = 0f;    // ��� �����ϵ��� �ʱ�ȭ

        // �Ű������� ��� ������ ����
        currentDataName = dataName;
        currentIsPlus = isPlus;
        currentValueLabel = valueLabel;
        currentCategori = categori;
        currentIsBigInteger = isBigInteger;
    }
    private void ValueChangeRoop(string dataName, bool isPlus, Label valueLabel, Categori categori)
    {
        int intValue = isPlus ? 1 : -1;
        if (currentIsBigInteger)
        {
            BigInteger bigValue = new BigInteger(intValue);
            ValueEvent(dataName, bigValue, valueLabel, false, categori);
        }
        else
            ValueEvent(dataName, intValue, valueLabel, false, categori);
    }
    private void ValueEvent(string dataName, IComparable value, Label valueLabel, bool isSet/*Or Change*/, Categori categori)
    {
        switch (categori)
        {
            case Categori.Currency:
                CurrencyCase();
                break;
            case Categori.Status:
                StatusCase();
                break;
            case Categori.Weapon:
                WeaponCase();
                break;
            case Categori.Skill:
                SkillCase();
                break;
            case Categori.Material:
                MaterialCase();
                break;
        }

        void CurrencyCase()
        {
            switch (dataName)
            {
                case "Level":
                    if (isSet)
                        _gameData.level = (int)value;
                    else
                        _gameData.level += (int)value;
                    _gameData.level = Mathf.Max(_gameData.level, 1);
                    BattleBroker.OnLevelExpSet();
                    break;
                case "Gold":
                    if (isSet)
                        _gameData.gold = (BigInteger)value;
                    else
                        _gameData.gold += (BigInteger)value;
                    _gameData.gold = BigInteger.Max(_gameData.gold, 0);
                    BattleBroker.OnGoldSet();
                    break;
                case "Dia":
                    if (isSet)
                        _gameData.dia = (int)value;
                    else
                        _gameData.dia += (int)value;
                    _gameData.dia = Mathf.Max(_gameData.dia, 0);
                    BattleBroker.OnDiaSet();
                    break;
                case "Emerald":
                    if (isSet)
                        _gameData.emerald = (int)value;
                    else
                        _gameData.emerald += (int)value;
                    _gameData.emerald = Mathf.Max(_gameData.emerald, 0);
                    BattleBroker.OnEmeraldSet();
                    break;
                case "MaxStage":
                    if (isSet)
                        _gameData.maxStageNum = (int)value;
                    else
                        _gameData.maxStageNum += (int)value;
                    _gameData.maxStageNum = Mathf.Clamp(_gameData.maxStageNum, 0, 299);
                    BattleBroker.OnMaxStageSet();
                    break;
            }
        }
        void StatusCase()
        {
            StatusType currentStatus = (StatusType)Enum.Parse(typeof(StatusType), dataName);
            int tempValue;
            if (isSet)
                tempValue = (int)value;
            else
                tempValue = _gameData.statLevel_Gold[currentStatus] + (int)value;
            _gameData.statLevel_Gold[currentStatus] = Mathf.Max(0, tempValue);

            PlayerBroker.OnStatusLevelSet?.Invoke(currentStatus, _gameData.statLevel_Gold[currentStatus]);
        }
        void WeaponCase()
        {
            string[] splitted = dataName.Split("::");
            string uid = splitted[0];
            string who = splitted[1];
            string what = splitted[2];
            Dictionary<string, int> weaponDict = null;
            if (what == "Level")
            {
                weaponDict = _gameData.weaponLevel;
            }
            else if (what == "Count")
            {
                weaponDict = _gameData.weaponCount;
            }
            int tempValue;
            if (isSet)
                tempValue = (int)value;
            else
            {
                if (!weaponDict.ContainsKey(uid))
                    weaponDict.Add(uid, 0);
                tempValue = weaponDict[uid] + (int)value;
            }

            tempValue = Mathf.Max(0, tempValue);
            if (what == "Level")
                tempValue = Mathf.Min(tempValue, PriceManager.MAXWEAPONLEVEL);
            weaponDict[uid] = tempValue;
            var weaponData = WeaponManager.instance.weaponDict[uid];

            if (what == "Level")
            {
                PlayerBroker.OnWeaponLevelSet?.Invoke(weaponData.UID, weaponDict[uid]);
            }
            else
            {
                PlayerBroker.OnWeaponCountSet?.Invoke(weaponData.UID, weaponDict[uid]);
            }
        }
        void SkillCase()
        {
            int intValue = (int)value;
            if (!_gameData.skillLevel.ContainsKey(dataName))
            {
                _gameData.skillLevel[dataName] = 0;
            }
            if (isSet)
                _gameData.skillLevel[dataName] = intValue;
            else
                _gameData.skillLevel[dataName] += intValue;
            _gameData.skillLevel[dataName] = Mathf.Clamp(_gameData.skillLevel[dataName], 0, PriceManager.MAXSKILLLEVEL);
            PlayerBroker.OnSkillLevelSet?.Invoke(dataName, intValue);
        }
        void MaterialCase()
        {
            int intValue = (int)value;
            if (!Enum.TryParse(dataName, out Rarity rarity))
            {
                return;
            }
            if (!_gameData.skillFragment.ContainsKey(rarity))
            {
                _gameData.skillFragment[rarity] = 0;
            }
            if (isSet)
                _gameData.skillFragment[rarity] = intValue;
            else
                _gameData.skillFragment[rarity] += intValue;
            _gameData.skillFragment[rarity] = Mathf.Max(_gameData.skillFragment[rarity], 0);

            PlayerBroker.OnFragmentSet?.Invoke(rarity, _gameData.skillFragment[rarity]);
        }
    }

    private void ChangeCategori(string categoriStr)
    {
        VisualElement tempCurrent = null;
        switch (categoriStr)
        {
            case "Currency":
                tempCurrent = currencyPanel;
                break;
            case "Status":
                tempCurrent = statusPanel;
                break;
            case "Weapon":
                tempCurrent = weaponPanel;
                break;
            case "Skill":
                tempCurrent = skillPanel;
                break;
            case "Material":
                tempCurrent = materialPanel;
                break;
        }
        if (currentPanel == tempCurrent)
            return;
        if (!(currentPanel == null))
            currentPanel.style.display = DisplayStyle.None;
        currentPanel = tempCurrent;
        currentPanel.style.display = DisplayStyle.Flex;
    }
    private void InitCurrency()
    {
        //Panel Set
        VisualElement levelPanel = currencyPanel.Q<VisualElement>("LevelPanel");
        VisualElement goldPanel = currencyPanel.Q<VisualElement>("GoldPanel");
        VisualElement diaPanel = currencyPanel.Q<VisualElement>("DiaPanel");
        VisualElement emeraldPanel = currencyPanel.Q<VisualElement>("EmeraldPanel");
        VisualElement maxStagePanel = currencyPanel.Q<VisualElement>("MaxStagePanel");
        //SetDataPanel
        SetDataPanel(levelPanel, "Level", "Level", _gameData.level.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(goldPanel, "Gold", "Gold", _gameData.gold.ToString(), Categori.Currency, true, 120f, 33f);
        SetDataPanel(diaPanel, "Dia", "Dia", _gameData.dia.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(emeraldPanel, "Emerald", "Emerald", _gameData.emerald.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(maxStagePanel, "MaxStage", "MaxStage", _gameData.maxStageNum.ToString(), Categori.Currency, false, 120f, 33f);
        //������ ����� ���� Label Set
        BattleBroker.OnLevelExpSet += () => { levelPanel.Q<Label>("ValueLabel").text = _gameData.level.ToString(); };
        BattleBroker.OnGoldSet += () => { goldPanel.Q<Label>("ValueLabel").text = _gameData.gold.ToString(); };
        BattleBroker.OnDiaSet += () => { diaPanel.Q<Label>("ValueLabel").text = _gameData.dia.ToString(); };
        BattleBroker.OnEmeraldSet += () => { emeraldPanel.Q<Label>("ValueLabel").text = _gameData.emerald.ToString(); };
        BattleBroker.OnMaxStageSet += () => { maxStagePanel.Q<Label>("ValueLabel").text = _gameData.maxStageNum.ToString(); };

    }
    private void InitStatus()
    {
        ScrollView scrollView = statusPanel.Q<ScrollView>();
        for (int i = 0; i < 5; i++)
        {
            TemplateContainer dataPanel = dataPanel_0.CloneTree();
            scrollView.Add(dataPanel);
            switch (i)
            {
                case 0://Power
                    SetDataPanel(dataPanel, "Power", "Power", _gameData.statLevel_Gold[StatusType.Power].ToString(), Categori.Status, false, 120f, 33f);
                    PlayerBroker.OnStatusLevelSet += (type, level) =>
                    {
                        if (type == StatusType.Power)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold[StatusType.Power].ToString();
                    };
                    break;
                case 1://MaxHp
                    SetDataPanel(dataPanel, "MaxHp", "MaxHp", _gameData.statLevel_Gold[StatusType.MaxHp].ToString(), Categori.Status, false, 120f, 33f);
                    PlayerBroker.OnStatusLevelSet += (type, level) =>
                    {
                        if (type == StatusType.MaxHp)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold[StatusType.MaxHp].ToString();
                    };
                    break;
                case 2://HpRecover
                    SetDataPanel(dataPanel, "HpRecover", "HpRecover", _gameData.statLevel_Gold[StatusType.HpRecover].ToString(), Categori.Status, false, 120f, 33f);
                    PlayerBroker.OnStatusLevelSet += (type, level) =>
                    {
                        if (type == StatusType.HpRecover)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold[StatusType.HpRecover].ToString();
                    };
                    break;
                case 3://Critical
                    SetDataPanel(dataPanel, "Critical", "Critical", _gameData.statLevel_Gold[StatusType.Critical].ToString(), Categori.Status, false, 120f, 33f);
                    PlayerBroker.OnStatusLevelSet += (type, level) =>
                    {
                        if (type == StatusType.Critical)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold[StatusType.Critical].ToString();
                    };
                    break;
                case 4://CriticalDamage
                    SetDataPanel(dataPanel, "CriticalDamage", "CriticalDamage", _gameData.statLevel_Gold[StatusType.CriticalDamage].ToString(), Categori.Status, false, 120f, 25f);
                    PlayerBroker.OnStatusLevelSet += (type, level) =>
                    {
                        if (type == StatusType.CriticalDamage)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold[StatusType.CriticalDamage].ToString();
                    };
                    break;
            }
        }

    }
    private void InitWeapon()
    {
        VisualElement scrollViewParent = weaponPanel.Q<VisualElement>("ScrollViewParent");
        weaponScrollViewArr = scrollViewParent.Children().Select(item => (ScrollView)item).ToArray();

        //DropDown
        DropdownField typeDropDown = weaponPanel.Q<DropdownField>("TypeDropDown");
        DropdownField valueDropDown = weaponPanel.Q<DropdownField>("ValueDropDown");
        typeDropDown.choices = new() { "Player", "Companion" };
        valueDropDown.choices = new() { "Level", "Count" };
        typeDropDown.value = typeDropDown.choices[0];
        valueDropDown.value = valueDropDown.choices[0];
        currentWeaponType = "Player";
        currentWeaponValue = "Level";

        //ScrollView
        typeDropDown.RegisterValueChangedCallback(evt => OnWeaponDropDownChange(evt.newValue, true));
        valueDropDown.RegisterValueChangedCallback(evt => OnWeaponDropDownChange(evt.newValue, false));
        List<WeaponData> playerDict = WeaponManager.instance.GetWeaponDataByRole(true);
        List<WeaponData> companionDict = WeaponManager.instance.GetWeaponDataByRole(false);
        InitEachWeapon(true, true, 0);//player's level
        InitEachWeapon(false, true, 1);//companion's level
        InitEachWeapon(true, false, 2);//player's count
        InitEachWeapon(false, false, 3);//companion's count
        for (int i = 0; i < weaponScrollViewArr.Length; i++)
        {
            if (i == 0)
            {
                weaponScrollViewArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                weaponScrollViewArr[i].style.display = DisplayStyle.None;
            }
        }
        void InitEachWeapon(bool isPlayer/*orCompanion*/, bool isLevel/*orCount*/, int scrollViewIndex)
        {
            string who = isPlayer ? "Player" : "Companion";
            string what = isLevel ? "Level" : "Count";
            List<WeaponData> targetDict = isPlayer ? playerDict : companionDict;
            VisualElement separatePanel = CreateSeparatePanel($"{who}'s Weapon {what}");
            weaponScrollViewArr[scrollViewIndex].Add(separatePanel);
            Dictionary<string, int> targetDataDict = isLevel ? _gameData.weaponLevel : _gameData.weaponCount;

            foreach (var weaponData in targetDict)
            {
                string uid = weaponData.UID;
                TemplateContainer dataPanel = dataPanel_0.CloneTree();
                weaponScrollViewArr[scrollViewIndex].Add(dataPanel);
                if (!targetDataDict.TryGetValue(uid, out int value))
                {
                    value = 0;
                }
                SetDataPanel(dataPanel, $"{uid}::{who}::{what}", uid.ToString(), value.ToString(), Categori.Weapon, false);
                if (isLevel)
                    PlayerBroker.OnWeaponLevelSet += (settedId, level) =>
                    {
                        if (uid == settedId)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.weaponLevel[uid].ToString();
                    };
                else
                    PlayerBroker.OnWeaponCountSet += (settedId, Count) =>
                    {
                        if (uid == settedId)
                            dataPanel.Q<Label>("ValueLabel").text = _gameData.weaponCount[uid].ToString();
                    };
            }
        }
    }

    private void OnWeaponDropDownChange(string newValue, bool isType)
    {
        if (isType)
        {
            currentWeaponType = newValue;
        }
        else
        {
            currentWeaponValue = newValue;
        }
        int newIndex = -1;
        if (currentWeaponType == "Player")
        {
            if (currentWeaponValue == "Level")
            {
                newIndex = 0;
            }
            else if (currentWeaponValue == "Count")
            {
                newIndex = 2;
            }
        }
        else if (currentWeaponType == "Companion")
        {
            if (currentWeaponValue == "Level")
            {
                newIndex = 1;
            }
            else if (currentWeaponValue == "Count")
            {
                newIndex = 3;
            }
        }
        for (int i = 0; i < weaponScrollViewArr.Length; i++)
        {
            if (i == newIndex)
            {
                weaponScrollViewArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                weaponScrollViewArr[i].style.display = DisplayStyle.None;
            }
        }
    }
    private void InitSkillData()
    {
        VisualElement scrollViewParent = skillPanel.Q<VisualElement>("ScrollViewParent");
        skillScrollViewArr = scrollViewParent.Children().Select(item => (ScrollView)item).ToArray();
        Dictionary<string, SkillData> skillDict = SkillManager.instance.GetSkillDict();
        skillScrollViewArr[0].Add(CreateSeparatePanel("Player Skill"));
        var playerValue = skillDict.Where(item => item.Value.isPlayerSkill).Select(item => item.Value.uid);
        InitEachSkill(playerValue, skillScrollViewArr[0]);
        skillScrollViewArr[1].Add(CreateSeparatePanel("Companion Skill"));
        var companionValue = skillDict.Where(item => !item.Value.isPlayerSkill).Select(item => item.Value.uid);
        InitEachSkill(companionValue, skillScrollViewArr[1]);
        skillScrollViewArr[0].style.display = DisplayStyle.Flex;
        skillScrollViewArr[1].style.display = DisplayStyle.None;
        currentSkillType = "Player";
        //DropDown
        DropdownField typeDropDown = skillPanel.Q<DropdownField>("TypeDropDown");
        typeDropDown.choices = new() { "Player", "Companion" };
        typeDropDown.value = typeDropDown.choices[0];
        currentSkillType = "Player";
        typeDropDown.RegisterValueChangedCallback(evt => OnSkillDropDownChange(evt.newValue));
    }
    void InitEachSkill(IEnumerable<string> uids, ScrollView scrollView)
    {
        foreach (var uid in uids)
        {
            TemplateContainer skillDataPanel = dataPanel_0.CloneTree();
            if (!_gameData.skillLevel.TryGetValue(uid, out int level))
            {
                level = 0;
            }
            scrollView.Add(skillDataPanel);
            SetDataPanel(skillDataPanel, uid, uid, level.ToString(), Categori.Skill, false);
            PlayerBroker.OnSkillLevelSet += (settedId, level) =>
            {
                if (settedId == uid)
                    skillDataPanel.Q<Label>("ValueLabel").text = _gameData.skillLevel[uid].ToString();
            };
        }
    }
    private void OnSkillDropDownChange(string newValue)
    {
        if (newValue == "Player")
        {
            skillScrollViewArr[0].style.display = DisplayStyle.Flex;
            skillScrollViewArr[1].style.display = DisplayStyle.None;
        }
        else if (newValue == "Companion")
        {
            skillScrollViewArr[0].style.display = DisplayStyle.None;
            skillScrollViewArr[1].style.display = DisplayStyle.Flex;
        }
    }
    private void InitMaterial()
    {
        Rarity[] rarityArr = (Rarity[])Enum.GetValues(typeof(Rarity));
        foreach (Rarity rarity in rarityArr)
        {
            // str�� Rarity enum���� ��ȯ
            if (!_gameData.skillFragment.TryGetValue(rarity, out int value))
            {
                value = 0;
            }
            VisualElement panel = materialPanel.Q<VisualElement>($"{rarity}FragmentPanel");
            SetDataPanel(panel, $"{rarity}", $"{rarity}", value.ToString(), Categori.Material, false);
            PlayerBroker.OnFragmentSet += (settedRarity, count) =>
            {
                if (settedRarity == rarity)
                    panel.Q<Label>("ValueLabel").text = _gameData.skillFragment[rarity].ToString();
            };
        }
        Label fragmentLabel = materialPanel.Q<VisualElement>("FragmentSeparatePanel").Q<Label>();
        fragmentLabel.text = "Fragment";
    }
    private void SetDataPanel(VisualElement panel, string dataId, string displayName, string valueStr, Categori categori, bool isBigInteger, float valueLabelWidth = 60f, float fontSize = 18f)//��ų�� ��츦 �⺻ ������ ���� ��
    {
        Label valueLabel = panel.Q<Label>("ValueLabel");
        valueLabel.style.width = valueLabelWidth;
        Label dataLabel = panel.Q<Label>("DataLabel");
        VisualElement setVe = panel.Q<VisualElement>("SetVe");
        VisualElement plusVe = panel.Q<VisualElement>("PlusVe");
        VisualElement minusVe = panel.Q<VisualElement>("MinusVe");
        SetHoverEventLikeButton(setVe, false);
        SetHoverEventLikeButton(plusVe, true);
        SetHoverEventLikeButton(minusVe, true);
        TextField textField = panel.Q<TextField>();
        VisualElement buttonPanel = panel.Q<VisualElement>("ButtonPanel");
        dataLabel.text = displayName;
        dataLabel.style.fontSize = fontSize;
        setVe.RegisterCallback<ClickEvent>(evt => OnSetButtonClick(dataId, textField, valueLabel, categori, isBigInteger));
        plusVe.RegisterCallback<PointerDownEvent>(evt => OnChangeButtonDown(dataId, true, valueLabel, categori, isBigInteger));
        minusVe.RegisterCallback<PointerDownEvent>(evt => OnChangeButtonDown(dataId, false, valueLabel, categori, isBigInteger));
        if (StartBroker.GetGameData == null)
            return;
        valueLabel.text = valueStr;
    }






    private VisualElement CreateSeparatePanel(string labelStr)
    {
        VisualElement result = separatePanel.CloneTree();
        result.Q<Label>().text = labelStr;
        return result;
    }
    private void SetHoverEventLikeButton(VisualElement ve, bool _isChangeVe)
    {
        // ���� ���� ����
        Color originalColor = ve.resolvedStyle.backgroundColor;
        Color hoverColor = new(originalColor.r * 0.8f, originalColor.g * 0.8f, originalColor.b * 0.8f, 1f);
        Color clickColor = new(originalColor.r * 0.6f, originalColor.g * 0.6f, originalColor.b * 0.6f, 1f);
        // MouseEnterEvent: ������ �ణ ��Ӱ� ����
        ve.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (ve == pressedVe)//������ �ִ� Ve�� hover�� ���
            {
                isHoverOnPressed = true;
            }
            else
            {
                ve.style.backgroundColor = hoverColor;

            }
        });

        // MouseLeaveEvent: ���� ���� ����
        ve.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            if (ve == pressedVe)
            {
                isHoverOnPressed = false;
            }
            else
            {
                ve.style.backgroundColor = new StyleColor(originalColor);
            }
        });

        // MouseDownEvent: ������ �� ��Ӱ� ����
        ve.RegisterCallback<MouseDownEvent>(evt =>
        {
            ve.style.backgroundColor = new StyleColor(clickColor);
            pressedOriginColor = originalColor;
            isPressingVe = true;
            pressedVe = ve;
            isPressedChange = _isChangeVe;
        });
    }
    private void MouseUpEventByWinApi()
    {
        if (GetAsyncKeyState(VK_LBUTTON) == 0) // ��ư�� ������ ���� ����
        {
            isPressingVe = false;
        }
    }
    private void OnEnable()
    {
        // �÷��� ��� ���� ���� ���� �̺�Ʈ ���
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        // �̺�Ʈ ���� (�޸� ���� ����)
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ToggleActiveUI(false);
        }
    }
}