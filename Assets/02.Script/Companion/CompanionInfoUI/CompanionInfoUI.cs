using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionInfoUI : MonoBehaviour
{
    private CompanionManager companionManager;
    public VisualElement root { get; private set; }
    public VisualElement[] renderTextureArr;
    private void Awake()
    {
        companionManager = CompanionManager.instance;
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt=>OnExitButtonClick());
        CategoriButtonInit();
        VisualElement renderTextureparent = root.Q<VisualElement>("RenderTextureParent");
        renderTextureArr = new VisualElement[renderTextureparent.childCount];
        for (int i = 0; i < renderTextureparent.childCount; i++)
        {
            renderTextureArr[i] = renderTextureparent.ElementAt(i);
        }
        
    }

    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    internal void ActiveUI(int companionIndex)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        for (int i = 0; i < renderTextureArr.Length; i++)
        {
            if (companionIndex == i)
            {
                renderTextureArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                renderTextureArr[i].style.display = DisplayStyle.None;
            }
        }
        VisualElement passiveParent = root.Q<VisualElement>("PassiveParent");
        for (int i = 0; i < passiveParent.childCount; i++)
        {
            VisualElement parentSlot = passiveParent.ElementAt(i);
            SkillData skillData = companionManager.companionStatusArr[companionIndex].companionSkillArr[i];
            VisualElement iconSprite = parentSlot.Q<VisualElement>("IconSprite");
            Label nameLabel = parentSlot.Q<Label>("NameLabel");
            nameLabel.text = skillData.skillName;
            Label effectLabel = parentSlot.Q<Label>("EffectLabel");
            effectLabel.text = skillData.simple;
            Label levelLabel = parentSlot.Q<Label>("LevelLabel");
            //
            Label cloverLabel = parentSlot.Q<Label>("CloverLabel");
            
            Label fragmentLabel = parentSlot.Q<Label>("FragmentLabel");
        
        }
    }
    private void CategoriButtonInit()
    {
        Button statusButton = root.Q<Button>("StatusButton");
        Button promoteButton = root.Q<Button>("PromoteButton");
        statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(true);
        });
        promoteButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(false);
        });
        ShowCategori(true);

        void ShowCategori(bool isStatus)
        {
            statusButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            promoteButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
            root.Q<VisualElement>("StatusPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<VisualElement>("PromotePanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
