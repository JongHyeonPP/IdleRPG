using UnityEditor;
using UnityEngine;

public class AppearanceController : MonoBehaviour
{
    [Header("Clothes")]
    [SerializeField] SpriteRenderer _clothBack_0;
    [SerializeField] SpriteRenderer _clothBack_1;
    [SerializeField] SpriteRenderer _clothHelmet;
    [SerializeField] SpriteRenderer _clothLeftArm;
    [SerializeField] SpriteRenderer _clothRightArm;
    [SerializeField] SpriteRenderer _clothLeftShoulder;
    [SerializeField] SpriteRenderer _clothRightShoulder;
    [SerializeField] SpriteRenderer _clothLeftFoot;
    [SerializeField] SpriteRenderer _clothRightFoot;
    [SerializeField] SpriteRenderer _clothBody;
    [SerializeField] SpriteRenderer _clothBodyArmor;
    [Header("Body")]
    [SerializeField] SpriteRenderer _bodyBody;
    [SerializeField] SpriteRenderer _bodyHead;
    [SerializeField] SpriteRenderer _bodyHair;
    [SerializeField] SpriteRenderer _bodyFaceHair;
    [SerializeField] SpriteRenderer _bodyLeftBackEye;
    [SerializeField] SpriteRenderer _bodyLeftFrontEye;
    [SerializeField] SpriteRenderer _bodyRightBackEye;
    [SerializeField] SpriteRenderer _bodyRightFrontEye;
    [SerializeField] SpriteRenderer _bodyLeftArm;
    [SerializeField] SpriteRenderer _bodyRightArm;
    [SerializeField] SpriteRenderer _bodyLeftFoot;
    [SerializeField] SpriteRenderer _bodyRightFoot;
    [Header("AppearanceData")]
    [SerializeField] AppearanceData _defaultAppearanceData;
    [SerializeField] AppearanceData _currentAppearanceData;

    #region SpriteRenderer에 대한 public 접근자
    // Body 관련
    public SpriteRenderer BodyHead => _bodyHead;
    public SpriteRenderer BodyHair => _bodyHair;
    public SpriteRenderer BodyFaceHair => _bodyFaceHair;
    public SpriteRenderer BodyBody => _bodyBody;
    public SpriteRenderer BodyLeftArm => _bodyLeftArm;
    public SpriteRenderer BodyRightArm => _bodyRightArm;
    public SpriteRenderer BodyLeftFoot => _bodyLeftFoot;
    public SpriteRenderer BodyRightFoot => _bodyRightFoot;

    // Clothes 관련
    public SpriteRenderer ClothHelmet => _clothHelmet;
    public SpriteRenderer ClothBody => _clothBody;
    public SpriteRenderer ClothLeftArm => _clothLeftArm;
    public SpriteRenderer ClothRightArm => _clothRightArm;
    public SpriteRenderer ClothLeftFoot => _clothLeftFoot;
    public SpriteRenderer ClothRightFoot => _clothRightFoot;
    #endregion

    public void SetAppearance(AppearanceData appearanceData)
    {
        _currentAppearanceData = appearanceData;
        // Clothes
        _clothBack_0.sprite = appearanceData.clothBack_0Sprite;
        _clothBack_1.sprite = appearanceData.clothBack_1Sprite;
        _clothHelmet.sprite = appearanceData.clothHelmetSprite;
        _clothLeftArm.sprite = appearanceData.clothLeftArmSprite;
        _clothRightArm.sprite = appearanceData.clothRightArmSprite;
        _clothLeftShoulder.sprite = appearanceData.clothLeftShoulderSprite;
        _clothRightShoulder.sprite = appearanceData.clothRightShoulderSprite;
        _clothLeftFoot.sprite = appearanceData.clothLeftFootSprite;
        _clothRightFoot.sprite = appearanceData.clothRightFootSprite;
        _clothBody.sprite = appearanceData.clothBodySprite;
        _clothBodyArmor.sprite = appearanceData.clothBodyArmorSprite;

        // Body
        _bodyBody.sprite = appearanceData.bodyBodySprite;
        _bodyHead.sprite = appearanceData.bodyHeadSprite;
        _bodyHair.sprite = appearanceData.bodyHairSprite;
        _bodyFaceHair.sprite = appearanceData.bodyFaceHairSprite;
        _bodyLeftBackEye.sprite = appearanceData.bodyBackEyeSprite;
        _bodyLeftFrontEye.sprite = appearanceData.bodyFrontEyeSprite;
        _bodyRightBackEye.sprite = appearanceData.bodyBackEyeSprite;
        _bodyRightFrontEye.sprite = appearanceData.bodyFrontEyeSprite;
        _bodyLeftArm.sprite = appearanceData.bodyLeftArmSprite;
        _bodyRightArm.sprite = appearanceData.bodyRightArmSprite;
        _bodyLeftFoot.sprite = appearanceData.bodyLeftFootSprite;
        _bodyRightFoot.sprite = appearanceData.bodyRightFootSprite;

        //Color
        _bodyHair.color = appearanceData.hairColor;
        _bodyFaceHair.color = appearanceData.hairColor;
        _bodyLeftFrontEye.color = appearanceData.eyeColor;
        _bodyRightFrontEye.color = appearanceData.eyeColor;


    }
#if UNITY_EDITOR
    [ContextMenu("SetDefaultAppearance")]
    public void SetDefaultAppearacne()
    {
        SetAppearance(_defaultAppearanceData);
        EditorUtility.SetDirty(gameObject);
    }
#endif
    public void SetRGB(float targetValue)
    {
        SpriteRenderer[] spriteFields = new SpriteRenderer[]
        {
        // Clothes
        _clothBack_0, _clothBack_1, _clothHelmet,
        _clothLeftArm, _clothRightArm,
        _clothLeftShoulder, _clothRightShoulder,
        _clothLeftFoot, _clothRightFoot,
        _clothBody, _clothBodyArmor,

        // Body
        _bodyBody, _bodyHead,
        _bodyLeftBackEye,
        _bodyRightBackEye,
        _bodyLeftArm, _bodyRightArm,
        _bodyLeftFoot, _bodyRightFoot
        };

        foreach (var sr in spriteFields)
        {
            if (sr != null)
            {
                sr.color = new Color(targetValue, targetValue, targetValue, 1f);
            }
        }

        Color hairColor = _currentAppearanceData.hairColor;
        _bodyHair.color = _bodyFaceHair.color = new Color(hairColor.r * targetValue, hairColor.g * targetValue, hairColor.b * targetValue, 1f);
        Color eyeColor = _currentAppearanceData.eyeColor;
        _bodyLeftFrontEye.color = _bodyRightFrontEye.color = new Color(eyeColor.r * targetValue, eyeColor.g * targetValue, eyeColor.b * targetValue, 1f);
    }
    public AppearanceData GetCurrentAppearanceData()
    {
        // 현재 AppearanceData의 복사본 반환
        AppearanceData copy = ScriptableObject.CreateInstance<AppearanceData>();

        // Clothes
        copy.clothBack_0Sprite = _currentAppearanceData.clothBack_0Sprite;
        copy.clothBack_1Sprite = _currentAppearanceData.clothBack_1Sprite;
        copy.clothHelmetSprite = _currentAppearanceData.clothHelmetSprite;
        copy.clothLeftArmSprite = _currentAppearanceData.clothLeftArmSprite;
        copy.clothRightArmSprite = _currentAppearanceData.clothRightArmSprite;
        copy.clothLeftShoulderSprite = _currentAppearanceData.clothLeftShoulderSprite;
        copy.clothRightShoulderSprite = _currentAppearanceData.clothRightShoulderSprite;
        copy.clothLeftFootSprite = _currentAppearanceData.clothLeftFootSprite;
        copy.clothRightFootSprite = _currentAppearanceData.clothRightFootSprite;
        copy.clothBodySprite = _currentAppearanceData.clothBodySprite;
        copy.clothBodyArmorSprite = _currentAppearanceData.clothBodyArmorSprite;

        // Body
        copy.bodyBodySprite = _currentAppearanceData.bodyBodySprite;
        copy.bodyHeadSprite = _currentAppearanceData.bodyHeadSprite;
        copy.bodyHairSprite = _currentAppearanceData.bodyHairSprite;
        copy.bodyFaceHairSprite = _currentAppearanceData.bodyFaceHairSprite;
        copy.bodyBackEyeSprite = _currentAppearanceData.bodyBackEyeSprite;
        copy.bodyFrontEyeSprite = _currentAppearanceData.bodyFrontEyeSprite;
        copy.bodyLeftArmSprite = _currentAppearanceData.bodyLeftArmSprite;
        copy.bodyRightArmSprite = _currentAppearanceData.bodyRightArmSprite;
        copy.bodyLeftFootSprite = _currentAppearanceData.bodyLeftFootSprite;
        copy.bodyRightFootSprite = _currentAppearanceData.bodyRightFootSprite;

        // Colors
        copy.hairColor = _currentAppearanceData.hairColor;
        copy.eyeColor = _currentAppearanceData.eyeColor;

        return copy;
    }
    // 기본 AppearanceData 접근자
    public AppearanceData GetDefaultAppearanceData()
    {
        return _defaultAppearanceData;
    }

    // 기본 AppearanceData 존재 여부 확인
    public bool HasDefaultAppearanceData()
    {
        return _defaultAppearanceData != null;
    }
}
