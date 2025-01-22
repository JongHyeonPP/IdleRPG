using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableScrollView : MonoBehaviour
{
    public ScrollView scrollView;
    public bool _isDragging { get; private set; } // �巡�� ���� �÷���
    private Vector2 _previousPointerPosition; // ���� �����ӿ����� ��ġ/���콺 ��ġ
    private Vector2 _previousScrollOffset;
    private float _dragThreshold = 5f; // �巡�׷� �Ǵ��ϴ� �ּ� �̵� �Ÿ�

    // �ν����� ����
    [SerializeField] private float _scrollSpeed = 1f;
    [SerializeField] private ScrollViewMode _mode;

    //Ŭ�� �̺�Ʈ
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
        // ��ġ �Ǵ� ���콺 �Է��� ���� �巡�� ���¸� �Ǵ�
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
                _isDragging = false; // �巡�� ����
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
            _isDragging = false; // �巡�� ����
        }
#endif

        // �巡�� ����
        if (pointerDown)
        {
            if (_previousPointerPosition != Vector2.zero)
            {
                float distance = Vector2.Distance(currentPointerPosition, _previousPointerPosition);
                if (distance > _dragThreshold)
                {
                    _isDragging = true; // �巡�� ���·� ��ȯ
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
            if (_isDragging) // �巡�� ���̸� ��ũ�� ���� ����
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
