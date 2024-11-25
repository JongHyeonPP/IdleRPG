using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ListViewBase : MonoBehaviour
{
    //상속 공통
    private ListView listView;
    private ScrollView scrollView; // ScrollView를 저장할 변수
    private bool isDragging = false; // 드래그 상태 확인
    private Vector2 previousScrollOffset; // 이전 스크롤 위치 저장
    private readonly List<int> items = new();
    [SerializeField] LVItemController controller;
    void Start()
    {
        // UI Document의 rootVisualElement 가져오기
        var root = GetComponent<UIDocument>().rootVisualElement;

        // ListView 가져오기 (UI Builder에서 생성한 경우)
        listView = root.Q<ListView>("ListView");

        for (int i = 0; i < controller.count; i++)
        {
            items.Add(i);
        }
        // ListView 초기화
        listView.itemsSource = items; // 데이터 소스
        listView.makeItem = MakeItem; // 새 UI 아이템 생성
        listView.bindItem = BindItem; // UI와 데이터 바인딩
        listView.fixedItemHeight = controller.itemHeight; // 각 아이템의 높이 설정
        listView.selectionType = SelectionType.Single; // 선택 유형 설정

        listView.Rebuild();
        // ScrollView 가져오기
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
        // 터치 입력 처리
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false; // 터치가 끝난 경우 상태 해제
                Debug.Log("Pointer up detected (Touch API)!");
            }
        }

        // 마우스 입력 처리 (에디터 또는 PC 환경)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false; // 눌림 상태 해제
            Debug.Log("Pointer up detected (Mouse)!");
        }
    }

    private void SetDragEvents()
    {
        // 마우스 또는 터치 눌림 상태 확인
        scrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                isDragging = true; // 드래그 시작
                previousScrollOffset = scrollView.scrollOffset; // 시작 위치 저장
                evt.StopPropagation(); // 이벤트 전파 중단
            }
        });

        // 드래그 종료
        scrollView.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                isDragging = false; // 드래그 종료
                evt.StopPropagation(); // 이벤트 전파 중단
            }
        });

        // 드래그 이동
        scrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (isDragging)
            {
                Vector2 newScrollOffset = previousScrollOffset + new Vector2(0, -evt.deltaPosition.y * 0.5f);

                // 스크롤 위치 제한
                newScrollOffset.y = Mathf.Clamp(
                    newScrollOffset.y,
                    0,
                    scrollView.contentContainer.resolvedStyle.height - scrollView.resolvedStyle.height
                );

                // 새로운 스크롤 위치 적용
                scrollView.scrollOffset = newScrollOffset;
                previousScrollOffset = newScrollOffset;

                evt.StopPropagation(); // 이벤트 전파 중단
            }
        });
    }

    // 새 UI 아이템 생성
    private VisualElement MakeItem()
    {
        return controller.GetTemplate();
    }

    // 데이터와 UI 바인딩
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
        // 기존 데이터를 새 데이터로 교체
        items.Clear();
        items.AddRange(newItems);

        // ListView를 새로고침
        listView.Rebuild();
    }
}
