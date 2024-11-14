using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ClickReceiverUI: MonoBehaviour
{
    //잠깐 쓰고 말꺼라 일반 변수
    [SerializeField] StartManager startManager;
    private Label startLabel;
    private VisualElement clickReceiver;
    private void Awake()
    {
        // UI Element 설정
        var root = GetComponent<UIDocument>().rootVisualElement;
        clickReceiver = root.Q<VisualElement>("ClickReceiver");
        startLabel = root.Q<Label>("StartLabel");
        // 클릭 리시버에 클릭 이벤트 연결
        clickReceiver.RegisterCallback<ClickEvent>(OnClickReceiver);
        // Start Label 깜박이는 애니메이션 시작
        StartCoroutine(BlinkStart());
    }
    private IEnumerator BlinkStart()
    {
        yield return new WaitForSeconds(0.1f);
        startLabel.RegisterCallback<TransitionEndEvent>(BlinkToggle);
        BlinkOut();
    }

    private void BlinkIn()
    {
        startLabel.RemoveFromClassList("StartLabel-Blink");
    }

    private void BlinkOut()
    {
        startLabel.AddToClassList("StartLabel-Blink");
    }

    private void BlinkToggle(TransitionEndEvent evt)
    {
        if (startLabel.ClassListContains("StartLabel-Blink"))
        {
            BlinkIn();
        }
        else
        {
            BlinkOut();
        }
    }

    private void OnClickReceiver(ClickEvent evt)
    {
        startManager.OnClickedStartImage();

        gameObject.SetActive(false);
    }

}
