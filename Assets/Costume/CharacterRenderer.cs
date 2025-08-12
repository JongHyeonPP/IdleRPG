using System.Collections.Generic;
using UnityEngine;



public class CharacterRenderer : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private AppearanceController _appearanceController;

    // �� ������ SpriteRenderer
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

    private Dictionary<BodyPart, SpriteRenderer> _partRenderers;    // ������ SpriteRenderer�� ������ ��ųʸ�
    private Sprite _savedHairSprite;                                // ��� ����� �Ӹ� �������� ������ ����
    private AppearanceData _currentAppearanceData;

    private void Awake()
    {
        // AppearanceController Ȯ��
        if (_appearanceController == null)
        {
            _appearanceController = GetComponent<AppearanceController>();
            if (_appearanceController == null)
            {
                _appearanceController = GetComponentInChildren<AppearanceController>();
            }
        }

        // �⺻ ��ųʸ� �ʱ�ȭ
        InitializeRendererDictionary();
    }

    private void Start()
    {
        if (_appearanceController == null) return;

        // AppearanceController���� ������ ���� ��������
        SetRenderersFromAppearanceController();

        // ��ųʸ� ������Ʈ
        UpdateRendererDictionary();

        // AppearanceData�� ��������
        _currentAppearanceData = _appearanceController.GetCurrentAppearanceData();
        _appearanceController.SetAppearance(_currentAppearanceData);
    }

    /// <summary>
    /// AppearanceController���� ������ ���� ��������
    /// </summary>
    private void SetRenderersFromAppearanceController()
    {
        // Body ���� ������
        _headRenderer = _appearanceController.BodyHead;
        _faceHairRenderer = _appearanceController.BodyFaceHair;
        _hairRenderer = _appearanceController.BodyHair;
        _bodyRenderer = _appearanceController.BodyBody;
        _armRRenderer = _appearanceController.BodyRightArm;
        _armLRenderer = _appearanceController.BodyLeftArm;
        _pantRRenderer = _appearanceController.BodyRightFoot;
        _pantLRenderer = _appearanceController.BodyLeftFoot;

        // Clothes ���� ������
        _helmetRenderer = _appearanceController.ClothHelmet;
    }

    /// <summary>
    /// ������ ��ųʸ� �ʱ�ȭ
    /// </summary>
    private void InitializeRendererDictionary()
    {
        _partRenderers = new Dictionary<BodyPart, SpriteRenderer>();
    }

    /// <summary>
    /// ������ ��ųʸ� ������Ʈ
    /// </summary>
    private void UpdateRendererDictionary()
    {
        _partRenderers.Clear();

        // �� ���� ������ ����
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
    /// Ư�� ������ ������ ����
    /// </summary>
    public void AppItem(BodyPart part, Sprite sprite, Color color)
    {
        // �ݵ�� �������� �ʱ�ȭ�Ǿ� �־�� ��
        if (_partRenderers.Count == 0)
        {
            UpdateRendererDictionary();
        }

        // ����� ������ ��, �Ӹ� �������� �����ؾ� ��
        if (part == BodyPart.Helmet)
        {
            // ���� �Ӹ� �������� ������ ����
            if (_hairRenderer != null && _hairRenderer.sprite != null)
            {
                _savedHairSprite = _hairRenderer.sprite;
                _hairRenderer.sprite = null;  // �Ӹ� ������ ����
            }

            // ��� ������ ����
            if (_helmetRenderer != null)
            {
                _helmetRenderer.sprite = sprite;
                _helmetRenderer.color = color;
            }
        }
        else if (part == BodyPart.Hair || part == BodyPart.FaceHair)
        {
            // �� �� �� �������� �����Ϸ��� ����� ���� �ؾ� ��
            // UnequipHelmet();  // ��� ����
            _helmetRenderer.sprite = null;

            // ��� ������ ����
            if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }
        else
        {
            // �ٸ� ������ �ܼ� ����
            if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }

        // AppearanceData�� �Բ� ������Ʈ
        UpdateAppearanceData(part, sprite, color);
    }

    /// <summary>
    /// AppearanceData ������Ʈ (AppearanceController�� ���� ���¿� ����ȭ)
    /// </summary>
    private void UpdateAppearanceData(BodyPart part, Sprite sprite, Color color)
    {
        if (_appearanceController == null || _currentAppearanceData == null) return;

        // �� AppearanceData ����
        AppearanceData updatedData = _appearanceController.GetCurrentAppearanceData();


        // �ش� ���� ������Ʈ
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

                // ��� ����� �Ӹ� ��������Ʈ ����
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

        // AppearanceController ������Ʈ
        //_appearanceController.SetAppearance(updatedData);
        _currentAppearanceData = updatedData;
    }

    /// <summary>
    /// ����� ���� �� �Ӹ� ������ ����
    /// </summary>
    public void UnequipHelmet()
    {
        if (_helmetRenderer != null)
        {
            // ��� ������ ����
            _helmetRenderer.sprite = null;
        }

        // ����� �Ӹ� ������ ����
        if (_savedHairSprite != null && _hairRenderer != null)
        {
            _hairRenderer.sprite = _savedHairSprite;
            _savedHairSprite = null;  // ���� �� ����� �Ӹ� ������ �ʱ�ȭ
        }

        // AppearanceData�� �Բ� ������Ʈ
        if (_appearanceController != null && _currentAppearanceData != null)
        {
            AppearanceData updatedData = ScriptableObject.CreateInstance<AppearanceData>();
            CopyAppearanceData(_currentAppearanceData, updatedData);
            updatedData.clothHelmetSprite = null;

            // AppearanceController ������Ʈ
            _appearanceController.SetAppearance(updatedData);
            _currentAppearanceData = updatedData;
        }
    }

    /// <summary>
    /// Ư�� ���� ������ �ʱ�ȭ
    /// </summary>
    public void ResetPartItem(BodyPart part)
    {
        // ������ �ʱ�ȭ
        if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
        {
            renderer.sprite = null;
        }

        // AppearanceData�� �Բ� ������Ʈ
        if (_appearanceController != null && _currentAppearanceData != null)
        {
            AppearanceData updatedData = ScriptableObject.CreateInstance<AppearanceData>();
            CopyAppearanceData(_currentAppearanceData, updatedData);

            // �ش� ���� ������Ʈ
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

            // AppearanceController ������Ʈ
          //  _appearanceController.SetAppearance(updatedData);
            _currentAppearanceData = updatedData;
        }
    }

    /// <summary>
    /// ��� ���� ������ �ʱ�ȭ�ϰ� �⺻ �ٵ� ����
    /// </summary>
    public void ResetAllItems()
    {
        // ��� ������ �ʱ�ȭ
        foreach (var renderer in _partRenderers.Values)
        {
            if (renderer != null)
            {
                renderer.sprite = null;
            }
        }

        // AppearanceData�� �Բ� �ʱ�ȭ - �⺻ �ٵ� ����
        if (_appearanceController != null)
        {
            if (_appearanceController.HasDefaultAppearanceData())
            {
                // �⺻ AppearanceData�� ������ ���
                _appearanceController.SetDefaultAppearacne();
                _currentAppearanceData = _appearanceController.GetCurrentAppearanceData();
            }
            else
            {
                // ���ο� �⺻ ���� ����
                SetDefaultAppearance();
            }
        }
    }

    /// <summary>
    /// �⺻ �������� �ʱ�ȭ (�ٵ� ����)
    /// </summary>
    public void SetDefaultAppearance()
    {
        // �� AppearanceData ����
        AppearanceData defaultData = ScriptableObject.CreateInstance<AppearanceData>();

        if (_currentAppearanceData != null)
        {
            // �ٵ� ���� ��������Ʈ ����
            defaultData.bodyBodySprite = _currentAppearanceData.bodyBodySprite;
            defaultData.bodyLeftArmSprite = _currentAppearanceData.bodyLeftArmSprite;
            defaultData.bodyRightArmSprite = _currentAppearanceData.bodyRightArmSprite;
            defaultData.bodyLeftFootSprite = _currentAppearanceData.bodyLeftFootSprite;
            defaultData.bodyRightFootSprite = _currentAppearanceData.bodyRightFootSprite;

            // �� ���� ���� ����
            defaultData.bodyBackEyeSprite = _currentAppearanceData.bodyBackEyeSprite;
            defaultData.bodyFrontEyeSprite = _currentAppearanceData.bodyFrontEyeSprite;
            defaultData.eyeColor = _currentAppearanceData.eyeColor;

            // �Ӹ�/��� ���� ����
            defaultData.hairColor = _currentAppearanceData.hairColor;
        }

        // AppearanceController ������Ʈ
        _appearanceController.SetAppearance(defaultData);
        _currentAppearanceData = defaultData;
    }

    /// <summary>
    /// AppearanceData �� ���� �޼���
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