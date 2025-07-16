#if UNITY_EDITOR
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
    private VisualElement currentPanel;//현재 활성화 된 패널
    //활성화 가능한 패널들
    private VisualElement currencyPanel;
    private VisualElement statPanel;
    private VisualElement weaponPanel;
    private VisualElement skillPanel;
    private VisualElement companionPanel;
    private VisualElement materialPanel;
    //동적으로 할당할 데이터 패널
    private VisualTreeAsset dataPanel_0;
    private VisualTreeAsset dataPanel_1;
    private VisualTreeAsset separatePanel;
    //카테고리 열거형
    private enum Categori
    {
        Currency, Stat, Weapon, Skill,Companion, Material
    }
    // 반복 실행 변수
    private float nextActionTime;
    private string currentDataName;
    private bool currentIsPlus;
    private Label currentValueLabel;
    private Categori currentCategori;
    private bool currentIsBigInteger;
    private bool isPressingVe;
    //버튼이 자연스럽게 보이기 위함
    private bool isPressedChange;
    private VisualElement pressedVe;
    private Color pressedOriginColor;
    private bool isHoverOnPressed;
    //WinApi
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private readonly int VK_LBUTTON = 0x01;
    //Stat Dropdown Variable
    private ScrollView[] statScrollViewArr;
    //Weapon Dropdown Variable
    private string currentWeaponType;
    private string currentWeaponValue;
    private ScrollView[] weaponScrollViewArr;
    //Companion Dropdown Variable
    private string currentCompanionType;
    private int currentCompanionIndex;
    private ScrollView[] companionScrollViewArr;
    //Skill Dropdown Variable
    private ScrollView[] skillScrollViewArr;

    [MenuItem("Window/Total Debugger")]

    public static void ShowWindow()
    {
        // EditorWindow를 열고 제목 설정
        var window = GetWindow<TotalDebugger>("Total Debugger");
        window.minSize = new Vector2(670, 800); // 창의 최소 크기 설정
        window.position = new Rect(600, 100, 670, 800); // 창의 초기 위치와 크기 설정
    }
    public void CreateGUI()
    {
        dataPanel_0 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/DataPanel_0.uxml");
        dataPanel_1 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/DataPanel_1.uxml");
        separatePanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/SeparatePanel.uxml");
        // UXML 파일 로드
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
            Debug.LogError("게임 데이터 호출 실패");
            ToggleActiveUI(false);
            return;
        }
        _gameData = StartBroker.GetGameData();
        if (_gameData == null)
        {
            Debug.LogError("게임 데이터 호출 실패");
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
        if (!isPressingVe && pressedVe != null)//딱 땠을 경우
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
            NetworkBroker.SaveServerData();
        }
        if (!isPressingVe)//누르고 있지 않을 때
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
        float interval = 0.1f; // 0.1초 주기
        if (EditorApplication.timeSinceStartup >= nextActionTime)
        {
            // 저장된 매개변수를 이용하여 ValueChangeRoop 호출
            ValueChangeRoop(currentDataName, currentIsPlus, currentValueLabel, currentCategori);
            nextActionTime = (float)EditorApplication.timeSinceStartup + interval;
        }
    }

    private void CurrencyAtStart()
    {
        currencyPanel.style.display = DisplayStyle.Flex;
        statPanel.style.display = DisplayStyle.None;
        weaponPanel.style.display = DisplayStyle.None;
        skillPanel.style.display = DisplayStyle.None;
        companionPanel.style.display = DisplayStyle.None;
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
        Button companionButton = categoriPanel.Q<Button>("CompanionButton");
        Button materialButton = categoriPanel.Q<Button>("MaterialButton");
        currencyButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Currency));
        statusButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Stat));
        weaponButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Weapon));
        skillButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Skill));
        companionButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Companion));
        materialButton.RegisterCallback<ClickEvent>(evt => ChangeCategori(Categori.Material));
    }

    private void InitCategoriPanel()
    {
        currencyPanel = rootVisualElement.Q<VisualElement>("CurrencyCategoriPanel");
        statPanel = rootVisualElement.Q<VisualElement>("StatusCategoriPanel");
        weaponPanel = rootVisualElement.Q<VisualElement>("WeaponCategoriPanel");
        skillPanel = rootVisualElement.Q<VisualElement>("SkillCategoriPanel");
        companionPanel = rootVisualElement.Q<VisualElement>("CompanionCategoriPanel");
        materialPanel = rootVisualElement.Q<VisualElement>("MaterialCategoriPanel");
        InitCurrency();
        InitStat();
        InitWeapon();
        InitSkillData();
        InitCompanion();
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
        NetworkBroker.SaveServerData();
    }
    private void OnChangeButtonDown(string dataName, bool isPlus, Label valueLabel, Categori categori, bool isBigInteger)
    {
        nextActionTime = 0f;    // 즉시 동작하도록 초기화

        // 매개변수를 멤버 변수에 저장
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
            BigInteger bigValue = new(intValue);
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
            case Categori.Stat:
                StatCase();
                break;
            case Categori.Weapon:
                WeaponCase();
                break;
            case Categori.Skill:
                SkillCase();
                break;
            case Categori.Companion:
                CompanionCase();
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
                    int prevLevel = _gameData.level;
                    if (isSet)
                        _gameData.level = (int)value;
                    else
                        _gameData.level += (int)value;
                    _gameData.level = Mathf.Max(_gameData.level, 1);
                    BattleBroker.OnLevelExpSet();
                    int statPoint = _gameData.level - prevLevel;
                    if (statPoint > 0)
                    {
                        _gameData.statPoint += statPoint;
                        BattleBroker.OnStatPointSet?.Invoke();
                    }
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
                case "Clover":
                    if (isSet)
                        _gameData.clover = (int)value;
                    else
                        _gameData.clover += (int)value;
                    _gameData.clover = Mathf.Max(_gameData.clover, 0);
                    BattleBroker.OnCloverSet();
                    break;
                case "MaxStage":
                    if (isSet)
                        _gameData.maxStageNum = (int)value;
                    else
                        _gameData.maxStageNum += (int)value;
                    _gameData.maxStageNum = Mathf.Clamp(_gameData.maxStageNum, 0, 299);
                    BattleBroker.OnMaxStageSet();
                    break;
                case "Scroll":
                    if (isSet)
                        _gameData.scroll = (int)value;
                    else
                        _gameData.scroll += (int)value;
                    _gameData.scroll = Mathf.Max(_gameData.scroll, 0);
                    BattleBroker.OnScrollSet();
                    break;
                case "StatPoint":
                    if (isSet)
                        _gameData.statPoint = (int)value;
                    else
                        _gameData.statPoint += (int)value;
                    _gameData.statPoint = Mathf.Max(_gameData.statPoint, 0);
                    BattleBroker.OnStatPointSet();
                    break;
            }
        }
        void StatCase()
        {
            string[] splitted = dataName.Split("::");
            string currency = splitted[0];
            StatusType currentStatus = (StatusType)Enum.Parse(typeof(StatusType), splitted[1]);
            Dictionary<StatusType, int> tempDict = null;
            switch (currency)
            {
                case "Gold":
                    tempDict = _gameData.statLevel_Gold;
                    break;
                case "StatPoint":
                    tempDict = _gameData.statLevel_StatPoint;
                    break;
            }
            int tempValue;
            if (isSet)
                tempValue = (int)value;
            else
                tempValue = tempDict[currentStatus] + (int)value;
           tempDict[currentStatus] = Mathf.Max(0, tempValue);
            switch (currency)
            {
                case "Gold":
                    PlayerBroker.OnGoldStatusLevelSet?.Invoke(currentStatus, _gameData.statLevel_Gold[currentStatus]);
                    break;
                case "StatPoint":
                    PlayerBroker.OnStatPointStatusLevelSet?.Invoke(currentStatus, _gameData.statLevel_StatPoint[currentStatus]);
                    break;
            }
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
            bool isPlayerSkill = dataName.Contains("Player");
            int maxLevel = isPlayerSkill ? PriceManager.MAXPLAYERSKILLLEVEL : PriceManager.MAXCOMPANIONSKILLLEVEL;
            _gameData.skillLevel[dataName] = Mathf.Clamp(_gameData.skillLevel[dataName], 0, maxLevel);
            PlayerBroker.OnSkillLevelSet?.Invoke(dataName, _gameData.skillLevel[dataName]);
            if (!isPlayerSkill)
            {
                for (int i = 0; i < CompanionManager.instance.companionArr.Length; i++)
                {
                    CompanionController companion = CompanionManager.instance.companionArr[i];
                    foreach (SkillData skill in companion.companionStatus.companionSkillArr)
                    {
                        if (skill.uid == dataName)
                        {
                            BattleBroker.OnCompanionExpSet?.Invoke(i);
                            return;
                        }
                    }
                }
                
            }
        }
        void CompanionCase()
        {
            int intValue = (int)value;
            int techIndex = int.Parse(dataName.Split('_')[1]);
            if (isSet)
                _gameData.companionPromoteTech[currentCompanionIndex][techIndex] = intValue;
            else
                _gameData.companionPromoteTech[currentCompanionIndex][techIndex] += intValue;
            _gameData.companionPromoteTech[currentCompanionIndex][techIndex] = Math.Clamp(_gameData.companionPromoteTech[currentCompanionIndex][techIndex], 0, 3);
            PlayerBroker.OnCompanionPromoteTechSet?.Invoke(currentCompanionIndex, _gameData.companionPromoteTech[currentCompanionIndex][techIndex], techIndex );
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

    private void ChangeCategori(Categori categori)
    {
        VisualElement tempCurrent = null;
        switch (categori)
        {
            case Categori.Currency:
                tempCurrent = currencyPanel;
                break;
            case Categori.Stat:
                tempCurrent = statPanel;
                break;
            case Categori.Weapon:
                tempCurrent = weaponPanel;
                break;
            case Categori.Skill:
                tempCurrent = skillPanel;
                break;
            case Categori.Companion:
                tempCurrent = companionPanel;
                break;
            case Categori.Material:
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
        VisualElement cloverPanel = currencyPanel.Q<VisualElement>("CloverPanel");
        VisualElement scrollPanel = currencyPanel.Q<VisualElement>("ScrollPanel");
        VisualElement maxStagePanel = currencyPanel.Q<VisualElement>("MaxStagePanel");
        VisualElement statPointPanel = currencyPanel.Q<VisualElement>("StatPointPanel");
        //SetDataPanel
        SetDataPanel(levelPanel, "Level", "Level", _gameData.level.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(goldPanel, "Gold", "Gold", _gameData.gold.ToString(), Categori.Currency, true, 120f, 33f);
        SetDataPanel(diaPanel, "Dia", "Dia", _gameData.dia.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(cloverPanel, "Clover", "Clover", _gameData.clover.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(scrollPanel, "Scroll", "Scroll", _gameData.scroll.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(maxStagePanel, "MaxStage", "Max Stage", _gameData.maxStageNum.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(statPointPanel, "StatPoint", "Stat Point", _gameData.statPoint.ToString(), Categori.Currency, false, 120f, 33f);
        //데이터 변경된 이후 Label Set
        BattleBroker.OnLevelExpSet += () => { levelPanel.Q<Label>("ValueLabel").text = _gameData.level.ToString(); };
        BattleBroker.OnGoldSet += () => { goldPanel.Q<Label>("ValueLabel").text = _gameData.gold.ToString(); };
        BattleBroker.OnDiaSet += () => { diaPanel.Q<Label>("ValueLabel").text = _gameData.dia.ToString(); };
        BattleBroker.OnCloverSet += () => { cloverPanel.Q<Label>("ValueLabel").text = _gameData.clover.ToString(); };
        BattleBroker.OnScrollSet += () => { scrollPanel.Q<Label>("ValueLabel").text = _gameData.scroll.ToString(); };
        BattleBroker.OnMaxStageSet += () => { maxStagePanel.Q<Label>("ValueLabel").text = _gameData.maxStageNum.ToString(); };
        BattleBroker.OnStatPointSet += () => { statPointPanel.Q<Label>("ValueLabel").text = _gameData.statPoint.ToString(); };

    }
    private void InitStat()
    {
        VisualElement scrollViewParent = statPanel.Q<VisualElement>("ScrollViewParent");
        statScrollViewArr = scrollViewParent.Children().Select(item => (ScrollView)item).ToArray();
        statScrollViewArr[0].style.display = DisplayStyle.Flex;
        statScrollViewArr[1].style.display = DisplayStyle.None;
        InitGoldStat();
        InitStatPointStat();
        //Dropdown
        DropdownField typeDropdown = statPanel.Q<DropdownField>("TypeDropdown");
        typeDropdown.choices = new() { "Gold", "StatPoint" };
        typeDropdown.value = typeDropdown.choices[0];
        typeDropdown.RegisterValueChangedCallback(evt => OnStatDropdownChange(evt.newValue));
    }

    private void InitGoldStat()
    {
        ScrollView scrollView = statScrollViewArr[0];
        scrollView.Add(CreateSeparatePanel("Gold Stat"));

        // 필요한 StatusType만 리스트로 정의 (필요한 경우 필터링 가능)
        StatusType[] goldStatTypes = new StatusType[]
        {
        StatusType.Power,
        StatusType.MaxHp,
        StatusType.HpRecover,
        StatusType.Critical,
        StatusType.CriticalDamage
        };

        foreach (StatusType type in goldStatTypes)
        {
            TemplateContainer dataPanel = dataPanel_0.CloneTree();
            scrollView.Add(dataPanel);

            // _gameData.statLevel_Gold에서 값이 없으면 0을 기본값으로 설정
            if (!_gameData.statLevel_Gold.ContainsKey(type))
            {
                _gameData.statLevel_Gold[type] = 0;
            }

            // SetDataPanel 실행
            SetDataPanel(
                dataPanel,
                $"Gold::{type}",
                type.ToString(),
                _gameData.statLevel_Gold[type].ToString(),
                Categori.Stat,
                false,
                120f,
                25f
            );

            // 값 변경 시 업데이트 처리
            PlayerBroker.OnGoldStatusLevelSet += (changedType, level) =>
            {
                if (changedType == type)
                    dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_Gold.ContainsKey(type) ? _gameData.statLevel_Gold[type].ToString() : "0";
            };
        }
    }
    private void InitStatPointStat()
    {
        ScrollView scrollView = statScrollViewArr[1];
        scrollView.Add(CreateSeparatePanel("StatPoint Stat"));

        // 필요한 StatusType만 리스트로 정의
        StatusType[] statPointStatTypes = new StatusType[]
        {
        StatusType.Power,
        StatusType.MaxHp,
        StatusType.HpRecover,
        StatusType.CriticalDamage,
        StatusType.GoldAscend
        };

        foreach (StatusType type in statPointStatTypes)
        {
            TemplateContainer dataPanel = dataPanel_0.CloneTree();
            scrollView.Add(dataPanel);

            // _gameData.statLevel_StatPoint에서 값이 없으면 0을 기본값으로 설정
            if (!_gameData.statLevel_StatPoint.ContainsKey(type))
            {
                _gameData.statLevel_StatPoint[type] = 0;
            }

            // SetDataPanel 실행
            SetDataPanel(
                dataPanel,
                $"StatPoint::{type}",
                type.ToString().Replace("MaxHp", "Max Hp").Replace("HpRecover", "Hp Recover").Replace("CriticalDamage", "Critical Damage").Replace("GoldAscend", "Gold Ascend"),
                _gameData.statLevel_StatPoint[type].ToString(),
                Categori.Stat,
                false,
                120f,
                25f
            );

            // 값 변경 시 업데이트 처리
            PlayerBroker.OnStatPointStatusLevelSet += (changedType, level) =>
            {
                if (changedType == type)
                    dataPanel.Q<Label>("ValueLabel").text = _gameData.statLevel_StatPoint.ContainsKey(type) ? _gameData.statLevel_StatPoint[type].ToString() : "0";
            };
        }
    }


    private void InitWeapon()
    {
        VisualElement scrollViewParent = weaponPanel.Q<VisualElement>("ScrollViewParent");
        weaponScrollViewArr = scrollViewParent.Children().Select(item => (ScrollView)item).ToArray();

        //Dropdown
        DropdownField typeDropdown = weaponPanel.Q<DropdownField>("TypeDropdown");
        DropdownField valueDropdown = weaponPanel.Q<DropdownField>("ValueDropdown");
        typeDropdown.choices = new() { "Player", "Companion" };
        valueDropdown.choices = new() { "Level", "Count" };
        typeDropdown.value = typeDropdown.choices[0];
        valueDropdown.value = valueDropdown.choices[0];
        currentWeaponType = "Player";
        currentWeaponValue = "Level";

        //ScrollView
        typeDropdown.RegisterValueChangedCallback(evt => OnWeaponDropdownChange(evt.newValue, true));
        valueDropdown.RegisterValueChangedCallback(evt => OnWeaponDropdownChange(evt.newValue, false));
        
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
    }
    void InitEachWeapon(bool isPlayer/*orCompanion*/, bool isLevel/*orCount*/, int scrollViewIndex)
    {
        List<WeaponData> playerDict = WeaponManager.instance.GetWeaponDataByRole(true);
        List<WeaponData> companionDict = WeaponManager.instance.GetWeaponDataByRole(false);
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
    private void OnWeaponDropdownChange(string newValue, bool isType)
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
        skillScrollViewArr[0].Add(CreateSeparatePanel("Player Skill"));
        SkillData[] playerArr = SkillManager.instance.playerSkillArr;
        InitEachSkill(playerArr, skillScrollViewArr[0]);
        skillScrollViewArr[1].Add(CreateSeparatePanel("Companion Skill"));
        var companionArr = SkillManager.instance.companionSkillArr;
        InitEachSkill(companionArr, skillScrollViewArr[1]);
        skillScrollViewArr[0].style.display = DisplayStyle.Flex;
        skillScrollViewArr[1].style.display = DisplayStyle.None;
        //Dropdown
        DropdownField typeDropdown = skillPanel.Q<DropdownField>("TypeDropdown");
        typeDropdown.choices = new() { "Player", "Companion" };
        typeDropdown.value = typeDropdown.choices[0];
        typeDropdown.RegisterValueChangedCallback(evt => OnSkillDropdownChange(evt.newValue));
    }

    void InitEachSkill(SkillData[] skillDataArr, ScrollView scrollView)
    {
        for (int i = 0; i < skillDataArr.Length; i++)
        {
            SkillData x = skillDataArr[i];
            string uid = x.uid;
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
    private void OnSkillDropdownChange(string newValue)
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
    private void OnStatDropdownChange(string newValue)
    {
        if (newValue == "Gold")
        {
            statScrollViewArr[0].style.display = DisplayStyle.Flex;
            statScrollViewArr[1].style.display = DisplayStyle.None;
        }
        else if (newValue == "StatPoint")
        {
            statScrollViewArr[0].style.display = DisplayStyle.None;
            statScrollViewArr[1].style.display = DisplayStyle.Flex;
        }
    }
    private void InitCompanion()
    {
        VisualElement scrollViewParent = companionPanel.Q<VisualElement>("ScrollViewParent");
        companionScrollViewArr = scrollViewParent.Children().Select(item => (ScrollView)item).ToArray();

        //Dropdown
        DropdownField typeDropdown = companionPanel.Q<DropdownField>("TypeDropdown");
        DropdownField companionDropdown = companionPanel.Q<DropdownField>("CompanionDropdown");
        typeDropdown.choices = new() { "PromoteEffect", "PromoteTech" };
        companionDropdown.choices = new() { "Companion_0", "Companion_1", "Companion_2" };
        typeDropdown.value = typeDropdown.choices[0];
        companionDropdown.value = companionDropdown.choices[0];
        currentCompanionType = "PromoteEffect";
        currentCompanionIndex = 0;

        //ScrollView
        typeDropdown.RegisterValueChangedCallback(evt => OnCompanionDropdownChange(evt.newValue, true));
        companionDropdown.RegisterValueChangedCallback(evt => OnCompanionDropdownChange(evt.newValue, false));
        InitCompanion_PromoteEffect(0, 0);
        InitCompanion_PromoteEffect(1, 1);
        InitCompanion_PromoteEffect(2, 2);
        InitCompanion_PromoteTech(0, 3);
        InitCompanion_PromoteTech(1, 4);
        InitCompanion_PromoteTech(2, 5);
        for (int i = 0; i < companionScrollViewArr.Length; i++)
        {
            if (i == 0)
            {
                companionScrollViewArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                companionScrollViewArr[i].style.display = DisplayStyle.None;
            }
        }
    }

    private void InitCompanion_PromoteEffect(int companionIndex, int scrollViewIndex)
    {
        VisualElement separatePanel = CreateSeparatePanel($"Companion_{companionIndex}'s Promote Effect");
        companionScrollViewArr[scrollViewIndex].Add(separatePanel);
        Dictionary<int, (StatusType, Rarity)> effect = _gameData.companionPromoteEffect[companionIndex];
        for (int i = 0; i < 5; i++)//Effect Index Roop
        {
            TemplateContainer panel_1Template = dataPanel_1.CloneTree();
            companionScrollViewArr[scrollViewIndex].Add(panel_1Template);
            panel_1Template.Q<Label>("DataLabel").text = $"Effect_{i}";
            DropdownField statDropdown = panel_1Template.Q<DropdownField>("Dropdown_0");
            DropdownField rarityDropdown = panel_1Template.Q<DropdownField>("Dropdown_1");
            int effectIndex = i;
            //StatDropdown

            statDropdown.label = "Stat";
            List<string> statList = new() {"None", "Power", "CriticalDamage", "MaxHp", "HpRecover", "MaxMp", "MpRecover", "GoldAscend", "Resist", "Penetration", "ExpAscend"};
            statDropdown.choices = statList;
            

            //RarityDropdown
            rarityDropdown.label = "Rarity";
            List<string> rarityList = new(Enum.GetNames(typeof(Rarity)));
            rarityList.Insert(0, "None");
            rarityDropdown.choices = rarityList;

            if (effect.TryGetValue(i, out (StatusType, Rarity) tuple))
            {
                statDropdown.value = tuple.Item1.ToString();
                rarityDropdown.value = tuple.Item2.ToString();
            }
            else
            {
                statDropdown.value = statDropdown.choices[0];
                rarityDropdown.value = rarityDropdown.choices[0];
            }
            PlayerBroker.OnCompanionPromoteEffectSet += (index_0, index_1, value) =>
            {
                if (value!=null&& (companionIndex == index_0) && (effectIndex == index_1))
                {
                    statDropdown.value = value.Value.Item1.ToString();
                    rarityDropdown.value = value.Value.Item2.ToString();
                }
            };
            statDropdown.RegisterValueChangedCallback(evt => OnCompanionPanelDropdown_PromoteEffect(statDropdown.value, rarityDropdown.value, effectIndex));
            rarityDropdown.RegisterValueChangedCallback(evt => OnCompanionPanelDropdown_PromoteEffect(statDropdown.value, rarityDropdown.value, effectIndex));
        }
    }

    private void OnCompanionPanelDropdown_PromoteEffect(string statStr, string rarityStr, int effectIndex)
    {
        if (Enum.TryParse(statStr, out StatusType statusType) && Enum.TryParse(rarityStr, out Rarity rarity))
        {
            PlayerBroker.OnCompanionPromoteEffectSet(currentCompanionIndex, effectIndex, new(statusType, rarity));
            _gameData.companionPromoteEffect[currentCompanionIndex][effectIndex] = new(statusType, rarity);
        }
        else
        {
            PlayerBroker.OnCompanionPromoteEffectSet( currentCompanionIndex, effectIndex, null);
            _gameData.companionPromoteEffect[currentCompanionIndex].Remove(effectIndex);
        }
        
        NetworkBroker.SaveServerData();
    }

    private void InitCompanion_PromoteTech(int companionIndex, int scrollViewIndex)
    {
        VisualElement separatePanel = CreateSeparatePanel($"Companion_{companionIndex}'s Promote Tech");
        companionScrollViewArr[scrollViewIndex].Add(separatePanel);
        for (int i = 0; i < 2; i++)
        {
            int techIndex = i;
            TemplateContainer panel_0Template = dataPanel_0.CloneTree();
            companionScrollViewArr[scrollViewIndex].Add(panel_0Template);
            panel_0Template.Q<Label>("DataLabel").text = $"Tech_{i}";
            SetDataPanel(panel_0Template, $"Tech_{i}", $"Tech_{i}", _gameData.companionPromoteTech[companionIndex][i].ToString(), Categori.Companion, false, 120f, 33f);
            PlayerBroker.OnCompanionPromoteTechSet += (companionArg, techIndex_0, techIndex_1) =>
            {
                if (companionArg == companionIndex && techIndex_1 == techIndex)
                    panel_0Template.Q<Label>("ValueLabel").text = _gameData.companionPromoteTech[companionArg][techIndex_1].ToString();
            };
        }
    }

    private void OnCompanionDropdownChange(string newValue, bool isType)
    {
        if (isType)
        {
            currentCompanionType = newValue;
        }
        else
        {
            currentCompanionIndex = int.Parse( newValue.Split('_')[1]);
        }
        int newIndex = -1;
        if (currentCompanionType == "PromoteEffect")
        {
            switch (currentCompanionIndex)
            {
                case 0:
                    newIndex = 0;
                    break;
                case 1:
                    newIndex = 1;
                    break;
                case 2:
                    newIndex = 2;
                    break;
            }
        }
        else if (currentCompanionType == "PromoteTech")
        {
            switch (currentCompanionIndex)
            {
                case 0:
                    newIndex = 3;
                    break;
                case 1:
                    newIndex = 4;
                    break;
                case 2:
                    newIndex = 5;
                    break;
            }
        }
        for (int i = 0; i < companionScrollViewArr.Length; i++)
        {
            if (i == newIndex)
            {
                companionScrollViewArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                companionScrollViewArr[i].style.display = DisplayStyle.None;
            }
        }
    }

    private void InitMaterial()
    {
        Rarity[] rarityArr = (Rarity[])Enum.GetValues(typeof(Rarity));
        foreach (Rarity rarity in rarityArr)
        {
            // str을 Rarity enum으로 변환
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
    private void SetDataPanel(VisualElement panel, string dataId, string displayName, string valueStr, Categori categori, bool isBigInteger, float valueLabelWidth = 60f, float fontSize = 18f)//스킬의 경우를 기본 값으로 보면 됨
    {
        Label valueLabel = panel.Q<Label>("ValueLabel");
        valueLabel.style.width = valueLabelWidth;
        Label dataLabel = panel.Q<Label>("DataLabel");
        VisualElement setVe = panel.Q<VisualElement>("SetVe");
        VisualElement plusVe = panel.Q<VisualElement>("PlusVe");
        VisualElement minusVe = panel.Q<VisualElement>("MinusVe");
        SetHoverEventAsButton(setVe, false);
        SetHoverEventAsButton(plusVe, true);
        SetHoverEventAsButton(minusVe, true);
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
    private void SetHoverEventAsButton(VisualElement ve, bool _isChangeVe)
    {
        // 원본 배경색 저장
        Color originalColor = ve.resolvedStyle.backgroundColor;
        Color hoverColor = new(originalColor.r * 0.8f, originalColor.g * 0.8f, originalColor.b * 0.8f, 1f);
        Color clickColor = new(originalColor.r * 0.6f, originalColor.g * 0.6f, originalColor.b * 0.6f, 1f);
        // MouseEnterEvent: 배경색을 약간 어둡게 설정
        ve.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (ve == pressedVe)//누르고 있는 Ve에 hover할 경우
            {
                isHoverOnPressed = true;
            }
            else
            {
                ve.style.backgroundColor = hoverColor;

            }
        });

        // MouseLeaveEvent: 원래 색상 복원
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

        // MouseDownEvent: 배경색을 더 어둡게 설정
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
        if (GetAsyncKeyState(VK_LBUTTON) == 0) // 버튼이 눌리지 않은 상태
        {
            isPressingVe = false;
        }
    }
    private void OnEnable()
    {
        // 플레이 모드 상태 변경 감지 이벤트 등록
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        // 이벤트 해제 (메모리 누수 방지)
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
#endif