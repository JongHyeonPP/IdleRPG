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


        // 아무 곳이나 누르면 닫히도록 포인터 다운 등록
        //overlayElement.RegisterCallback<PointerDownEvent>(OnClose);

        // 시작 시 숨김
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
