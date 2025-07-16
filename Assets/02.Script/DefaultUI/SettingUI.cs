using UnityEngine;
using UnityEngine.UIElements;

public class SettingUI : MonoBehaviour
{
    Button _saveButton;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // SaveButton 찾기
        _saveButton = root.Q<Button>("SaveButton");
        // 클릭 이벤트 콜백 연결
        if (_saveButton != null)
        {
            _saveButton.clicked += OnSaveButtonClick;
            _saveButton.clicked += ()=> Debug.Log("LOg당히히");
        }

    }

    // SaveButton 클릭 시 호출될 메서드
    void OnSaveButtonClick()
    {
        NetworkBroker.SaveServerData();
    }
}
