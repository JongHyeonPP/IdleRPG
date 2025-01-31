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
using UnityEditor.Rendering.LookDev;
using GooglePlayGames.BasicApi;
public class TotalDebugger : EditorWindow
{
    private GameData _gameData;
    private VisualElement currentPanel;//현재 활성화 된 패널
    //활성화 가능한 패널들
    private VisualElement currencyPanel;
    private VisualElement statusPanel;
    private VisualElement weaponPanel;
    private VisualElement skillPanel;
    private VisualElement materialPanel;
    //동적으로 할당할 데이터 패널
    private VisualTreeAsset dataPanel_0;
    private VisualTreeAsset separatePanel;
    //카테고리 열거형
    private enum Categori
    {
        Currency, Status, Weapon, Skill, Material
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
    //Weapon DropDown Variable
    private string currentWeaponType;
    private string currentWeaponValue;
    private ScrollView[] weaponScrollViewArr;

    [MenuItem("Window/Total Debugger")]

    public static void ShowWindow()
    {
        // EditorWindow를 열고 제목 설정
        var window = GetWindow<TotalDebugger>("Total Debugger");
        window.minSize = new Vector2(600, 800); // 창의 최소 크기 설정
        window.position = new Rect(600, 100, 600, 800); // 창의 초기 위치와 크기 설정
    }
    public void CreateGUI()
    {
        dataPanel_0 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/TotalDebugger/DataPanel_0.uxml");
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
            StartBroker.SaveLocal();
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
        InitSkill();
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
            BigInteger bigValue = new BigInteger(intValue);
            ValueEvent(dataName, bigValue, valueLabel, false, categori);
        }
        else
            ValueEvent(dataName, intValue, valueLabel, false, categori);
    }
    private void ValueEvent(string dataName, IComparable value, Label valueLabel, bool isSet/*Or Change*/, Categori categori)
    {
        string result;
        switch (categori)
        {
            case Categori.Currency:
                result = CurrencyCase();
                break;
            case Categori.Status:
                result = StatusCase();
                break;
            case Categori.Weapon:
                result = WeaponCase();
                break;
            case Categori.Skill:
                result = SkillCase();
                break;
            case Categori.Material:
                result = MaterialCase();
                break;
            default:
                result = string.Empty;
                break;
        }
        valueLabel.text = result;

        string CurrencyCase()
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
                    return _gameData.level.ToString();
                case "Gold":
                    if (isSet)
                        _gameData.gold = (BigInteger)value;
                    else
                        _gameData.gold += (BigInteger)value;
                    _gameData.gold = BigInteger.Max(_gameData.gold, 0);
                    BattleBroker.OnGoldSet();
                    return _gameData.gold.ToString();
                case "Dia":
                    if (isSet)
                        GameManager.instance.dia = (int)value;
                    else
                        GameManager.instance.dia += (int)value;
                    GameManager.instance.dia = Mathf.Max(GameManager.instance.dia, 0);
                    BattleBroker.OnDiaSet();
                    return GameManager.instance.dia.ToString();
                case "Emerald":
                    if (isSet)
                        GameManager.instance.emerald = (int)value;
                    else
                        GameManager.instance.emerald += (int)value;
                    GameManager.instance.emerald = Mathf.Max(GameManager.instance.emerald, 0);
                    BattleBroker.OnEmeraldSet();
                    return GameManager.instance.emerald.ToString();
                case "MaxStage":
                    if (isSet)
                        _gameData.maxStageNum = (int)value;
                    else
                        _gameData.maxStageNum += (int)value;
                    _gameData.maxStageNum = Mathf.Clamp(_gameData.maxStageNum, 0, 299);
                    return _gameData.maxStageNum.ToString();
                default:
                    return string.Empty;
            }
        }
        string StatusCase()
        {
            StatusType currentStatus = (StatusType)Enum.Parse(typeof(StatusType),dataName);
            int tempValue;
            if (isSet)
                tempValue = (int)value;
            else
                tempValue = _gameData.statLevel_Gold[currentStatus] + (int)value;
            _gameData.statLevel_Gold[currentStatus] = Mathf.Max(0, tempValue);

            PlayerBroker.OnStatusLevelSet?.Invoke(currentStatus, _gameData.statLevel_Gold[currentStatus]);
            return _gameData.statLevel_Gold[currentStatus].ToString();
        }
        string WeaponCase()
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
                PlayerBroker.OnWeaponLevelSet?.Invoke(weaponData, weaponDict[uid]);
            }
            else
            {
                PlayerBroker.OnWeaponCountSet?.Invoke(weaponData, weaponDict[uid]);
            }
            return weaponDict[uid].ToString();
        }
        string SkillCase()
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
            BattleBroker.OnSkillLevelSet?.Invoke(dataName, intValue);
            return _gameData.skillLevel[dataName].ToString();
        }
        string MaterialCase()
        {
            int intValue = (int)value;
            if (!Enum.TryParse(dataName, out Rarity rarity))
            {
                return String.Empty;
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
            
            BattleBroker.OnFragmentSet?.Invoke(rarity, _gameData.skillFragment[rarity]);
            return _gameData.skillFragment[rarity].ToString();
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
        VisualElement levelPanel = currencyPanel.Q<VisualElement>("LevelPanel");
        VisualElement goldPanel = currencyPanel.Q<VisualElement>("GoldPanel");
        VisualElement diaPanel = currencyPanel.Q<VisualElement>("DiaPanel");
        VisualElement emeraldPanel = currencyPanel.Q<VisualElement>("EmeraldPanel");
        VisualElement maxStagePanel = currencyPanel.Q<VisualElement>("MaxStagePanel");
        SetDataPanel(levelPanel, "Level","Level", _gameData.level.ToString(), Categori.Currency, false, 120f,33f);
        SetDataPanel(goldPanel, "Gold","Gold", _gameData.gold.ToString(), Categori.Currency, true, 120f, 33f);
        SetDataPanel(diaPanel, "Dia","Dia", GameManager.instance.dia.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(emeraldPanel, "Emerald","Emerald", GameManager.instance.emerald.ToString(), Categori.Currency, false, 120f, 33f);
        SetDataPanel(maxStagePanel, "MaxStage","MaxStage", _gameData.maxStageNum.ToString(), Categori.Currency, false, 120f, 33f);
        BattleBroker.OnGoldSet += () => { goldPanel.Q<Label>("ValueLabel").text = _gameData.gold.ToString(); };
        BattleBroker.OnLevelExpSet += () => { goldPanel.Q<Label>("ValueLabel").text = _gameData.level.ToString(); };
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
                    break;
                case 1://MaxHp
                    SetDataPanel(dataPanel, "MaxHp","MaxHp", _gameData.statLevel_Gold[StatusType.MaxHp].ToString(), Categori.Status, false, 120f, 33f);
                    break;
                case 2://HpRecover
                    SetDataPanel(dataPanel, "HpRecover","HpRecover", _gameData.statLevel_Gold[StatusType.HpRecover].ToString(), Categori.Status, false, 120f, 33f);
                    break;
                case 3://Critical
                    SetDataPanel(dataPanel, "Critical","Critical", _gameData.statLevel_Gold[StatusType.Critical].ToString(), Categori.Status, false, 120f, 33f);
                    break;
                case 4://CriticalDamage
                    SetDataPanel(dataPanel, "CriticalDamage","CriticalDamage", _gameData.statLevel_Gold[StatusType.CriticalDamage].ToString(), Categori.Status, false, 120f, 25f);
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
        typeDropDown.choices = new() {"Player","Companion" };
        valueDropDown.choices = new() {"Level","Count" };
        typeDropDown.value = typeDropDown.choices[0];
        valueDropDown.value = valueDropDown.choices[0];
        currentWeaponType = "Player";
        currentWeaponValue = "Level";

        //ScrollView
        typeDropDown.RegisterValueChangedCallback(evt=>OnWeaponDropDownChange(evt.newValue, true));
        valueDropDown.RegisterValueChangedCallback(evt=>OnWeaponDropDownChange(evt.newValue, false));
        List<WeaponData> playerDict = WeaponManager.instance.GetWeaponDataByRole(true);
        List<WeaponData> companionDict = WeaponManager.instance.GetWeaponDataByRole(false);
        InitEachWeapon(true, true,0);//player's level
        InitEachWeapon(false, true,1);//companion's level
        InitEachWeapon(true, false,2);//player's count
        InitEachWeapon(false, false,3);//companion's count
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
            Dictionary<string, int> targetDataDict = isLevel? _gameData.weaponLevel:_gameData.weaponCount;
            
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

    private void SetDataPanel(VisualElement panel, string dataId, string displayName, string valueStr, Categori categori, bool isBigInteger,float valueLabelWidth = 60f, float fontSize = 18f)//스킬의 경우를 기본 값으로 보면 됨
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
    private void InitSkill()
    {
        Dictionary<string, SkillData> skillDict = SkillManager.instance.GetSkillDict();
        ScrollView scrollView = skillPanel.Q<ScrollView>("ScrollView");
        scrollView.Add(CreateSeparatePanel("Player Skill"));
        var playerKvp = skillDict.Where(item => item.Value.isPlayerSkill);
        InitEachSkill(playerKvp, scrollView);
        scrollView.Add(CreateSeparatePanel("Party Skill"));
        var partyKvp = skillDict.Where(item => !item.Value.isPlayerSkill);
        InitEachSkill(partyKvp, scrollView);

    }

    void InitEachSkill(IEnumerable<KeyValuePair<string, SkillData>> kvps, ScrollView scrollView)
    {
        foreach (KeyValuePair<string, SkillData> kvp in kvps)
        {
            TemplateContainer skillDataPanel = dataPanel_0.CloneTree();
            if (!_gameData.skillLevel.TryGetValue(kvp.Key, out int level))
            {
                level = 0;
            }
            scrollView.Add(skillDataPanel);
            SetDataPanel(skillDataPanel, kvp.Key,kvp.Key, level.ToString(), Categori.Skill, false);
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
        }
        Label fragmentLabel = materialPanel.Q<VisualElement>("FragmentSeparatePanel").Q<Label>();
        fragmentLabel.text = "Fragment";
    }
    private VisualElement CreateSeparatePanel(string labelStr)
    {
        VisualElement result = separatePanel.CloneTree();
        result.Q<Label>().text = labelStr;
        return result;
    }
    private void SetHoverEventLikeButton(VisualElement ve, bool _isChangeVe)
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