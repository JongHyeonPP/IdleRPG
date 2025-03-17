using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionInfoUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    [SerializeField] private AbilityTable abilityTable;
    private void Awake()
    {

        root = GetComponent<UIDocument>().rootVisualElement;

        root.style.display = DisplayStyle.None;

        InitUI();
    }
    public void ShowOption()
    {
        root.style.display = DisplayStyle.Flex;
    }
    private void InitUI()
    {
        var exitButton = root.Q<Button>("ExitButton");
        var abilityLabels = new Dictionary<string, Label>
        {
             { "�߰� ���ݷ�", root.Q<Label>("PowerLabel") },
             { "�߰� ü��", root.Q<Label>("MaxHpLabel") },
             { "ũ��Ƽ�� ������", root.Q<Label>("CriticalDamageLabel") }
         };

        foreach (var abilityData in abilityTable.Abilities)
        {
            if (abilityLabels.TryGetValue(abilityData.AbilityName, out Label label))
            {
                string valuesText = string.Join("/", abilityData.Values);
                label.text = $"{abilityData.AbilityName}(%) : {valuesText}";
            }
        }
        exitButton.clicked += HideUI;
    }
    private void HideUI()
    {
        root.style.display = DisplayStyle.None;
    }
}
