using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CostumeManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _costumeSlotPrefab;
    [SerializeField] private Transform _costumeGridParent;

    [Header("Character Data")]
    [SerializeField] private CharacterRenderer _characterRenderer;
    [SerializeField] private List<CostumeItem> _availableCostumes;  // ���� �ڽ�Ƭ ����Ʈ (ScriptableObject)

    private void Start()
    {
        PopulateCostumeGrid();
    }

    /// <summary>
    /// UI�� �ڽ�Ƭ ��ư ����
    /// </summary>
    private void PopulateCostumeGrid()
    {
        foreach (var costume in _availableCostumes)
        {
            GameObject slotObject = Instantiate(_costumeSlotPrefab, _costumeGridParent);
            Image icon = slotObject.transform.Find("CostumeIcon").GetComponent<Image>();
            Text name = slotObject.transform.Find("CostumeName").GetComponent<Text>();
            Button slotButton = slotObject.GetComponent<Button>();

            // ������ ����
            if (costume.IconTexture != null)
            {
                icon.sprite = Sprite.Create(costume.IconTexture,
                    new Rect(0, 0, costume.IconTexture.width, costume.IconTexture.height), new Vector2(0.5f, 0.5f));
                name.text = costume.Name;
            }

            // Ŭ���ϸ� �ش� �ڽ�Ƭ ����
            slotButton.onClick.AddListener(() => EquipCostume(costume));
        }
    }

    /// <summary>
    /// Ư�� �ڽ�Ƭ ���� (���� ������ ���� ����)
    /// </summary>
    private void EquipCostume(CostumeItem costume)
    {
        if (_characterRenderer == null) return;

        foreach (var partData in costume.Parts)
        {
            _characterRenderer.ApplySprite(partData.Part, partData.CostumeSprite);
        }
    }

    /// <summary>
    /// ��� �ڽ�Ƭ �ʱ�ȭ
    /// </summary>
    public void ResetAllSprites()
    {
        _characterRenderer.ResetAllSprites();
    }
}
