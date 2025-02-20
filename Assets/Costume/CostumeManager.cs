using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CostumeManager : MonoSingleton<CostumeManager>
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _costumeSlotPrefab;                                      // 코스튬 아이템 프리팹
    [SerializeField] private Transform _costumeGridParent;                                       // 코스튬 아이템 부모 위치

    [Header("Character Data")]
    [SerializeField] private CharacterRenderer _characterRenderer;                               // 캐릭터 랜더러
    [SerializeField] private List<CostumeItem> _availableCostumes;                               // 가능 코스튬 리스트

    [SerializeField] private List<CostumeItem> _temporaryCostumes = new List<CostumeItem>();     // 임시로 착용한 코스튬 리스트
    [SerializeField] private List<CostumeItem> _savedCostumes = new List<CostumeItem>();         // 저장된 코스튬 리스트

    public CostumeItem[] AllCostumeDatas;                                                        // 모든 코스튬 데이터들

    private void Start()
    {
        // 저장 코스튬 가져오는 기능 만들어야함
        // 없으면 new로 초기화
        if(_savedCostumes == null)  
            _savedCostumes = new List<CostumeItem>(_temporaryCostumes);

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
                icon.color = costume.IconColor;
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

        // 이미 임시 리스트에 있다면 해제, 아니면 추가
        if (_temporaryCostumes.Contains(costume))
        {
            _temporaryCostumes.Remove(costume);
        }
        else
        {
            _temporaryCostumes.Add(costume);
        }

        foreach (var partData in costume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }
    }

    /// <summary>
    /// 현재 코스튬 상태를 저장시킴
    /// </summary>
    public void SaveCostumeChanges()
    {
        _savedCostumes = new List<CostumeItem>(_temporaryCostumes);
        // _savedCostumes를 서버에 보냄

        Debug.Log("Costume 저장됨");
    }

    /// <summary>
    /// 모든 코스튬 아예 전체 뺸걸로 초기화
    /// </summary>
    public void ResetAllCostumes()
    {
        _characterRenderer.ResetAllItems();
        _temporaryCostumes = new List<CostumeItem>();
    }

    /// <summary>
    /// 저장하지 않은 변경사항을 취소하고 마지막 저장 상태로 복원
    /// </summary>
    public void CancelCostumeChanges()
    {
        // 임시 리스트를 마지막 저장된 상태로 복원
        _temporaryCostumes = new List<CostumeItem>(_savedCostumes);

        // 캐릭터 스프라이트 재적용
        _characterRenderer.ResetAllItems();
        foreach (var savedCostume in _savedCostumes)
        {
            foreach (var partData in savedCostume.Parts)
            {
                _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
            }
        }

        Debug.Log("코스튬 마지막 저장 상태로 돌아감");
    }
}
