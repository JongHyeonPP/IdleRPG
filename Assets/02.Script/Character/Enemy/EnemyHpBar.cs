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
        rootChild.style.bottom = enemyScreenPos.y - 50f; // UI Toolkit ��ǥ��� �Ʒ����� ���� �ö�
    }
    public void SetDisplay(bool isDisplay)
    {
        rootChild.style.display = isDisplay ? DisplayStyle.Flex : DisplayStyle.None;
    }
    public void SetHpRatio(float hpRatio)
    {
        // ��� ����
        _hpBar.value = hpRatio;

        // �̹� ���� ���� �ڷ�ƾ�� �ִٸ� ����
        if (_delayedReduceCoroutine != null)
            StopCoroutine(_delayedReduceCoroutine);

        // �� �ڷ�ƾ ����
        _delayedReduceCoroutine = StartCoroutine(ReduceDelayedBar(hpRatio));
    }

    private IEnumerator ReduceDelayedBar(float targetRatio)
    {
        float current = _delayedBar.value;
        float speed = 1.0f; // �ʴ� 1.0 ������ŭ ���� (�ӵ� ���� ����)

        while (_delayedBar.value > targetRatio)
        {
            current -= Time.deltaTime * speed;
            _delayedBar.value = Mathf.Max(current, targetRatio); // �ּҰ� ����
            yield return null;
        }

        _delayedBar.value = targetRatio; // ��Ȯ�� ����
    }

}
