using EnumCollection;
using UnityEngine;

public abstract class StoryChapter : MonoBehaviour
{
   protected GameObject objectParent;
    public StageInfo nextStage;
    public virtual StoryTalker[] LocalTalkers => null;

    // 이 챕터에서 사용하는 모델
    public virtual GameObject[] LocalModels => null;

    public virtual StoryRenderType[] GetRequiredRenderTypes() => null;

    public abstract void BuildActions(StoryRunner runner);

    private void Awake()
    {
        // objectParent가 비어 있으면 0번째 자식 자동 할당
        if (objectParent == null && transform.childCount > 0)
            objectParent = transform.GetChild(0).gameObject;
    }

    public void SetChapterActive(bool active)
    {
        if (objectParent != null)
            objectParent.SetActive(active);
    }
}
