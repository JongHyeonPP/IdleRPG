using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableScrollView : MonoBehaviour
{
    public ScrollView scrollView;
    public bool _isDragging { get; private set; } // 드래그 상태 플래그
    private Vector2 _previousPointerPosition; // 이전 프레임에서의 터치/마우스 위치
    private Vector2 _previousScrollOffset;
    private float _dragThreshold = 5f; // 드래그로 판단하는 최소 이동 거리

    // 인스펙터 노출
    [SerializeField] private float _scrollSpeed = 1f;
    [SerializeField] private ScrollViewMode _mode;

    //클릭 이벤트
    public Action OnItemClicked;
    private void Awake()
    {
        InitScrollView();
    }
    public void InitScrollView()
    {
        scrollView = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>();
        scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.mode = _mode;
        _previousPointerPosition = Vector2.zero;
        SetEvents();
    }

    void Update()
    {
        // 터치 또는 마우스 입력을 통해 드래그 상태를 판단
        bool pointerDown = false;
        Vector2 currentPointerPosition = Vector2.zero;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            currentPointerPosition = touch.position;
            pointerDown = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isDragging = false; // 드래그 종료
            }
        }
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            currentPointerPosition = Input.mousePosition;
            pointerDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false; // 드래그 종료
        }
#endif

        // 드래그 판정
        if (pointerDown)
        {
            if (_previousPointerPosition != Vector2.zero)
            {
                float distance = Vector2.Distance(currentPointerPosition, _previousPointerPosition);
                if (distance > _dragThreshold)
                {
                    _isDragging = true; // 드래그 상태로 전환
                }
            }
            _previousPointerPosition = currentPointerPosition;
        }
        else
        {
            _previousPointerPosition = Vector2.zero;
        }
    }

    private void SetEvents()
    {
        scrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _previousScrollOffset = scrollView.scrollOffset;
                evt.StopPropagation();
            }
        });

        scrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_isDragging) // 드래그 중이면 스크롤 동작 수행
            {
                Vector2 newScrollOffset = _previousScrollOffset;

                switch (scrollView.mode)
                {
                    case ScrollViewMode.Vertical:
                        newScrollOffset.y += -evt.deltaPosition.y * _scrollSpeed;
                        newScrollOffset.y = Mathf.Clamp(
                            newScrollOffset.y,
                            0,
                            Mathf.Max(0, scrollView.contentContainer.resolvedStyle.height - scrollView.resolvedStyle.height)
                        );
                        break;

                    case ScrollViewMode.Horizontal:
                        newScrollOffset.x += -evt.deltaPosition.x * _scrollSpeed;
                        newScrollOffset.x = Mathf.Clamp(
                            newScrollOffset.x,
                            0,
                            Mathf.Max(0, scrollView.contentContainer.resolvedStyle.width - scrollView.resolvedStyle.width)
                        );
                        break;

                    case ScrollViewMode.VerticalAndHorizontal:
                        newScrollOffset.y += -evt.deltaPosition.y * _scrollSpeed;
                        newScrollOffset.x += -evt.deltaPosition.x * _scrollSpeed;

                        newScrollOffset.y = Mathf.Clamp(
                            newScrollOffset.y,
                            0,
                            Mathf.Max(0, scrollView.contentContainer.resolvedStyle.height - scrollView.resolvedStyle.height)
                        );

                        newScrollOffset.x = Mathf.Clamp(
                            newScrollOffset.x,
                            0,
                            Mathf.Max(0, scrollView.contentContainer.resolvedStyle.width - scrollView.resolvedStyle.width)
                        );
                        break;
                }

                scrollView.scrollOffset = newScrollOffset;
                _previousScrollOffset = newScrollOffset;

                evt.StopPropagation();
            }
        });
    }
}
