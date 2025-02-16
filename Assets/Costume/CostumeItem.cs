using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostumePartData
{
    public BodyPart Part;            // 적용할 부위 
    public Sprite CostumeSprite;     // 캐릭터에 적용할 스프라이트
    public Color CostumeColor;       // 코스튬 색상
}

[CreateAssetMenu(fileName = "NewCostumeItem", menuName = "Scriptable Objects/CostumeItem")]
[System.Serializable]
public class CostumeItem : ScriptableObject
{
    public string Name;                         // 의상 이름
    public Texture2D IconTexture;               // UI 아이콘용 텍스처
    public List<CostumePartData> Parts;         // 여러 부위의 스프라이트 리스트
}

