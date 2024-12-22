using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableScrollView : MonoBehaviour
{
    protected VisualElement _scrollView;
    protected bool _isDragging = false;
    protected Vector2 _lastMousePosition;
    protected VisualElement _content;
    protected VisualElement root;
    protected string _scrollviewName;
    
    protected virtual float MinY => -1150f;
    protected virtual float MaxY => -50f;

    protected virtual void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _scrollView = root.Q<VisualElement>(_scrollviewName);
        _content = _scrollView.Q<VisualElement>("unity-content-container");
    }

    protected virtual void Start()
    {
        _scrollView.RegisterCallback<PointerDownEvent>(OnScrollDown);
        _scrollView.RegisterCallback<PointerMoveEvent>(OnScrollMove);
        _scrollView.RegisterCallback<PointerUpEvent>(OnScrollUp);
        _scrollView.RegisterCallback<PointerLeaveEvent>(evt => { _isDragging = false; });
    }

    protected void OnScrollDown(PointerDownEvent evt)
    {
        _isDragging = true;
        _lastMousePosition = evt.position;
    }

    protected void OnScrollMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        Vector2 delta = (Vector2)evt.position - _lastMousePosition;

        float currentY = _content.transform.position.y;
        float newY = currentY + delta.y;

        if (newY > MaxY)
        {
            newY = MaxY;
            StartCoroutine(SmoothMoveToOriginalY(MaxY));
        }
        else if (newY < MinY)
        {
            newY = MinY;
            StartCoroutine(SmoothMoveToOriginalY(MinY));
        }

        _content.transform.position = new Vector3(
            _content.transform.position.x,
            newY,
            0
        );

        _lastMousePosition = evt.position;
    }

    protected IEnumerator SmoothMoveToOriginalY(float targetY)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPosition = _content.transform.position;
        Vector3 targetPosition = new Vector3(
            _content.transform.position.x,
            targetY,
            0
        );

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _content.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / duration
            );
            yield return null;
        }

        _content.transform.position = targetPosition;
    }

    protected void OnScrollUp(PointerUpEvent evt)
    {
        _isDragging = false;
        evt.StopPropagation();
    }
}
