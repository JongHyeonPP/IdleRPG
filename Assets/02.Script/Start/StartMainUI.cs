using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class StartMainUI : MonoBehaviour
{
    private Label _loadingLabel;
    private Label _bottomLabel;
    private void Awake()
    {
        // UI Element ����
        var root = GetComponent<UIDocument>().rootVisualElement;
        _loadingLabel = root.Q<Label>("LoadingLabel");
        _bottomLabel = root.Q<Label>("BottomLabel");

        // UI �ʱ� ���� ����
        _loadingLabel.style.visibility = Visibility.Hidden;
        _bottomLabel.style.visibility = Visibility.Hidden;
    }
    //���� ���� �ε� ���̶�� ����.
    public void LoadingLebelSet()
    {
        _loadingLabel.style.visibility = Visibility.Visible;
        _bottomLabel.style.visibility = Visibility.Hidden;
    }
    //���� ��Ȳ�� �����ִ� ���� �ϴ� UI�� ������ �ִ´�.
    public void SetBottomLabelText(string text)
    {
        _bottomLabel.text = text;
        _bottomLabel.style.visibility = Visibility.Visible;
    }
}
