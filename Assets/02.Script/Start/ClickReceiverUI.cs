using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ClickReceiverUI: MonoBehaviour
{
    //��� ���� ������ �Ϲ� ����
    [SerializeField] StartManager startManager;
    private Label startLabel;
    private VisualElement clickReceiver;
    private void Awake()
    {
        // UI Element ����
        var root = GetComponent<UIDocument>().rootVisualElement;
        clickReceiver = root.Q<VisualElement>("ClickReceiver");
        startLabel = root.Q<Label>("StartLabel");
        // Ŭ�� ���ù��� Ŭ�� �̺�Ʈ ����
        clickReceiver.RegisterCallback<ClickEvent>(OnClickReceiver);
        // Start Label �����̴� �ִϸ��̼� ����
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
