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
        // ���� ������ �غ�
        var weaponCategories = new List<string>
        {
            "�߸� ���",
            "���ο� ���",
            "�ʺ� ���",
            "������ ���",
            "���� ���",
            "��ö�� ���",
            "������ ���",
            "����� ���"
        };

        // ListView ����
        _mainListView.itemsSource = weaponCategories;
        _mainListView.makeItem = () => new Label();
        _mainListView.bindItem = (element, index) =>
        {
            var label = (Label)element;
            label.text = weaponCategories[index];
        };

        // ī�װ� ���� �� ���� ���� ����Ʈ�� ����
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
      

        // ���� ���� ������ ��������
        var weaponDetails = GetWeaponsForCategory(category);

        // ����Ʈ�� ���� ����
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

        // �� �信 �߰�
        var detailsContainer = new VisualElement { name = "details-container" };
        detailsContainer.Add(weaponListView);
        _root.Add(detailsContainer);
    }

    private List<string> GetWeaponsForCategory(string category)
    {
        // ī�װ��� ���� ����Ʈ ����
        return category switch
        {
            "�߸� ���" => new List<string> { "���� 1", "���� 2", "���� 3", "���� 4" },
            "���ο� ���" => new List<string> { "���� A", "���� B", "���� C" },
            _ => new List<string> { "�⺻ ����" }
        };
    }
}
