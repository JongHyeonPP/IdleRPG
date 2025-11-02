using UnityEngine;

/// <summary>
/// 특정 승급 테크(라인/단계)에 해당하는 외형을 제어하는 컴포넌트.
/// - PlayerBroker 이벤트를 구독해: 
///   1) 외형 스킨 적용(CompanionTechRenderSet) 
///   2) 명암/밝기(RGB) 조정(CompanionTechRgbSet)
/// - 이 오브젝트가 담당하는 테크(line/step)와 일치할 때만 반응한다.
/// </summary>
public class CompanionTechController : MonoBehaviour
{
    private AppearanceController _appearanceController;      // 외형(스킨/팔레트 등) 적용 컨트롤러

    [SerializeField] SpriteRenderer _footHoldRenderer;       // 바닥(받침) 스프라이트 (RGB 동기화용)
    [SerializeField] int techIndex_0;                        // 테크 라인(0=기본, 1/2/3=분기)
    [SerializeField] int techIndex_1;                        // 테크 단계(0/1)

    private void Start()
    {
        _appearanceController = GetComponent<AppearanceController>();

        // 이벤트 구독: 외형 교체 / RGB 값 변경
        PlayerBroker.CompanionTechRenderSet += CompanionTechRenderSet;
        PlayerBroker.CompanionTechRgbSet += CompanionTechRgbSet;
    }

    /// <summary>
    /// 테크 RGB 조정 요청. 이 컨트롤러의 (라인,단계)와 일치할 때만 적용.
    /// targetValue는 0~1 범위의 회색톤/명암 값으로 가정.
    /// </summary>
    public void CompanionTechRgbSet(float targetValue, (int, int) techIndex)
    {
        if (techIndex_0 != techIndex.Item1 || techIndex_1 != techIndex.Item2)
            return;

        _appearanceController.SetRGB(targetValue);
        _footHoldRenderer.color = new Color(targetValue, targetValue, targetValue, 1f);
    }

    /// <summary>
    /// 외형 스킨 적용 요청. 동료 인덱스를 받아 해당 동료의 테크 데이터에서 스킨을 가져와 적용.
    /// </summary>
    private void CompanionTechRenderSet(int companionIndex)
    {
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[companionIndex].companionStatus;
        var techData = CompanionManager.instance.GetCompanionTechData(companionIndex, techIndex_0, techIndex_1);
        _appearanceController.SetAppearance(techData.appearanceData);
    }

    // 참고: OnDestroy에서 이벤트 구독 해제를 권장합니다.
    // (메서드 추가 원치 않으시면 유지하시고, 누수 방지를 위해 추후 정리 권장)
    // private void OnDestroy() {
    //     PlayerBroker.CompanionTechRenderSet -= CompanionTechRenderSet;
    //     PlayerBroker.CompanionTechRgbSet    -= CompanionTechRgbSet;
    // }
}
