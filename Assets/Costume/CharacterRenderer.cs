using System.Collections.Generic;
using UnityEngine;



public class CharacterRenderer : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private AppearanceController _appearanceController;

    // 각 부위별 SpriteRenderer
    [Header("Character Parts")]
    [SerializeField] private SpriteRenderer _headRenderer;
    [SerializeField] private SpriteRenderer _faceHairRenderer;
    [SerializeField] private SpriteRenderer _hairRenderer;
    [SerializeField] private SpriteRenderer _helmetRenderer;

    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private SpriteRenderer _armRRenderer;
    [SerializeField] private SpriteRenderer _armLRenderer;
    [SerializeField] private SpriteRenderer _pantRRenderer;
    [SerializeField] private SpriteRenderer _pantLRenderer;

    private Dictionary<BodyPart, SpriteRenderer> _partRenderers;    // 부위별 SpriteRenderer을 저장할 딕셔너리
    private Sprite _savedHairSprite;                                // 헬멧 착용시 머리 아이템을 저장할 변수
    private AppearanceData _currentAppearanceData;

    private void Awake()
    {
        // AppearanceController 확인
        if (_appearanceController == null)
        {
            _appearanceController = GetComponent<AppearanceController>();
            if (_appearanceController == null)
            {
                _appearanceController = GetComponentInChildren<AppearanceController>();
            }
        }

        // 기본 딕셔너리 초기화
        InitializeRendererDictionary();
    }

    private void Start()
    {
        if (_appearanceController == null) return;

        // AppearanceController에서 렌더러 참조 가져오기
        SetRenderersFromAppearanceController();

        // 딕셔너리 업데이트
        UpdateRendererDictionary();

        // AppearanceData도 가져오기
        _currentAppearanceData = _appearanceController.GetCurrentAppearanceData();
        _appearanceController.SetAppearance(_currentAppearanceData);
    }

    /// <summary>
    /// AppearanceController에서 렌더러 참조 가져오기
    /// </summary>
    private void SetRenderersFromAppearanceController()
    {
        // Body 관련 렌더러
        _headRenderer = _appearanceController.BodyHead;
        _faceHairRenderer = _appearanceController.BodyFaceHair;
        _hairRenderer = _appearanceController.BodyHair;
        _bodyRenderer = _appearanceController.BodyBody;
        _armRRenderer = _appearanceController.BodyRightArm;
        _armLRenderer = _appearanceController.BodyLeftArm;
        _pantRRenderer = _appearanceController.BodyRightFoot;
        _pantLRenderer = _appearanceController.BodyLeftFoot;

        // Clothes 관련 렌더러
        _helmetRenderer = _appearanceController.ClothHelmet;
    }

    /// <summary>
    /// 렌더러 딕셔너리 초기화
    /// </summary>
    private void InitializeRendererDictionary()
    {
        _partRenderers = new Dictionary<BodyPart, SpriteRenderer>();
    }

    /// <summary>
    /// 렌더러 딕셔너리 업데이트
    /// </summary>
    private void UpdateRendererDictionary()
    {
        _partRenderers.Clear();

        // 각 부위 렌더러 매핑
        _partRenderers.Add(BodyPart.Head, _headRenderer);
        _partRenderers.Add(BodyPart.FaceHair, _faceHairRenderer);
        _partRenderers.Add(BodyPart.Hair, _hairRenderer);
        _partRenderers.Add(BodyPart.Helmet, _helmetRenderer);
        _partRenderers.Add(BodyPart.Body, _bodyRenderer);
        _partRenderers.Add(BodyPart.Arm_R, _armRRenderer);
        _partRenderers.Add(BodyPart.Arm_L, _armLRenderer);
        _partRenderers.Add(BodyPart.Pant_R, _pantRRenderer);
        _partRenderers.Add(BodyPart.Pant_L, _pantLRenderer);
    }

    /// <summary>
    /// 특정 부위에 아이템 적용
    /// </summary>
    public void AppItem(BodyPart part, Sprite sprite, Color color)
    {
        // 반드시 렌더러가 초기화되어 있어야 함
        if (_partRenderers.Count == 0)
        {
            UpdateRendererDictionary();
        }

        // 헬멧을 적용할 때, 머리 아이템을 제거해야 함
        if (part == BodyPart.Helmet)
        {
            // 현재 머리 아이템이 있으면 저장
            if (_hairRenderer != null && _hairRenderer.sprite != null)
            {
                _savedHairSprite = _hairRenderer.sprite;
                _hairRenderer.sprite = null;  // 머리 아이템 제거
            }

            // 헬멧 아이템 적용
            if (_helmetRenderer != null)
            {
                _helmetRenderer.sprite = sprite;
                _helmetRenderer.color = color;
            }
        }
        else if (part == BodyPart.Hair || part == BodyPart.FaceHair)
        {
            // 헤어나 얼굴 털 아이템을 적용하려면 헬멧을 벗게 해야 함
            // UnequipHelmet();  // 헬멧 벗기
            _helmetRenderer.sprite = null;

            // 헤어 아이템 적용
            if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }
        else
        {
            // 다른 부위는 단순 적용
            if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }

        // AppearanceData도 함께 업데이트
        UpdateAppearanceData(part, sprite, color);
    }

    /// <summary>
    /// AppearanceData 업데이트 (AppearanceController의 현재 상태와 동기화)
    /// </summary>
    private void UpdateAppearanceData(BodyPart part, Sprite sprite, Color color)
    {
        if (_appearanceController == null || _currentAppearanceData == null) return;

        // 새 AppearanceData 생성
        AppearanceData updatedData = _appearanceController.GetCurrentAppearanceData();


        // 해당 부위 업데이트
        switch (part)
        {
            case BodyPart.Head:
                updatedData.bodyHeadSprite = sprite;
                break;
            case BodyPart.FaceHair:
                updatedData.bodyFaceHairSprite = sprite;
                updatedData.hairColor = color;
                break;
            case BodyPart.Hair:
                updatedData.bodyHairSprite = sprite;
                updatedData.hairColor = color;
                break;
            case BodyPart.Helmet:
                updatedData.clothHelmetSprite = sprite;

                // 헬멧 착용시 머리 스프라이트 제거
                if (sprite != null)
                {
                    _savedHairSprite = updatedData.bodyHairSprite;
                    //updatedData.bodyHairSprite = null;
                }
                break;
            case BodyPart.Body:
                updatedData.clothBodySprite = sprite;
                break;
            case BodyPart.Arm_R:
                updatedData.clothRightArmSprite = sprite;
                break;
            case BodyPart.Arm_L:
                updatedData.clothLeftArmSprite = sprite;
                break;
            case BodyPart.Pant_R:
                updatedData.clothRightFootSprite = sprite;
                break;
            case BodyPart.Pant_L:
                updatedData.clothLeftFootSprite = sprite;
                break;
        }

        // AppearanceController 업데이트
        //_appearanceController.SetAppearance(updatedData);
        _currentAppearanceData = updatedData;
    }

    /// <summary>
    /// 헬멧을 벗을 때 머리 아이템 복원
    /// </summary>
    public void UnequipHelmet()
    {
        if (_helmetRenderer != null)
        {
            // 헬멧 아이템 제거
            _helmetRenderer.sprite = null;
        }

        // 저장된 머리 아이템 복원
        if (_savedHairSprite != null && _hairRenderer != null)
        {
            _hairRenderer.sprite = _savedHairSprite;
            _savedHairSprite = null;  // 복원 후 저장된 머리 아이템 초기화
        }

        // AppearanceData도 함께 업데이트
        if (_appearanceController != null && _currentAppearanceData != null)
        {
            AppearanceData updatedData = ScriptableObject.CreateInstance<AppearanceData>();
            CopyAppearanceData(_currentAppearanceData, updatedData);
            updatedData.clothHelmetSprite = null;

            // AppearanceController 업데이트
            _appearanceController.SetAppearance(updatedData);
            _currentAppearanceData = updatedData;
        }
    }

    /// <summary>
    /// 특정 부위 아이템 초기화
    /// </summary>
    public void ResetPartItem(BodyPart part)
    {
        // 렌더러 초기화
        if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
        {
            renderer.sprite = null;
        }

        // AppearanceData도 함께 업데이트
        if (_appearanceController != null && _currentAppearanceData != null)
        {
            AppearanceData updatedData = ScriptableObject.CreateInstance<AppearanceData>();
            CopyAppearanceData(_currentAppearanceData, updatedData);

            // 해당 부위 업데이트
            switch (part)
            {
                case BodyPart.Head:
                    updatedData.bodyHeadSprite = null;
                    break;
                case BodyPart.FaceHair:
                    updatedData.bodyFaceHairSprite = null;
                    break;
                case BodyPart.Hair:
                    updatedData.bodyHairSprite = null;
                    break;
                case BodyPart.Helmet:
                    _hairRenderer.sprite = _savedHairSprite;
                    updatedData.clothHelmetSprite = null;
                    break;
                case BodyPart.Body:
                    updatedData.clothBodySprite = null;
                    break;
                case BodyPart.Arm_R:
                    updatedData.clothRightArmSprite = null;
                    break;
                case BodyPart.Arm_L:
                    updatedData.clothLeftArmSprite = null;
                    break;
                case BodyPart.Pant_R:
                    updatedData.clothRightFootSprite = null;
                    break;
                case BodyPart.Pant_L:
                    updatedData.clothLeftFootSprite = null;
                    break;
            }

            // AppearanceController 업데이트
          //  _appearanceController.SetAppearance(updatedData);
            _currentAppearanceData = updatedData;
        }
    }

    /// <summary>
    /// 모든 부위 아이템 초기화하고 기본 바디 유지
    /// </summary>
    public void ResetAllItems()
    {
        // 모든 렌더러 초기화
        foreach (var renderer in _partRenderers.Values)
        {
            if (renderer != null)
            {
                renderer.sprite = null;
            }
        }

        // AppearanceData도 함께 초기화 - 기본 바디 유지
        if (_appearanceController != null)
        {
            if (_appearanceController.HasDefaultAppearanceData())
            {
                // 기본 AppearanceData가 있으면 사용
                _appearanceController.SetDefaultAppearacne();
                _currentAppearanceData = _appearanceController.GetCurrentAppearanceData();
            }
            else
            {
                // 새로운 기본 상태 생성
                SetDefaultAppearance();
            }
        }
    }

    /// <summary>
    /// 기본 외형으로 초기화 (바디 유지)
    /// </summary>
    public void SetDefaultAppearance()
    {
        // 새 AppearanceData 생성
        AppearanceData defaultData = ScriptableObject.CreateInstance<AppearanceData>();

        if (_currentAppearanceData != null)
        {
            // 바디 관련 스프라이트 복사
            defaultData.bodyBodySprite = _currentAppearanceData.bodyBodySprite;
            defaultData.bodyLeftArmSprite = _currentAppearanceData.bodyLeftArmSprite;
            defaultData.bodyRightArmSprite = _currentAppearanceData.bodyRightArmSprite;
            defaultData.bodyLeftFootSprite = _currentAppearanceData.bodyLeftFootSprite;
            defaultData.bodyRightFootSprite = _currentAppearanceData.bodyRightFootSprite;

            // 눈 관련 설정 복사
            defaultData.bodyBackEyeSprite = _currentAppearanceData.bodyBackEyeSprite;
            defaultData.bodyFrontEyeSprite = _currentAppearanceData.bodyFrontEyeSprite;
            defaultData.eyeColor = _currentAppearanceData.eyeColor;

            // 머리/헤어 색상 유지
            defaultData.hairColor = _currentAppearanceData.hairColor;
        }

        // AppearanceController 업데이트
        _appearanceController.SetAppearance(defaultData);
        _currentAppearanceData = defaultData;
    }

    /// <summary>
    /// AppearanceData 값 복사 메서드
    /// </summary>
    private void CopyAppearanceData(AppearanceData source, AppearanceData target)
    {
        if (source == null || target == null) return;

        // Clothes
        target.clothBack_0Sprite = source.clothBack_0Sprite;
        target.clothBack_1Sprite = source.clothBack_1Sprite;
        target.clothHelmetSprite = source.clothHelmetSprite;
        target.clothLeftArmSprite = source.clothLeftArmSprite;
        target.clothRightArmSprite = source.clothRightArmSprite;
        target.clothLeftShoulderSprite = source.clothLeftShoulderSprite;
        target.clothRightShoulderSprite = source.clothRightShoulderSprite;
        target.clothLeftFootSprite = source.clothLeftFootSprite;
        target.clothRightFootSprite = source.clothRightFootSprite;
        target.clothBodySprite = source.clothBodySprite;
        target.clothBodyArmorSprite = source.clothBodyArmorSprite;

        // Body
        target.bodyBodySprite = source.bodyBodySprite;
        target.bodyHeadSprite = source.bodyHeadSprite;
        target.bodyHairSprite = source.bodyHairSprite;
        target.bodyFaceHairSprite = source.bodyFaceHairSprite;
        target.bodyBackEyeSprite = source.bodyBackEyeSprite;
        target.bodyFrontEyeSprite = source.bodyFrontEyeSprite;
        target.bodyLeftArmSprite = source.bodyLeftArmSprite;
        target.bodyRightArmSprite = source.bodyRightArmSprite;
        target.bodyLeftFootSprite = source.bodyLeftFootSprite;
        target.bodyRightFootSprite = source.bodyRightFootSprite;

        // Colors
        target.hairColor = source.hairColor;
        target.eyeColor = source.eyeColor;
    }
}