using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillListController : LVItemController
{
    [SerializeField] SkillInfoUI skillInfoUI;
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        SkillDataSet skillDataSet = item as SkillDataSet;
        for (int i = 0; i < skillDataSet.dataSet.Count; i++)
        {
            SkillData skillData = skillDataSet.dataSet[i];
            VisualElement dataRootParent = element.Q<VisualElement>($"SkillData_{i}");
            Label nameLabel = dataRootParent.Q<Label>("NameLabel");
            nameLabel.text = skillData.name;
            ProgressBar containProgressBar = dataRootParent.Q<ProgressBar>("ContainProgressBar");
            GameData gameData = StartBroker.GetGameData();
            if (!gameData.skillCount.TryGetValue(skillData.name, out int skillCount))
            {
                skillCount = 0;
            }
            int requirePiece = PriceManager.instance.GetRequirePiece_Skill(skillData.rarity, skillCount);
            containProgressBar.title = $"{skillCount}/{requirePiece}";
            VisualElement clickVe = dataRootParent.Q<VisualElement>("ClickVe");
            clickVe.RegisterCallback<ClickEvent>(evt =>
            {
                skillInfoUI.ActiveUI(skillData);
            });
            clickVe.style.backgroundImage = new(skillData.icon);
        }
    }
}
