using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NoticeDot
{
    #region 사용해야할 메서드
    //UI 관련 C# 스크립트에서 생성자를 통해 객체 생성
    public NoticeDot(VisualElement parentVe, MonoBehaviour parentMono)
    {
        _parentMono = parentMono;
        if (!parentMono.TryGetComponent<UIDocument>(out var uiDocument))
        {
            Debug.LogError("UIDocument를 찾을 수 없습니다.");
            return;
        }

        _parentRoot = uiDocument.rootVisualElement;
        _root = parentVe.Q<VisualElement>("NoticeDot");
        if (_root == null)
        {
            Debug.LogError("Invalid Notice Dot");
            return;
        }

        _expand = _root.Q<VisualElement>("Notice_Expand");
        _mainPanel = _root.Q<VisualElement>("Notice_MainPanel");
    }
    //렌더링 순위를 UIDocument 내에서 최상위에 위치시키는 메서드
    //DisplayStyle이 Flex인 시점에 호출해야 정상적으로 작동된다.
    public void SetParentToRoot()
    {
        _parentMono.StartCoroutine(SetParentToRootCor());
    }

    //Notice 띄울라면 이거 호출하면 됨
    public void StartNotice()
    {
        _root.style.display = DisplayStyle.Flex;
        _parentMono.StartCoroutine(AnimateLoop());
    }
    //Notice 감출라면 이거 호출하면 됨
    public void StopNotice()
    {
        _root.style.display = DisplayStyle.None;
        foreach (var x in _coroutineSet)
        {
            _parentMono.StopCoroutine(x);
        }
        _expand.transform.scale =_mainPanel.transform.scale = Vector3.one;
        _coroutineSet.Clear();
    }
    //SetparentToRoot를 발동하면 원래 부모의 좌표를 따라가지 않아서 별도로 위치 이동시킬 때 호출하면 됨
    public void OnPositionChange(float xChange, float yChange)
    {
        // 현재 위치 가져오기
        float currentLeft = _root.resolvedStyle.left;
        float currentTop = _root.resolvedStyle.top;

        // 변경된 위치 적용
        _root.style.left = currentLeft + xChange;
        _root.style.top = currentTop + yChange;
    }

    #endregion
    #region 몰라도 됨
    private VisualElement _parentRoot;
    private VisualElement _root;
    private MonoBehaviour _parentMono;//코루틴 발동 매개체
    //readonly=>생성자 초기화
    private readonly VisualElement _mainPanel;
    private readonly VisualElement _expand;
    private HashSet<Coroutine> _coroutineSet = new();
    //생성자를 이용해 사용해야 함. 게임 오브젝트 생성 X
    // MainPanel 애니메이션 설정
    private const float _bigScaleNum = 1.2f;
    private const float _pulseScaleNum = 1.3f; // 짧은 커지는 효과의 크기
    private const float _bigDuration = 1f;
    private const float _pulseDuration = 0.2f; // 짧고 빠른 커짐
    private const float _smallDuration = 0.5f;
    private const float _easeExponent = 1f; // Ease-Out 파라미터

    // Expand 애니메이션 설정
    private const float _expandMin = 1f; // 최소 크기
    private const float _expandMax = 5f; // 최대 크기
    private IEnumerator SetParentToRootCor()
    {
        Vector2 unsettedPosition = _root.worldBound.position;
        
        while (_root.worldBound.position == unsettedPosition)
        {
            yield return null;
        }
        Vector2 worldPosition = _root.worldBound.position;
        _root.RemoveFromHierarchy();
        _parentRoot.Add(_root);

        Vector2 localPosition = _parentRoot.WorldToLocal(worldPosition);
        _root.style.position = Position.Absolute;
        _root.style.left = localPosition.x;
        _root.style.top = localPosition.y;
    }
    private IEnumerator AnimateLoop()
    {
        while (true)
        {
            yield return CoroutineWithHashSet(ScaleUp());
            _parentMono.StartCoroutine(CoroutineWithHashSet(ExpandUp()));
            yield return CoroutineWithHashSet(ScalePulse());
            yield return CoroutineWithHashSet(ScaleDown());
        }
    }

    private IEnumerator CoroutineWithHashSet(IEnumerator coroutineMethod)
    {
        Coroutine coroutineTemp = _parentMono.StartCoroutine(coroutineMethod);
        _coroutineSet.Add(coroutineTemp);
        yield return coroutineTemp;
        _coroutineSet.Remove(coroutineTemp);
    }

    private IEnumerator ScaleUp()
    {
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * _bigScaleNum;
        float elapsedTime = 0f;

        while (elapsedTime < _bigDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _bigDuration;
            t = 1f - Mathf.Pow(1f - t, _easeExponent); // Ease-Out 적용
            _mainPanel.transform.scale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        _mainPanel.transform.scale = targetScale;
    }

    private IEnumerator ScalePulse()
    {
        Vector3 startScale = Vector3.one * _bigScaleNum;
        Vector3 targetScale = Vector3.one * _pulseScaleNum;
        float elapsedTime = 0f;

        while (elapsedTime < _pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _pulseDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // 강한 Ease-Out
            _mainPanel.transform.scale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        _mainPanel.transform.scale = targetScale;
    }

    private IEnumerator ScaleDown()
    {
        Vector3 startScale = Vector3.one * _pulseScaleNum;
        Vector3 targetScale = Vector3.one;
        float elapsedTime = 0f;

        while (elapsedTime < _smallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _smallDuration;
            _mainPanel.transform.scale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        _mainPanel.transform.scale = targetScale;
    }

    private IEnumerator ExpandUp()
    {
        float elapsedTime = 0f;
        float totalDuration = _pulseDuration + _smallDuration; // Expand는 pulse 시작 → small 종료까지 커짐

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // Ease-Out 적용

            // 크기 변화 (1 → 5)
            _expand.transform.scale = Vector3.one * Mathf.Lerp(_expandMin, _expandMax, t);
            // 알파값 변화 (1 → 0)
            _expand.style.opacity = 1f - t;

            yield return null;
        }
        _expand.transform.scale = Vector3.one * _expandMax;
        _expand.style.opacity = 0f;

        // Expand 크기 즉시 1로 리셋 후 반복
        _expand.transform.scale = Vector3.one * _expandMin;
        _expand.style.opacity = 1f;
    }
    #endregion
}