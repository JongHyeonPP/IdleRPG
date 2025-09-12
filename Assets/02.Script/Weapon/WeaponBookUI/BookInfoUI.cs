using UnityEngine;
using UnityEngine.UIElements;

public class BookInfoUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label descriptionLabel;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        descriptionLabel = root.Q<Label>("First");


        // �ƹ� ���̳� ������ �������� ������ �ٿ� ���
        //overlayElement.RegisterCallback<PointerDownEvent>(OnClose);

        // ���� �� ����
        root.style.display = DisplayStyle.None;
    }

    public void Show(string descriptionText)
    {
        if (descriptionLabel != null)
            descriptionLabel.text = descriptionText;

        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root,true);
    }

    public void Hide()
    {
        UIBroker.InactiveCurrentUI();
    }

    //private void OnClose(PointerDownEvent evt)
    //{
    //    Hide();
    //    evt.StopPropagation();
    //}
}
