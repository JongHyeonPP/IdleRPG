using UnityEngine;
using UnityEngine.UIElements;

public class SettingUI : MonoBehaviour
{
    Button _saveButton;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // SaveButton ã��
        _saveButton = root.Q<Button>("SaveButton");
        // Ŭ�� �̺�Ʈ �ݹ� ����
        if (_saveButton != null)
        {
            _saveButton.clicked += OnSaveButtonClick;
            _saveButton.clicked += ()=> Debug.Log("LOg������");
        }

    }

    // SaveButton Ŭ�� �� ȣ��� �޼���
    void OnSaveButtonClick()
    {
        NetworkBroker.SaveServerData();
    }
}
