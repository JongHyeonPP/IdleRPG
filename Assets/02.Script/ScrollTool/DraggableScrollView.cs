using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableScrollView : MonoBehaviour
{
    public ScrollView scrollView;
    public bool _isDragging { get; private set; }
    private Vector2 _previousPointerPosition;
    private Vector2 _previousScrollOffset;
    private float _dragThreshold = 5f;

    [SerializeField] private float _scrollSpeed = 1f;
    [SerializeField] private ScrollViewMode _mode;
    [SerializeField] protected UIDocument _targetDocument;
    [SerializeField] private string _scrollViewName;

    [SerializeField] private float _clickSuppressDuration = 0.12f;
    private float _suppressUntilTime;

    private void Awake()
    {
        InitScrollView();
    }

    public void InitScrollView()
    {
        if (_targetDocument == null)
            _targetDocument = GetComponent<UIDocument>();

        if (_targetDocument == null)
        {
            Debug.LogError("UIDocument is null");
            Destroy(this);
            return;
        }

        if (string.IsNullOrEmpty(_scrollViewName))
            scrollView = _targetDocument.rootVisualElement.Q<ScrollView>();
        else
            scrollView = _targetDocument.rootVisualElement.Q<VisualElement>(_scrollViewName)?.Q<ScrollView>();

        if (scrollView == null)
        {
            Debug.LogError("ScrollView not found");
            Destroy(this);
            return;
        }

        scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.mode = _mode;
        _previousPointerPosition = Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        SetEvents();
#endif
    }

    private Coroutine _scrollLockCoroutine;
    private Vector2 _lockedScrollPosition;

    public void LockScrollPosition()
    {
        if (scrollView == null) return;

        _lockedScrollPosition = scrollView.scrollOffset;

        if (_scrollLockCoroutine != null)
            StopCoroutine(_scrollLockCoroutine);

        _scrollLockCoroutine = StartCoroutine(KeepScrollPosition());
    }

    private IEnumerator KeepScrollPosition()
    {
        while (true)
        {
            scrollView.scrollOffset = _lockedScrollPosition;
            yield return null;
        }
    }

    public void UnlockScrollPosition()
    {
        if (_scrollLockCoroutine != null)
        {
            StopCoroutine(_scrollLockCoroutine);
            _scrollLockCoroutine = null;
        }

        scrollView.scrollOffset = _lockedScrollPosition;

        scrollView.RegisterCallback<PointerMoveEvent>(BlockScrollEvent);
        scrollView.RegisterCallback<WheelEvent>(BlockScrollEvent);

        scrollView.schedule.Execute(() =>
        {
            scrollView.UnregisterCallback<PointerMoveEvent>(BlockScrollEvent);
            scrollView.UnregisterCallback<WheelEvent>(BlockScrollEvent);
        }).ExecuteLater(100);
    }

    private void BlockScrollEvent(EventBase evt)
    {
        evt.StopPropagation();
    }

    public bool ShouldBlockClick()
    {
        return _isDragging || Time.unscaledTime < _suppressUntilTime;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void Update()
    {
        bool pointerDown = false;
        Vector2 currentPointerPosition = Vector2.zero;

        if (Input.GetMouseButton(0))
        {
            currentPointerPosition = Input.mousePosition;
            pointerDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
                _suppressUntilTime = Time.unscaledTime + _clickSuppressDuration;

            _isDragging = false;
        }

        if (pointerDown)
        {
            if (_previousPointerPosition != Vector2.zero)
            {
                float distance = Vector2.Distance(currentPointerPosition, _previousPointerPosition);
                if (distance > _dragThreshold)
                {
                    _isDragging = true;
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

        scrollView.RegisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (_isDragging)
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
    }
#endif
}
