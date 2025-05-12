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

public enum CostumePart
{
    None = -1,      // Default null ��
    Hair = 0,       // ��� ��Ÿ��
    Helmet,         // ���, ���� �� �Ӹ� ���
    Face,           // ��
    Top,            // ����
    Bottom,         // ����
    Shoes,          // �Ź�
    Armor,          // ����
    Eye,            // ��
    Beard           // ����
}

[CreateAssetMenu(fileName = "NewCostumeItem", menuName = "Scriptable Objects/CostumeItem")]
[System.Serializable]
public class CostumeItem : ScriptableObject, IGachaItems
{
    public string Uid;                                  // ������
    public string Name;                                 // �̸�
    public string Description;                          // �ڽ�Ƭ ������ ����
    public Texture2D IconTexture;                       // UI �����ܿ� �ؽ�ó
    public Texture2D IconXTexture;                      // UI ������ ������ �ؽ�ó
    public Color IconColor = Color.white;               // ������ ����
    public CostumePart CostumeType;                     // �ڽ�Ƭ Ÿ��
    public List<CostumePartData> Parts;                 // ���� ���� ������ ����Ʈ

    [SerializeField] private Rarity _costumeRarity;     // �ڽ�Ƭ�� ���

    // IGachaItem �������̽� ���� (������ �ʿ���)
    public Rarity ItemRarity => _costumeRarity;
}
