using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBook : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private ListView _mainListView;
    private VisualElement _root;
    private void Awake()
    {
        
        _root = GetComponent<UIDocument>().rootVisualElement;

        _mainListView = _root.Q<ListView>("List");

    }
    private void Start()
    {
        InitializeMainListView();
    }
    public void ShowWeaponBook()
    {
        _mainListView.style.display = DisplayStyle.Flex;
    }
    public void HideWeaponBook()
    {
        _mainListView.style.display = DisplayStyle.None;
    }
    private void InitializeMainListView()
    {
        // 무기 데이터 준비
        var weaponCategories = new List<string>
        {
            "야만 요미",
            "새로운 요미",
            "초보 요미",
            "섬뜩한 요미",
            "빛의 요미",
            "강철의 요미",
            "날렵한 요미",
            "고대의 요미"
        };

        // ListView 구성
        _mainListView.itemsSource = weaponCategories;
        _mainListView.makeItem = () => new Label();
        _mainListView.bindItem = (element, index) =>
        {
            var label = (Label)element;
            label.text = weaponCategories[index];
        };

        // 카테고리 선택 시 세부 무기 리스트뷰 생성
        _mainListView.onSelectionChange += selectedItems =>
        {
            if (selectedItems.FirstOrDefault() is string category)
            {
                ShowWeaponDetails(category);
            }
        };
    }

    private void ShowWeaponDetails(string category)
    {
      

        // 세부 무기 데이터 가져오기
        var weaponDetails = GetWeaponsForCategory(category);

        // 리스트뷰 동적 생성
        var weaponListView = new ListView
        {
            itemsSource = weaponDetails,
            itemHeight = 50,
            makeItem = () => new Label(),
            bindItem = (element, index) =>
            {
                var label = (Label)element;
                label.text = weaponDetails[index];
            }
        };

        // 상세 뷰에 추가
        var detailsContainer = new VisualElement { name = "details-container" };
        detailsContainer.Add(weaponListView);
        _root.Add(detailsContainer);
    }

    private List<string> GetWeaponsForCategory(string category)
    {
        // 카테고리별 무기 리스트 예제
        return category switch
        {
            "야만 요미" => new List<string> { "무기 1", "무기 2", "무기 3", "무기 4" },
            "새로운 요미" => new List<string> { "무기 A", "무기 B", "무기 C" },
            _ => new List<string> { "기본 무기" }
        };
    }
}
