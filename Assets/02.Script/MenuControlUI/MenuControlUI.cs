using EnumCollection;
using Kamgam.UIToolkitBlurredBackground;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
public class MenuControlUI : MonoBehaviour, IGeneralUI
{
    //Menu UI
    [SerializeField] StatUI _statUI;
    [SerializeField] WeaponUI _weaponUI;
    [SerializeField] SkillUI _skillUI;
    [SerializeField] CompanionUI _companionUI;
    [SerializeField] AdventureUI _adventureUI;
    [SerializeField] StoreUI _storeUI;

    //MenuRelated
    [SerializeField] EquipedSkillUI _equipedSkillUI;
    [SerializeField] OfflineRewardReceive _offlineRewardReceive;
    [SerializeField] WeaponBookUI _weaponBookUI;
    [SerializeField] float[] _menuHeight;
    //Etc
    public VisualElement root { private set; get; }
    public UIDocument notice;
    private BlurredBackground blurredBackground;
    private Coroutine blurRoutine;
    private readonly NoticeDot[] _noticeDotArr = new NoticeDot[6];
    
    private VisualElement[] plankArr;
    private int currentIndex = -1;
    private VisualElement menuParent;

    private IMenuUI[] menuUiArr;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        VisualElement noticeParent = notice.rootVisualElement.Q<VisualElement>("Parent");
        blurredBackground = root.Q<BlurredBackground>("BlurredBackground");
        blurredBackground.BlurTint = new Color(0f,0f,0f,0f);
        UIBroker.ActiveBlurredBackground += ActiveBlurredBackground;

        menuUiArr = new IMenuUI[] { _statUI, _weaponUI, _skillUI, _companionUI, _adventureUI, _storeUI };

        for (int i = 0; i < 6; i++)
        {
            _noticeDotArr[i] = new NoticeDot(noticeParent.ElementAt(i), this);
        }
        plankArr = buttonParent.Children().Select(item=>item.Q<VisualElement>("PlankPanel")).ToArray();
        menuParent = root.Q<VisualElement>("MenuParent");
        for (int i = 0; i < buttonParent.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = buttonParent.ElementAt(localIndex);
            Label label = menuButton.Q<Label>();
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => UIBroker.ChangeMenu(localIndex));
            switch (i)
            {
                case 0:
                    label.text = "스탯";
                    break;
                case 1:
                    label.text = "무기";
                    break;
                case 2:
                    label.text = "스킬";
                    break;
                case 3:
                    label.text = "동료";
                    break;
                case 4:
                    label.text = "모험";
                    break;
                case 5:
                    label.text = "상점";
                    break;
            }
        }
        UIBroker.ChangeMenu += (index) => OnChangeMenu(index == currentIndex ? 0 : index);
        UIBroker.OnMenuUINotice += OnMenuUINotice;
    }

    private void Start()
    {
        var rootChild = root.Q<VisualElement>("MenuControlUI");
        var upperParent = root.Q<VisualElement>("UpperParent");
        upperParent.Add(_offlineRewardReceive.root);
        upperParent.Add(_equipedSkillUI.root);
        _equipedSkillUI.root.style.position = Position.Relative;
        _offlineRewardReceive.root.style.position = Position.Relative;
        menuParent.Add(_statUI.root);
        menuParent.Add(_weaponUI.root);
        menuParent.Add(_skillUI.root);
        //menuParent.Add(_weaponBookUI.root);
        menuParent.Add(_companionUI.root);
        menuParent.Add(_storeUI.root);
        menuParent.Add(_adventureUI.root);
        _statUI.root.ElementAt(0).style.height = Length.Percent(100);
        _skillUI.root.ElementAt(0).style.height = Length.Percent(100);
        _weaponUI.root.ElementAt(0).style.height = Length.Percent(100);
        _companionUI.root.ElementAt(0).style.height = Length.Percent(100);
        _storeUI.root.ElementAt(0).style.height = Length.Percent(100);
        UIBroker.ChangeMenu(0);

    }
    private void OnMenuUINotice(int dotIndex, bool isActive)
    {
        if (isActive)
            _noticeDotArr[dotIndex].StartNotice();
        else
            _noticeDotArr[dotIndex].StopNotice();
    }
    private void OnChangeMenu(int index)
    {
        if (BattleBroker.GetBattleType != null)
        {
            switch (BattleBroker.GetBattleType())
            {
                case BattleType.Boss:
                case BattleType.CompanionTech:
                    if (index == 3 || index == 4)
                    {
                        UIBroker.ShowPopUpInBattle("전투중에는 이용이 불가합니다");
                        return;
                    }
                    break;
            }
        }
        if (currentIndex != index)
        {
            if (currentIndex == -1)
            {
                menuUiArr[1].InactiveUI();
                menuUiArr[2].InactiveUI();
                menuUiArr[3].InactiveUI();
                menuUiArr[4].InactiveUI();
                menuUiArr[5].InactiveUI();
            }
            else
            {
                plankArr[currentIndex].style.bottom = 0f;
                _noticeDotArr[currentIndex].OnPositionSet(0, 0);
                menuUiArr[currentIndex].InactiveUI();
            }
            menuParent.style.height = _menuHeight[index];
            plankArr[index].style.bottom = 30f;
            _noticeDotArr[index].OnPositionSet(0, -30);
            menuUiArr[index].ActiveUI();
            currentIndex = index;
        }
    }
    private void ActiveBlurredBackground(bool active, float duration)
    {
        if (blurRoutine != null)
            StopCoroutine(blurRoutine);

        blurRoutine = StartCoroutine(FadeBlurTintCoroutine(active, duration));
    }

    private IEnumerator FadeBlurTintCoroutine(bool active, float duration)
    {
        Color baseColor = blurredBackground.BlurTint;
        float startAlpha = active ? 0f : 1f;
        float endAlpha = active ? 1f : 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            blurredBackground.BlurTint = newColor;
            yield return null;
        }
        blurredBackground.BlurTint = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);
    }
    public void OnBattle()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }

}