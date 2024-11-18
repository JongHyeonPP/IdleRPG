using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ClickReceiverUI: MonoBehaviour
{
    [SerializeField] StartManager _startManager;//StartManager�� �̱����� �ƴ϶� �ν����͸� ���� �����Ѵ�.
    //���� �� ����� ���ִ� �ؽ�Ʈ
    private Label _startLabel;
    private VisualElement _clickReceiver;
    private void Awake()
    {
        // UI Element ����
        var _root = GetComponent<UIDocument>().rootVisualElement;
        _clickReceiver = _root.Q<VisualElement>("ClickReceiver");
        _startLabel = _root.Q<Label>("StartLabel");
        // Ŭ�� ���ù��� Ŭ�� �̺�Ʈ ����
        _clickReceiver.RegisterCallback<ClickEvent>(OnClickReceiver);
        // Start Label �����̴� �ִϸ��̼� ����
        StartCoroutine(BlinkStart());
    }
    //StartBext�� ���İ��� �ֱ������� �����ϵ��� �����Ѵ�.
    private IEnumerator BlinkStart()
    {
        yield return new WaitForSeconds(0.1f);
        //�������� �ݹ��� �̿��� ���ѷ��� ����
        _startLabel.RegisterCallback<TransitionEndEvent>(BlinkToggle);
        BlinkOut();
    }
    //���İ��� �پ���.
    private void BlinkIn()
    {
        _startLabel.RemoveFromClassList("StartLabel-Blink");
    }
    //���İ��� �þ��.
    private void BlinkOut()
    {
        _startLabel.AddToClassList("StartLabel-Blink");
    }
    //��Ȳ�� ���� ���İ��� �þ�ų� �پ�鵵�� �Ѵ�.
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
    //ClickReceiver�� Ŭ������ �� �Ͼ �ϵ��� ����
    private void OnClickReceiver(ClickEvent evt)
    {
        _startManager.OnClickedStartImage();

        gameObject.SetActive(false);
    }

}
