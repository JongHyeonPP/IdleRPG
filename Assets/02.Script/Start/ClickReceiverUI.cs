using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ClickReceiverUI: MonoBehaviour
{
    [SerializeField] StartManager _startManager;//StartManager는 싱글톤이 아니라 인스펙터를 통해 참조한다.
    //시작 시 가운데에 떠있는 텍스트
    private Label _startLabel;
    private VisualElement _clickReceiver;
    private void Awake()
    {
        // UI Element 설정
        var _root = GetComponent<UIDocument>().rootVisualElement;
        _clickReceiver = _root.Q<VisualElement>("ClickReceiver");
        _startLabel = _root.Q<Label>("StartLabel");
        // 클릭 리시버에 클릭 이벤트 연결
        _clickReceiver.RegisterCallback<ClickEvent>(OnClickReceiver);
        // Start Label 깜박이는 애니메이션 시작
        StartCoroutine(BlinkStart());
    }
    //StartBext의 알파값을 주기적으로 변경하도록 유도한다.
    private IEnumerator BlinkStart()
    {
        yield return new WaitForSeconds(0.1f);
        //레지스터 콜백을 이용한 무한루프 유도
        _startLabel.RegisterCallback<TransitionEndEvent>(BlinkToggle);
        BlinkOut();
    }
    //알파값이 줄어든다.
    private void BlinkIn()
    {
        _startLabel.RemoveFromClassList("StartLabel-Blink");
    }
    //알파값이 늘어난다.
    private void BlinkOut()
    {
        _startLabel.AddToClassList("StartLabel-Blink");
    }
    //상황에 맞춰 알파값이 늘어나거나 줄어들도록 한다.
    private void BlinkToggle(TransitionEndEvent evt)
    {
        if (_startLabel.ClassListContains("StartLabel-Blink"))
        {
            BlinkIn();
        }
        else
        {
            BlinkOut();
        }
    }
    //ClickReceiver를 클릭했을 때 일어날 일들을 정의
    private void OnClickReceiver(ClickEvent evt)
    {
        _startManager.OnClickedStartImage();

        gameObject.SetActive(false);
    }

}
