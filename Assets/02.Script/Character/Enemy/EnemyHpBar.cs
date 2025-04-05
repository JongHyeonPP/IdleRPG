using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHpBar : MonoBehaviour
{
    private VisualElement rootChild;
    public EnemyHpBarPool pool;
    private ProgressBar _hpBar;
    private ProgressBar _delayedBar;
    private Coroutine _delayedReduceCoroutine;
    private void Awake()
    {
        rootChild = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("EnemyHpBar");
        _hpBar = rootChild.Q<ProgressBar>("HpBar");
        _hpBar.Q<VisualElement>(className: "unity-progress-bar__progress").style.backgroundColor = new Color(1f, 0.22f, 0.22f);
        _delayedBar = rootChild.Q<ProgressBar>("DelayedBar");
    }
    public void SetPosition(Vector2 enemyScreenPos )
    {
        rootChild.style.left = enemyScreenPos.x - 70f;
        rootChild.style.bottom = enemyScreenPos.y - 50f; // UI Toolkit 좌표계는 아래에서 위로 올라감
    }
    public void SetDisplay(bool isDisplay)
    {
        rootChild.style.display = isDisplay ? DisplayStyle.Flex : DisplayStyle.None;
    }
    public void SetHpRatio(float hpRatio)
    {
        // 즉시 변경
        _hpBar.value = hpRatio;

        // 이미 실행 중인 코루틴이 있다면 중지
        if (_delayedReduceCoroutine != null)
            StopCoroutine(_delayedReduceCoroutine);

        // 새 코루틴 시작
        _delayedReduceCoroutine = StartCoroutine(ReduceDelayedBar(hpRatio));
    }

    private IEnumerator ReduceDelayedBar(float targetRatio)
    {
        float current = _delayedBar.value;
        float speed = 1.0f; // 초당 1.0 비율만큼 감소 (속도 조절 가능)

        while (_delayedBar.value > targetRatio)
        {
            current -= Time.deltaTime * speed;
            _delayedBar.value = Mathf.Max(current, targetRatio); // 최소값 보정
            yield return null;
        }

        _delayedBar.value = targetRatio; // 정확히 맞춤
    }

}
