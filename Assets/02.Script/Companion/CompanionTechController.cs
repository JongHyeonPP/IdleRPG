using UnityEngine;

public class CompanionTechController : MonoBehaviour
{
    private AppearanceController _appearanceController;
    [SerializeField] SpriteRenderer _footHoldRenderer;
    [SerializeField] int techIndex_0;
    [SerializeField] int techIndex_1;
    private void Start()
    {
        _appearanceController = GetComponent<AppearanceController>();
        PlayerBroker.CompanionTechRenderSet += CompanionTechRenderSet;
        PlayerBroker.CompanionTechRgbSet += CompanionTechRgbSet;
    }
    public void CompanionTechRgbSet(float targetValue, (int,int) techIndex)
    {
        if (techIndex_0 != techIndex.Item1 || techIndex_1 != techIndex.Item2)
            return;
        _appearanceController.SetRGB(targetValue);
        _footHoldRenderer.color = new Color(targetValue, targetValue, targetValue, 1f);
    }
    private void CompanionTechRenderSet(int companionIndex)
    {
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[companionIndex].companionStatus;
        CompanionTechData techData = CompanionManager.instance.GetCompanionTechData(companionIndex, techIndex_0, techIndex_1);
        _appearanceController.SetAppearance(techData.appearanceData);

    }
}