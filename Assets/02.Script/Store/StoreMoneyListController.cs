using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StoreMoneyListController : MonoBehaviour
{
    [Header("UI Toolkit Bindings")]
    [SerializeField] private UIDocument _ui;
    [SerializeField] private ScrollView _scrollView;         // 인스펙터로 할당 or 런타임에 Q로 찾기
    [SerializeField] private VisualTreeAsset _itemTemplate;  // 아이템 UXML 템플릿

    // 루트 문서에서 id로 가져오려면 사용
    [SerializeField] private string _scrollViewId = "MoneyScrollView"; // ScrollView의 UXML id

    readonly List<ItemView> _items = new();

    void Awake()
    {
        if (_ui == null) _ui = GetComponent<UIDocument>();
        if (_scrollView == null && _ui != null)
            _scrollView = _ui.rootVisualElement.Q<ScrollView>(_scrollViewId);
    }

    public void SetItems(IList<StoreMoneyItemData> dataList)
    {
        if (_scrollView == null || _itemTemplate == null) return;

        // 기존 항목 정리(이벤트 해제 및 UI 비움)
        foreach (var it in _items) it.Dispose();
        _items.Clear();
        _scrollView.Clear();

        // 생성
        for (int i = 0; i < dataList.Count; i++)
        {
            var ve = _itemTemplate.Instantiate();    // 템플릿 인스턴스
            ve.style.flexGrow = 0;                   // 필요시 레이아웃 조정
            _scrollView.Add(ve);

            var view = new ItemView(ve);
            view.Bind(dataList[i]);
            _items.Add(view);
        }
    }

    // 개별 아이템 뷰 래퍼
    sealed class ItemView
    {
        readonly VisualElement _root;
        readonly Button _btn;                 // #StoreMoneyBtn
        readonly VisualElement _icon;         // #StoreMoneyBtnIcon (Image 또는 VisualElement)
        readonly Label _gold;                 // #StoreGlodLabel
        readonly Label _goldExtra;            // #SlotGoldExLabel
        readonly Label _money;                // #SlotMoneyLabel
        System.Action _onClick;               // 이벤트 참조 보관

        public ItemView(VisualElement root)
        {
            _root = root;
            _btn = root.Q<Button>("StoreMoneyBtn");
            _icon = root.Q<VisualElement>("StoreMoneyBtnIcon");
            _gold = root.Q<Label>("StoreGlodLabel");   // 원문 오타 그대로 사용
            _goldExtra = root.Q<Label>("SlotGoldExLabel");
            _money = root.Q<Label>("SlotMoneyLabel");
        }

        public void Bind(StoreMoneyItemData data)
        {
            // 텍스트 바인딩
            if (_gold != null) _gold.text = data.Gold ?? data.GoldEx; // 제목 자리에 금액 텍스트를 쓰고 싶다면 조정
            if (_goldExtra != null) _goldExtra.text = data.GoldEx ?? string.Empty;
            if (_money != null) _money.text = data.Money ?? string.Empty;

            // 아이콘 바인딩 (Image 또는 VisualElement 배경 지원)
            if (_icon != null && data.Icon != null)
            {
                var img = _icon as Image;
                if (img != null)
                {
                    img.image = data.Icon;
                }
                else
                {
                    _icon.style.backgroundImage = new StyleBackground(data.Icon);
                    _icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                }
            }

            // 버튼 이벤트 교체
            if (_btn != null)
            {
                if (_onClick != null) _btn.clicked -= _onClick;
                _onClick = data.OnClick;
                if (_onClick != null) _btn.clicked += _onClick;
            }
        }

        public void Dispose()
        {
            if (_btn != null && _onClick != null) _btn.clicked -= _onClick;
            _onClick = null;
        }
    }
}
