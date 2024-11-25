using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ListViewBase : MonoBehaviour
{
    //��� ����
    private ListView listView;
    private ScrollView scrollView; // ScrollView�� ������ ����
    private bool isDragging = false; // �巡�� ���� Ȯ��
    private Vector2 previousScrollOffset; // ���� ��ũ�� ��ġ ����
    private readonly List<int> items = new();
    [SerializeField] LVItemController controller;
    void Start()
    {
        // UI Document�� rootVisualElement ��������
        var root = GetComponent<UIDocument>().rootVisualElement;

        // ListView �������� (UI Builder���� ������ ���)
        listView = root.Q<ListView>("ListView");

        for (int i = 0; i < controller.count; i++)
        {
            items.Add(i);
        }
        // ListView �ʱ�ȭ
        listView.itemsSource = items; // ������ �ҽ�
        listView.makeItem = MakeItem; // �� UI ������ ����
        listView.bindItem = BindItem; // UI�� ������ ���ε�
        listView.fixedItemHeight = controller.itemHeight; // �� �������� ���� ����
        listView.selectionType = SelectionType.Single; // ���� ���� ����

        listView.Rebuild();
        // ScrollView ��������
        scrollView = listView.Q<ScrollView>();
        if (scrollView == null)
        {
            Debug.LogError("ScrollView could not be found!");
            return;
        }
        scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        SetDragEvents();
    }

    void Update()
    {
        // ��ġ �Է� ó��
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false; // ��ġ�� ���� ��� ���� ����
                Debug.Log("Pointer up detected (Touch API)!");
            }
        }

        // ���콺 �Է� ó�� (������ �Ǵ� PC ȯ��)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false; // ���� ���� ����
            Debug.Log("Pointer up detected (Mouse)!");
        }
    }

    private void SetDragEvents()
    {
        // ���콺 �Ǵ� ��ġ ���� ���� Ȯ��
        scrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                isDragging = true; // �巡�� ����
                previousScrollOffset = scrollView.scrollOffset; // ���� ��ġ ����
                evt.StopPropagation(); // �̺�Ʈ ���� �ߴ�
            }
        });

        // �巡�� ����
        scrollView.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                isDragging = false; // �巡�� ����
                evt.StopPropagation(); // �̺�Ʈ ���� �ߴ�
            }
        });

        // �巡�� �̵�
        scrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (isDragging)
            {
                Vector2 newScrollOffset = previousScrollOffset + new Vector2(0, -evt.deltaPosition.y * 0.5f);

                // ��ũ�� ��ġ ����
                newScrollOffset.y = Mathf.Clamp(
                    newScrollOffset.y,
                    0,
                    scrollView.contentContainer.resolvedStyle.height - scrollView.resolvedStyle.height
                );

                // ���ο� ��ũ�� ��ġ ����
                scrollView.scrollOffset = newScrollOffset;
                previousScrollOffset = newScrollOffset;

                evt.StopPropagation(); // �̺�Ʈ ���� �ߴ�
            }
        });
    }

    // �� UI ������ ����
    private VisualElement MakeItem()
    {
        return controller.GetTemplate();
    }

    // �����Ϳ� UI ���ε�
    private void BindItem(VisualElement element, int index)
    {
        controller.BindItem(element, index);
    }
    public void AddItem(int newItem)
    {
        items.Add(newItem);
        listView.Rebuild();
    }
    void UpdateItems(List<int> newItems)
    {
        // ���� �����͸� �� �����ͷ� ��ü
        items.Clear();
        items.AddRange(newItems);

        // ListView�� ���ΰ�ħ
        listView.Rebuild();
    }
}
