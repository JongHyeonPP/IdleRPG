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
    [Header("Set In Editor")]
    [SerializeField] AppearanceData _defaultAppearanceData;

    public void SetAppearance(AppearanceData appearanceData)
    {
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
    [ContextMenu("SetDefaultAppearance")]
    public void SetDefaultAppearacne()
    {
        SetAppearance(_defaultAppearanceData);
    }
}
