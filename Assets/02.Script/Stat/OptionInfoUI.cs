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
             { "추가 공격력", root.Q<Label>("PowerLabel") },
             { "추가 체력", root.Q<Label>("MaxHpLabel") },
             { "크리티컬 데미지", root.Q<Label>("CriticalDamageLabel") }
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
