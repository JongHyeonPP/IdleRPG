using EnumCollection;
using Kamgam.UIToolkitBlurredBackground;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class MenuControlUI : MonoBehaviour, IGeneralUI
{
    [Header("Menu UI")]
    [SerializeField] StatUI _statUI;
    [SerializeField] WeaponUI _weaponUI;
    [SerializeField] SkillUI _skillUI;
    [SerializeField] CompanionUI _companionUI;
    [SerializeField] AdventureUI _adventureUI;
    [SerializeField] StoreUI _storeUI;

    [Header("Menu Related")]
    [SerializeField] EquipedSkillUI _equipedSkillUI;
    [SerializeField] OfflineRewardReceive _offlineRewardReceive;
    [SerializeField] WeaponBookUI _weaponBookUI;
    [SerializeField] float[] _menuHeight;

    [Header("Camera Control")]
    [SerializeField] CameraInfo expandInfo;
    [SerializeField] CameraInfo shrinkInfo;
    [SerializeField] Camera mainCamera;

    [Header("Etc")]
    public UIDocument notice;
    public VisualElement root { private set; get; }

    private BlurredBackground blurredBackground;
    private Coroutine blurRoutine;
    private readonly NoticeDot[] _noticeDotArr = new NoticeDot[6];
    private VisualElement[] plankArr;
    private int currentIndex = -1;
    private VisualElement menuParent;
    private IMenuUI[] menuUiArr;

    [Serializable]
    public class CameraInfo
    {
        public Vector3 position;
        public float orthographicSize;
    }

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        VisualElement noticeParent = notice.rootVisualElement.Q<VisualElement>("Parent");
        blurredBackground = root.Q<BlurredBackground>("BlurredBackground");
        blurredBackground.BlurTint = new Color(0f, 0f, 0f, 0f);

        UIBroker.ActiveBlurredBackground += ActiveBlurredBackground;
        UIBroker.ChangeMenu += (index) => OnChangeMenu(index == currentIndex ? 0 : index);
        UIBroker.OnMenuUINotice += OnMenuUINotice;

        menuUiArr = new IMenuUI[] { _statUI, _weaponUI, _skillUI, _companionUI, _adventureUI, _storeUI };

        for (int i = 0; i < 6; i++)
            _noticeDotArr[i] = new NoticeDot(noticeParent.ElementAt(i), this);

        plankArr = buttonParent.Children().Select(item => item.Q<VisualElement>("PlankPanel")).ToArray();
        menuParent = root.Q<VisualElement>("MenuParent");

        for (int i = 0; i < buttonParent.childCount; i++)
        {
            int localIndex = i;
            VisualElement menuButton = buttonParent.ElementAt(localIndex);
            Label label = menuButton.Q<Label>();
            Button button = menuButton.Q<Button>();
            button.RegisterCallback<ClickEvent>(evt => UIBroker.ChangeMenu(localIndex));

            label.text = i switch
            {
                0 => "스탯",
                1 => "무기",
                2 => "스킬",
                3 => "동료",
                4 => "모험",
                5 => "상점",
                _ => "???"
            };
        }
    }

    private void Start()
    {
        var upperParent = root.Q<VisualElement>("UpperParent");
        upperParent.Add(_offlineRewardReceive.root);
        upperParent.Add(_equipedSkillUI.root);
        _equipedSkillUI.root.style.position = Position.Relative;
        _offlineRewardReceive.root.style.position = Position.Relative;

        menuParent.Add(_statUI.root);
        menuParent.Add(_weaponUI.root);
        menuParent.Add(_skillUI.root);
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
        // 전투 중 진입 제한
        if (BattleBroker.GetBattleType != null)
        {
            switch (BattleBroker.GetBattleType())
            {
                case BattleType.Boss:
                case BattleType.CompanionTech:
                case BattleType.Adventure:
                    if (index == 3 || index == 4) // 동료 or 모험
                    {
                        UIBroker.ShowPopUpInBattle("전투중에는 이용이 불가합니다");
                        return;
                    }
                    break;
            }
        }

        // 카메라 변경도 여기서 처리
        UpdateCameraSize(index);

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

    private void UpdateCameraSize(int index)
    {
        // 전투 중이면 카메라 조정 금지
        if (BattleBroker.GetBattleType != null)
        {
            switch (BattleBroker.GetBattleType())
            {
                case BattleType.Boss:
                case BattleType.CompanionTech:
                case BattleType.Adventure:
                    return;
            }
        }

        CameraInfo targetInfo = (index == 0 || index == 1 || index == 2) ? shrinkInfo : expandInfo;

        mainCamera.transform.position = targetInfo.position;
        mainCamera.orthographicSize = targetInfo.orthographicSize;
        UIBroker.SetPlayerBarPosition();
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

    public void OnBattle() => root.style.display = DisplayStyle.Flex;
    public void OnStory() => root.style.display = DisplayStyle.None;
    public void OnBoss() { }
}
