using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostumePartData
{
    public BodyPart Part;                           // ������ ����
    public Sprite CostumeSprite;                    // ĳ���Ϳ� ������ ��������Ʈ
    public Color CostumeColor = Color.white;        // �ڽ�Ƭ ����
}

[CreateAssetMenu(fileName = "NewCostumeItem", menuName = "Scriptable Objects/CostumeItem")]
[System.Serializable]
public class CostumeItem : ScriptableObject, IGachaItems
{
    public string Name;                                 // �̸�
    public Texture2D IconTexture;                       // UI �����ܿ� �ؽ�ó
    public Color IconColor = Color.white;               // ������ ����
    public List<CostumePartData> Parts;                 // ���� ���� ������ ����Ʈ

    [SerializeField] private Rarity _costumeRarity;      // �ڽ�Ƭ�� ���

    // IGachaItem �������̽� ���� (������ �ʿ���)
    public Rarity ItemRarity => _costumeRarity;
}
