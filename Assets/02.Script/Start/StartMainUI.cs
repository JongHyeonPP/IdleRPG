using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class StartMainUI : MonoBehaviour
{
    private Label _loadingLabel;
    private Label _bottomLabel;
    private void Awake()
    {
        // UI Element 설정
        var root = GetComponent<UIDocument>().rootVisualElement;
        _loadingLabel = root.Q<Label>("LoadingLabel");
        _bottomLabel = root.Q<Label>("BottomLabel");

        // UI 초기 상태 설정
        _loadingLabel.style.visibility = Visibility.Hidden;
        _bottomLabel.style.visibility = Visibility.Hidden;
    }
    //메인 씬을 로드 중이라고 띄운다.
    public void LoadingLebelSet()
    {
        _loadingLabel.style.visibility = Visibility.Visible;
        _bottomLabel.style.visibility = Visibility.Hidden;
    }
    //진행 상황을 보여주는 좌측 하단 UI에 정보를 넣는다.
    public void SetBottomLabelText(string text)
    {
        _bottomLabel.text = text;
        _bottomLabel.style.visibility = Visibility.Visible;
    }
}
