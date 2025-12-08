using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NoticeDot
{
    #region 사용해야할 메서드
    public NoticeDot(VisualElement parentVe, MonoBehaviour parentMono)
    {
        _parentMono = parentMono;
        if (!parentMono.TryGetComponent<UIDocument>(out var uiDocument))
        {
            Debug.LogError("UIDocument를 찾을 수 없습니다.");
            return;
        }
        root = parentVe.Q<VisualElement>("NoticeDot");
        if (root == null)
        {
            Debug.LogError("Invalid Notice Dot");
            return;
        }

        _expand = root.Q<VisualElement>("Notice_Expand");
        _mainPanel = root.Q<VisualElement>("Notice_MainPanel");
        InactiveNotice();

        BattleBroker.SwitchToBattle += () => PauseOrResume(false);
        BattleBroker.SwitchToStory += (_,_) => PauseOrResume(true);
    }

    public void StartNotice()
    {
        if (isPaused)
        {
            startRequestedWhilePaused = true;
            return;
        }

        if (isOperating)
            return;

        isOperating = true;
        root.style.visibility = Visibility.Visible;
        _parentMono.StartCoroutine(CoroutineWithHashSet(AnimateLoop()));
    }

    public void StopNotice()
    {
        if (!isOperating)
            return;

        InactiveNotice();
    }

    private void InactiveNotice()
    {
        isOperating = false;
        root.style.visibility = Visibility.Hidden;
        foreach (var x in _coroutineSet)
            _parentMono.StopCoroutine(x);

        _expand.transform.scale = _mainPanel.transform.scale = Vector3.one;
        _coroutineSet.Clear();
    }

    public void PauseOrResume(bool pause)
    {
        if (pause)
        {
            isPaused = true;
            startRequestedWhilePaused = false; // Pause 직전 상태 초기화
            StopNotice();
        }
        else
        {
            isPaused = false;

            // Pause 중에 Start가 씹혀있었으면 강제로 Start
            if (startRequestedWhilePaused)
                StartNotice();

            startRequestedWhilePaused = false;
        }
    }

    public void OnPositionSet(float xSet, float ySet)
    {
        root.style.left = xSet;
        root.style.top = ySet;
    }
    #endregion

    #region 몰라도 됨
    public VisualElement root;
    private MonoBehaviour _parentMono;
    private readonly VisualElement _mainPanel;
    private readonly VisualElement _expand;
    private HashSet<Coroutine> _coroutineSet = new();

    private bool isOperating;
    private bool isPaused;
    private bool startRequestedWhilePaused;

    private const float _bigScaleNum = 1.2f;
    private const float _pulseScaleNum = 1.3f;
    private const float _bigDuration = 1f;
    private const float _pulseDuration = 0.2f;
    private const float _smallDuration = 0.5f;
    private const float _easeExponent = 1f;

    private const float _expandMin = 1f;
    private const float _expandMax = 5f;

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
            t = 1f - Mathf.Pow(1f - t, _easeExponent);
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
            t = 1f - Mathf.Pow(1f - t, 2f);
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
        float totalDuration = _pulseDuration + _smallDuration;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalDuration;
            t = 1f - Mathf.Pow(1f - t, 2f);

            _expand.transform.scale = Vector3.one * Mathf.Lerp(_expandMin, _expandMax, t);
            _expand.style.opacity = 1f - t;

            yield return null;
        }

        _expand.transform.scale = Vector3.one * _expandMax;
        _expand.style.opacity = 0f;
        _expand.transform.scale = Vector3.one * _expandMin;
        _expand.style.opacity = 1f;
    }
    #endregion
}
