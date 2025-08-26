using System.Collections.Generic;
using UnityEngine;

public enum BodyPart
{
    Head,
    FaceHair,
    Hair,
    Helmet,
    Body,
    Arm_R,
    Arm_L,
    Pant_R,
    Pant_L
}

public class CostumeCharacterRenderer : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private AppearanceController _uiAppearanceController;
    [SerializeField] private AppearanceController _gameAppearanceController;

    // Character Parts
    private SpriteRenderer _headRenderer;
    private SpriteRenderer _faceHairRenderer;
    private SpriteRenderer _hairRenderer;
    private SpriteRenderer _helmetRenderer;
    private SpriteRenderer _bodyRenderer;
    private SpriteRenderer _armRRenderer;
    private SpriteRenderer _armLRenderer;
    private SpriteRenderer _pantRRenderer;
    private SpriteRenderer _pantLRenderer;

    private Dictionary<BodyPart, SpriteRenderer> _partRenderers;
    private Sprite _savedHairSprite;
    private AppearanceData _curAppearanceData;

    private void Awake()
    {
        if (_uiAppearanceController == null)
            _uiAppearanceController = GetComponent<AppearanceController>() ?? GetComponentInChildren<AppearanceController>();

        if (_gameAppearanceController == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _gameAppearanceController = player.GetComponent<AppearanceController>();
        }

        Init();
    }

    public void Init()
    {
        if (_uiAppearanceController == null) return;

        SetRenderersFromAppearanceController();
        UpdateRendererDictionary();

        _curAppearanceData = _uiAppearanceController.GetCurrentAppearanceData();
    }

    private void SetRenderersFromAppearanceController()
    {
        _headRenderer = _uiAppearanceController.BodyHead;
        _faceHairRenderer = _uiAppearanceController.BodyFaceHair;
        _hairRenderer = _uiAppearanceController.BodyHair;
        _bodyRenderer = _uiAppearanceController.BodyBody;
        _armRRenderer = _uiAppearanceController.BodyRightArm;
        _armLRenderer = _uiAppearanceController.BodyLeftArm;
        _pantRRenderer = _uiAppearanceController.BodyRightFoot;
        _pantLRenderer = _uiAppearanceController.BodyLeftFoot;
        _helmetRenderer = _uiAppearanceController.ClothHelmet;
    }

    private void UpdateRendererDictionary()
    {
        _partRenderers = new Dictionary<BodyPart, SpriteRenderer>
        {
            { BodyPart.Head, _headRenderer },
            { BodyPart.FaceHair, _faceHairRenderer },
            { BodyPart.Hair, _hairRenderer },
            { BodyPart.Helmet, _helmetRenderer },
            { BodyPart.Body, _bodyRenderer },
            { BodyPart.Arm_R, _armRRenderer },
            { BodyPart.Arm_L, _armLRenderer },
            { BodyPart.Pant_R, _pantRRenderer },
            { BodyPart.Pant_L, _pantLRenderer }
        };
    }

    public void AppItem(BodyPart part, Sprite sprite, Color color)
    {
        Debug.Log($"[CostumeManager]AddIemÃß°¡ -> [part:{part}]");

        if (_partRenderers.Count == 0)
            UpdateRendererDictionary();

        bool isWearingHelmet = _helmetRenderer?.sprite != null || _curAppearanceData?.clothHelmetSprite != null;

        if (part == BodyPart.Helmet)
        {
            if (_helmetRenderer != null)
            {
                _helmetRenderer.sprite = sprite;
                _helmetRenderer.color = color;
            }

            _savedHairSprite = _hairRenderer.sprite;
            _hairRenderer.sprite = null;
        }
        else if (part == BodyPart.Hair)
        {
            if (isWearingHelmet)
            {
                _savedHairSprite = sprite;
                _hairRenderer.color = color;
            }
            else if (_partRenderers.TryGetValue(part, out var renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }
        else if (part == BodyPart.FaceHair)
        {
            if (_partRenderers.TryGetValue(part, out var renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }
        else
        {
            if (_partRenderers.TryGetValue(part, out var renderer) && renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }

        UpdateAppearanceData(part, sprite, color);
    }

    private void UpdateAppearanceData(BodyPart part, Sprite sprite, Color color)
    {
        if (_uiAppearanceController == null) return;

        AppearanceData updatedData = _uiAppearanceController.GetCurrentAppearanceData();

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
                if (_helmetRenderer?.sprite == null && updatedData.clothHelmetSprite == null)
                {
                    updatedData.bodyHairSprite = sprite;
                    updatedData.hairColor = color;
                }
                break;
            case BodyPart.Helmet:
                updatedData.clothHelmetSprite = sprite;
                if (updatedData.bodyHairSprite != null)
                {
                    updatedData.bodyHairSprite = null;
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

        _uiAppearanceController.SetAppearance(updatedData);
        _curAppearanceData = updatedData;
    }

    public void UnequipHelmet()
    {
        Debug.Log("UnequipHelmet -");
        if (_helmetRenderer != null)
            _helmetRenderer.sprite = null;

        if (_hairRenderer != null)
        {
            if (_savedHairSprite != null)
            {
                _hairRenderer.sprite = _savedHairSprite;
                if (_curAppearanceData != null)
                    _curAppearanceData.bodyHairSprite = _savedHairSprite;
            }
            else if (_curAppearanceData?.bodyHairSprite != null)
            {
                _hairRenderer.sprite = _curAppearanceData.bodyHairSprite;
            }
        }

        if (_curAppearanceData != null)
        {
            _curAppearanceData.clothHelmetSprite = null;
        }

        _savedHairSprite = null;
        UpdateGameAppearanceData();
    }

    public void ResetPartItem(BodyPart part)
    {
        Debug.Log("ResetPartItem -"+ part);
        if (_partRenderers.TryGetValue(part, out var renderer) && renderer != null)
        {
            renderer.sprite = null;
        }

        if (_curAppearanceData != null)
        {
            switch (part)
            {
                case BodyPart.Head:
                    _curAppearanceData.bodyHeadSprite = null;
                    break;
                case BodyPart.FaceHair:
                    _curAppearanceData.bodyFaceHairSprite = null;
                    break;
                case BodyPart.Hair:
                    _curAppearanceData.bodyHairSprite = null;
                    break;
                case BodyPart.Helmet:
                    _curAppearanceData.clothHelmetSprite = null;
                    _hairRenderer.sprite = _savedHairSprite;
                    break;
                case BodyPart.Body:
                    _curAppearanceData.clothBodySprite = null;
                    break;
                case BodyPart.Arm_R:
                    _curAppearanceData.clothRightArmSprite = null;
                    break;
                case BodyPart.Arm_L:
                    _curAppearanceData.clothLeftArmSprite = null;
                    break;
                case BodyPart.Pant_R:
                    _curAppearanceData.clothRightFootSprite = null;
                    break;
                case BodyPart.Pant_L:
                    _curAppearanceData.clothLeftFootSprite = null;
                    break;
            }
        }
    }

    public void ResetAllItems()
    {
        Debug.Log("ResetAllItems -" );
        foreach (var renderer in _partRenderers.Values)
        {
            if (renderer != null)
                renderer.sprite = null;
        }

        if (_uiAppearanceController != null)
        {
            if (_uiAppearanceController.HasDefaultAppearanceData())
            {
                _uiAppearanceController.SetDefaultAppearacne();
                _curAppearanceData = _uiAppearanceController.GetCurrentAppearanceData();
            }
            else
            {
                SetDefaultAppearance();
            }
        }
    }

    public void SetDefaultAppearance()
    {
        Debug.Log("SetDefaultAppearance -");
        AppearanceData defaultData = ScriptableObject.CreateInstance<AppearanceData>();

        if (_curAppearanceData != null)
        {
            defaultData.bodyBodySprite = _curAppearanceData.bodyBodySprite;
            defaultData.bodyLeftArmSprite = _curAppearanceData.bodyLeftArmSprite;
            defaultData.bodyRightArmSprite = _curAppearanceData.bodyRightArmSprite;
            defaultData.bodyLeftFootSprite = _curAppearanceData.bodyLeftFootSprite;
            defaultData.bodyRightFootSprite = _curAppearanceData.bodyRightFootSprite;
            defaultData.bodyBackEyeSprite = _curAppearanceData.bodyBackEyeSprite;
            defaultData.bodyFrontEyeSprite = _curAppearanceData.bodyFrontEyeSprite;
            defaultData.hairColor = _curAppearanceData.hairColor;
            defaultData.eyeColor = _curAppearanceData.eyeColor;
        }

        _uiAppearanceController.SetAppearance(defaultData);
        _curAppearanceData = defaultData;
    }

    public void UpdateGameAppearanceData()
    {
        _gameAppearanceController?.SetAppearance(_curAppearanceData);
    }
}
