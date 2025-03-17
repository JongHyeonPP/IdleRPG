using UnityEngine;

[CreateAssetMenu(fileName = "AppearanceData", menuName = "Scriptable Objects/AppearanceData")]
public class AppearanceData : ScriptableObject
{
    [Header("Clothes")]
    public Sprite clothBack_0Sprite;
    public Sprite clothBack_1Sprite;
    public Sprite clothHelmetSprite;
    public Sprite clothBodySprite;
    public Sprite clothLeftArmSprite;
    public Sprite clothRightArmSprite;
    public Sprite clothLeftFootSprite;
    public Sprite clothRightFootSprite;
    public Sprite clothBodyArmorSprite;
    public Sprite clothLeftShoulderSprite;
    public Sprite clothRightShoulderSprite;
    [Header("Body")]
    public Sprite bodyLeftArmSprite;
    public Sprite bodyRightArmSprite;
    public Sprite bodyBodySprite;
    public Sprite bodyLeftFootSprite;
    public Sprite bodyRightFootSprite;
    public Sprite bodyHeadSprite;
    public Sprite bodyHairSprite;
    public Sprite bodyFaceHairSprite;
    public Sprite bodyBackEyeSprite;
    public Sprite bodyFrontEyeSprite;
    [Header("Color")]
    public Color hairColor;
    public Color eyeColor;
}
