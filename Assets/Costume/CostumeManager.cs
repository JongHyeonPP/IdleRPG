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
    [SerializeField] private List<CostumeItem> _availableCostumes;  // 여러 코스튬 리스트 (ScriptableObject)

    private void Start()
    {
        PopulateCostumeGrid();
    }

    /// <summary>
    /// UI에 코스튬 버튼 생성
    /// </summary>
    private void PopulateCostumeGrid()
    {
        foreach (var costume in _availableCostumes)
        {
            GameObject slotObject = Instantiate(_costumeSlotPrefab, _costumeGridParent);
            Image icon = slotObject.transform.Find("CostumeIcon").GetComponent<Image>();
            Text name = slotObject.transform.Find("CostumeName").GetComponent<Text>();
            Button slotButton = slotObject.GetComponent<Button>();

            // 아이콘 설정
            if (costume.IconTexture != null)
            {
                icon.sprite = Sprite.Create(costume.IconTexture,
                    new Rect(0, 0, costume.IconTexture.width, costume.IconTexture.height), new Vector2(0.5f, 0.5f));
                name.text = costume.Name;
            }

            // 클릭하면 해당 코스튬 장착
            slotButton.onClick.AddListener(() => EquipCostume(costume));
        }
    }

    /// <summary>
    /// 특정 코스튬 장착 (여러 부위에 적용 가능)
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
    /// 모든 코스튬 초기화
    /// </summary>
    public void ResetAllSprites()
    {
        _characterRenderer.ResetAllSprites();
    }
}
