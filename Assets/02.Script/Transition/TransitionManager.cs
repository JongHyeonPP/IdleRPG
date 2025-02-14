using EnumCollection;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    private BattleType currentBattleType = BattleType.Default;
    public StoryManager storyManager;

    private void OnEnable()
    {
        BattleBroker.SwitchToStory += SwitchToStoryMode;
        BattleBroker.SwitchBattle += SwitchToBattleMode;
       
    }

    private void Start()
    {
        // SwitchToStoryMode(1);
        BattleBroker.SwitchToStory?.Invoke(1);
    }

    public void SwitchToStoryMode(int index)
    {
        if (currentBattleType == BattleType.Story) return;

        foreach (var battleUI in MediatorManager<IBattleUI>.GetRegisteredObjects())
        {
            battleUI.DeactivateBattleMode();
        }
        if (index == 1)
        {
            //StartCoroutine(storyManager.FadeEffect());
            StoryBroker.StoryModeStart?.Invoke(1);
        }


        currentBattleType = BattleType.Story;
    }

    public void SwitchToBattleMode()
    {
        if (currentBattleType == BattleType.Boss || currentBattleType == BattleType.Default) return;


        foreach (var battleUI in MediatorManager<IBattleUI>.GetRegisteredObjects())
        {
            battleUI.ActivateBattleMode();
        }
      //  storyManager.HideStoryUI();
       // StoryBroker.EndStoryMode?.Invoke();
        currentBattleType = BattleType.Default;
    }

}
