using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StoreUI : MonoBehaviour, IMenuUI
{
    // UI 루트
    public VisualElement root { get; private set; }

    // Panel 참조
    private VisualElement _storePanel;
    private VisualElement _moneyPanel;

    // 버튼 참조
    private Button _storeButton;
    private Button _mainButton;

    // 색상 상태
    private readonly Color inactiveColor = new(0.3f, 0.3f, 0.3f);
    private readonly Color activeColor = new(0.7f, 0.7f, 0.7f);

    // 애니메이션용
    private VisualElement _rootChild;
    private float _targetHeight;            // 현재 목표 높이
    private float _duration = 0.2f;         // 확장 시간
    private float _shrinkDuration = 0.8f;   // 수축 시간
    private float _overshootFactor = 1.01f; // 튕김 효과 배율

    // 패널별 크기
    private const float STORE_HEIGHT = 1000f;
    private const float MONEY_HEIGHT = 1900f;

    private void Awake()
    {
        // UI 루트 가져오기
        root = GetComponent<UIDocument>().rootVisualElement;

        // 패널 찾기
        _storePanel = root.Q<VisualElement>("StorePanel");
        _moneyPanel = root.Q<VisualElement>("MoneyPanel");

        // 버튼 찾기
        _storeButton = root.Q<Button>("StoreButton");
        _mainButton = root.Q<Button>("MoneyButton");

        _rootChild = root.Q<VisualElement>("StoreUI");

        // 버튼 이벤트 연결
        _storeButton?.RegisterCallback<ClickEvent>(_ => OnClickPanelButton(_storePanel, _storeButton, STORE_HEIGHT));
        _mainButton?.RegisterCallback<ClickEvent>(_ => OnClickPanelButton(_moneyPanel, _mainButton, MONEY_HEIGHT));

        // 초기 상태: StorePanel 열기
        OnClickPanelButton(_storePanel, _storeButton, STORE_HEIGHT);
    }

    void IMenuUI.ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        StoreManager.Instance.OpenStore();
       // StoreManagerRe.Instance.OpenStore();

        // 상점 열릴 때 FX 및 애니메이션 실행
        ParticleFxManager.Instance.Play("StoreOpen");
        StartCoroutine(AnimateUI());
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
        ParticleFxManager.Instance.Stop("StoreOpen");
    }

    /// <summary>
    /// 버튼 클릭 시 패널 전환 및 버튼 색상 업데이트
    /// </summary>
    private void OnClickPanelButton(VisualElement targetPanel, Button targetButton, float height)
    {
        // 목표 높이 갱신
        _targetHeight = height;

        // 모든 패널 숨기기
        _storePanel.style.display = DisplayStyle.None;
        _moneyPanel.style.display = DisplayStyle.None;

        // 모든 버튼 비활성화 색상
        _storeButton.style.unityBackgroundImageTintColor = inactiveColor;
        _mainButton.style.unityBackgroundImageTintColor = inactiveColor;

        // 선택된 패널 표시 및 버튼 강조
        targetPanel.style.display = DisplayStyle.Flex;
        targetButton.style.unityBackgroundImageTintColor = activeColor;
        
        if(targetPanel == _storePanel)
            ParticleFxManager.Instance.Play("StoreOpen");
        else
            ParticleFxManager.Instance.Stop("StoreOpen");

        // 패널 전환 시 애니메이션 실행
        StartCoroutine(AnimateUI());
    }

    /// <summary>
    /// 패널 열릴 때 위로 튕기듯 확장되는 UI 애니메이션
    /// </summary>
    private IEnumerator AnimateUI()
    {
        float elapsed = 0f;
        _rootChild.style.height = 0;
        float overshootHeight = _targetHeight * _overshootFactor;

        // 확장 단계
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _rootChild.style.height = Mathf.Lerp(0, overshootHeight, t);
            yield return null;
        }
        _rootChild.style.height = overshootHeight;

        // 수축 단계 (부드럽게)
        elapsed = 0f;
        float startHeight = overshootHeight;

        while (elapsed < _shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _shrinkDuration);
            float easedT = 1 - Mathf.Pow(1 - t, 3); // easeOutCubic
            _rootChild.style.height = Mathf.Lerp(startHeight, _targetHeight, easedT);
            yield return null;
        }
        _rootChild.style.height = _targetHeight;
    }
}
