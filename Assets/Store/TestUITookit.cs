using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 그냥 ui툴킷 연습...
/// </summary>
public class TestUITookit : MonoBehaviour
{
    private Button _drawBtn;                // Draw 버튼
    private Label _statusLabel;             // 상태를 표시하는 라벨
    private Slider _valueSlider;            // 값 조정을 위한 슬라이더
    private Toggle _toggleOption;           // 활성/비활성 토글

    private void OnEnable()
    {
        // UIDocument에서 루트 요소 가져오기
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Draw 버튼 요소 초기화 및 클릭 이벤트 등록
        _drawBtn = root.Q<Button>("DrawBtn");
        _drawBtn?.RegisterCallback<ClickEvent>(evt => UpdateLabel("Draw 버튼이 클릭되었습니다."));

        // 상태 표시 라벨 요소 초기화
        _statusLabel = root.Q<Label>("StatusLabel");

        // 슬라이더 요소 초기화 및 값 변경 이벤트 등록
        _valueSlider = root.Q<Slider>("ValueSlider");
        _valueSlider?.RegisterValueChangedCallback(evt => UpdateLabel($"슬라이더 값: {evt.newValue:F2}"));

        // 토글 요소 초기화 및 값 변경 이벤트 등록
        _toggleOption = root.Q<Toggle>("ToggleOption");
        _toggleOption?.RegisterValueChangedCallback(evt => UpdateLabel(evt.newValue ? "토글이 활성화되었습니다." : "토글이 비활성화되었습니다."));
    }

    // 상태 라벨을 업데이트하는 메서드
    private void UpdateLabel(string newText)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = newText;
        }
    }

    private void OnDisable()
    {
        // 이벤트 해제
        _drawBtn?.UnregisterCallback<ClickEvent>(evt => UpdateLabel("Draw 버튼이 클릭되었습니다."));
        _valueSlider?.UnregisterValueChangedCallback(evt => UpdateLabel($"슬라이더 값: {evt.newValue:F2}"));
        _toggleOption?.UnregisterValueChangedCallback(evt => UpdateLabel(evt.newValue ? "토글이 활성화되었습니다." : "토글이 비활성화되었습니다."));
    }
}
