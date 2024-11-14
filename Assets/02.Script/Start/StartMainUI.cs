using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class StartMainUI : MonoBehaviour
{
    private Label loadingLabel;
    private Label bottomLabel;
    private void Awake()
    {
        // UI Element ����
        var root = GetComponent<UIDocument>().rootVisualElement;
        loadingLabel = root.Q<Label>("LoadingLabel");
        bottomLabel = root.Q<Label>("BottomLabel");

        // UI �ʱ� ���� ����
        loadingLabel.style.visibility = Visibility.Hidden;
        bottomLabel.style.visibility = Visibility.Hidden;
    }
    public void InitLabel()
    {
        loadingLabel.style.visibility = Visibility.Visible;
        bottomLabel.style.visibility = Visibility.Hidden;
    }

    public void SetBottomLabelText(string text)
    {
        bottomLabel.text = text;
        bottomLabel.style.visibility = Visibility.Visible;
    }
}
