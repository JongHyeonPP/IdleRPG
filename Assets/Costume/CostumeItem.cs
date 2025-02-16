using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostumePartData
{
    public BodyPart Part;            // ������ ���� 
    public Sprite CostumeSprite;     // ĳ���Ϳ� ������ ��������Ʈ
    public Color CostumeColor;       // �ڽ�Ƭ ����
}

[CreateAssetMenu(fileName = "NewCostumeItem", menuName = "Scriptable Objects/CostumeItem")]
[System.Serializable]
public class CostumeItem : ScriptableObject
{
    public string Name;                         // �ǻ� �̸�
    public Texture2D IconTexture;               // UI �����ܿ� �ؽ�ó
    public List<CostumePartData> Parts;         // ���� ������ ��������Ʈ ����Ʈ
}

