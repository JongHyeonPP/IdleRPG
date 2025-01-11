using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// �׳� ui��Ŷ ����...
/// </summary>
public class TestUITookit : MonoBehaviour
{
    private Button _drawBtn;                // Draw ��ư
    private Label _statusLabel;             // ���¸� ǥ���ϴ� ��
    private Slider _valueSlider;            // �� ������ ���� �����̴�
    private Toggle _toggleOption;           // Ȱ��/��Ȱ�� ���

    private void OnEnable()
    {
        // UIDocument���� ��Ʈ ��� ��������
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Draw ��ư ��� �ʱ�ȭ �� Ŭ�� �̺�Ʈ ���
        _drawBtn = root.Q<Button>("DrawBtn");
        _drawBtn?.RegisterCallback<ClickEvent>(evt => UpdateLabel("Draw ��ư�� Ŭ���Ǿ����ϴ�."));

        // ���� ǥ�� �� ��� �ʱ�ȭ
        _statusLabel = root.Q<Label>("StatusLabel");

        // �����̴� ��� �ʱ�ȭ �� �� ���� �̺�Ʈ ���
        _valueSlider = root.Q<Slider>("ValueSlider");
        _valueSlider?.RegisterValueChangedCallback(evt => UpdateLabel($"�����̴� ��: {evt.newValue:F2}"));

        // ��� ��� �ʱ�ȭ �� �� ���� �̺�Ʈ ���
        _toggleOption = root.Q<Toggle>("ToggleOption");
        _toggleOption?.RegisterValueChangedCallback(evt => UpdateLabel(evt.newValue ? "����� Ȱ��ȭ�Ǿ����ϴ�." : "����� ��Ȱ��ȭ�Ǿ����ϴ�."));
    }

    // ���� ���� ������Ʈ�ϴ� �޼���
    private void UpdateLabel(string newText)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = newText;
        }
    }

    private void OnDisable()
    {
        // �̺�Ʈ ����
        _drawBtn?.UnregisterCallback<ClickEvent>(evt => UpdateLabel("Draw ��ư�� Ŭ���Ǿ����ϴ�."));
        _valueSlider?.UnregisterValueChangedCallback(evt => UpdateLabel($"�����̴� ��: {evt.newValue:F2}"));
        _toggleOption?.UnregisterValueChangedCallback(evt => UpdateLabel(evt.newValue ? "����� Ȱ��ȭ�Ǿ����ϴ�." : "����� ��Ȱ��ȭ�Ǿ����ϴ�."));
    }
}
