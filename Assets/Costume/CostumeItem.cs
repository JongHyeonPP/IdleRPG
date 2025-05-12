using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostumePartData
{
    public BodyPart Part;                           // 적용할 부위
    public Sprite CostumeSprite;                    // 캐릭터에 적용할 스프라이트
    public Color CostumeColor = Color.white;        // 코스튬 색상
}

public enum CostumePart
{
    None = -1,      // Default null 값
    Hair = 0,       // 헤어 스타일
    Helmet,         // 헬멧, 모자 등 머리 장식
    Face,           // 얼굴
    Top,            // 상의
    Bottom,         // 하의
    Shoes,          // 신발
    Armor,          // 갑옷
    Eye,            // 눈
    Beard           // 수염
}

[CreateAssetMenu(fileName = "NewCostumeItem", menuName = "Scriptable Objects/CostumeItem")]
[System.Serializable]
public class CostumeItem : ScriptableObject, IGachaItems
{
    public string Uid;                                  // 고유값
    public string Name;                                 // 이름
    public string Description;                          // 코스튬 아이템 설명
    public Texture2D IconTexture;                       // UI 아이콘용 텍스처
    public Texture2D IconXTexture;                      // UI 아이콘 없을때 텍스처
    public Color IconColor = Color.white;               // 아이콘 색상
    public CostumePart CostumeType;                     // 코스튬 타입
    public List<CostumePartData> Parts;                 // 여러 부위 데이터 리스트

    [SerializeField] private Rarity _costumeRarity;     // 코스튬의 등급

    // IGachaItem 인터페이스 구현 (무조건 필요함)
    public Rarity ItemRarity => _costumeRarity;
}
