using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EnumCollection;
using System.Linq;


public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;
    [Header("UI Elements")]
    [SerializeField] private GameObject _costumeSlotPrefab;                                      // 코스튬 아이템 프리팹
    [SerializeField] private Transform _costumeGridParent;                                       // 코스튬 아이템 부모 위치

    [Header("Character Data")]
    [SerializeField] private CharacterRenderer _characterRenderer;                               // 캐릭터 랜더러
    [SerializeField] private List<CostumeItem> _availableCostumes;                               // 가능 코스튬 리스트

    [SerializeField] private List<CostumeItem> _temporaryCostumes = new List<CostumeItem>();     // 임시로 착용한 코스튬 리스트
    [SerializeField] private List<CostumeItem> _savedCostumes = new List<CostumeItem>();         // 저장된 코스튬 리스트

    public CostumeItem[] AllCostumeDatas;                                                        // 모든 코스튬 데이터들
    public List<CostumeItem> TemporaryCostumes => _temporaryCostumes;

    [Header("Default Items")]
    [Tooltip("0: 헤어, 1: 상의, 2: 하의, 3: 신발")]
    [SerializeField] private CostumeItem[] _defaultItems;                                        // 각 부위별 기본 아이템 설정

    public List<string> TestEquipedCostumes = new();                                 // 장착한 코스튬들
    public List<string> TestOwnedCostumes = new();

    private void Awake()
    {
        Instance = this;

        // 테스트용
        /*        GameData gameData = StartBroker.GetGameData();
                List<string> costumeUidstest = new List<string>();
                costumeUidstest.Add("0");
                costumeUidstest.Add("100");
                costumeUidstest.Add("200");
                gameData.ownedCostumes = costumeUidstest;
                StartBroker.SaveLocal();*/
    }

    private void Start()
    {
        TestEquipedCostumes = new() { "0", "1", "2" };
        TestOwnedCostumes = new() { "0", "1", "2" };

        // 저장된 코스튬 정보 가져오기
        GameData gameData = StartBroker.GetGameData();
        gameData.ownedCostumes = new(){"0", "1", "2"};
        gameData.equipedCostumes = new(){"0", "1", "2"};
        NetworkBroker.SaveServerData();
    }

    public bool IsEquipped(string uid)
    {
        //  return _temporaryCostumes.Any(item => item.Uid == uid);
        return TestEquipedCostumes.Contains(uid);

    }
    public bool IsOwend(string uid) 
    {
       // GameData gameData = StartBroker.GetGameData(); 
        //   return gameData.ownedCostumes.Contains(uid);
        return TestOwnedCostumes.Contains(uid);
    }

    /// <summary>
    /// UI 필터 타입에 따라 코스튬 목록 필터링
    /// </summary>
    public List<CostumeItem> GetCostumesByFilterType(int filterType)
    {
        if (AllCostumeDatas == null || AllCostumeDatas.Length == 0)
            return new List<CostumeItem>();

        List<CostumeItem> allCostumes = AllCostumeDatas.ToList();

        switch (filterType)
        {
            case 0: // 전체
                return allCostumes;

            case 1: // 상의
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Top).ToList();

            case 2: // 하의 (하의+신발)
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Bottom ||
                    costume.CostumeType == CostumePart.Shoes).ToList();

            case 3: // 외모 (헤어, 얼굴, 헬멧 등)
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Hair ||
                    costume.CostumeType == CostumePart.Face ||
                    costume.CostumeType == CostumePart.Helmet).ToList();

            case 4: // 기타 (나머지)
                return allCostumes.Where(costume =>
                    costume.CostumeType != CostumePart.Top &&
                    costume.CostumeType != CostumePart.Bottom &&
                    costume.CostumeType != CostumePart.Shoes &&
                    costume.CostumeType != CostumePart.Hair &&
                    costume.CostumeType != CostumePart.Face &&
                    costume.CostumeType != CostumePart.Helmet).ToList();

            default:
                return allCostumes;
        }
    }

    /// <summary>
    /// 소유한 코스튬 중 필터 타입에 맞는 목록 반환
    /// </summary>
    public List<CostumeItem> GetOwnedCostumesByFilterType(int filterType)
    {
        // 먼저 모든 소유 코스튬 가져오기
        List<CostumeItem> ownedCostumes = AllCostumeDatas
            .Where(costume => IsOwend(costume.Uid))
            .ToList();

        // 필터 적용
        switch (filterType)
        {
            case 0: // 전체
                return ownedCostumes;

            case 1: // 상의
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Top).ToList();

            case 2: // 하의 (하의+신발)
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Bottom ||
                    costume.CostumeType == CostumePart.Shoes).ToList();

            case 3: // 외모 (헤어, 얼굴, 헬멧 등)
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Hair ||
                    costume.CostumeType == CostumePart.Face ||
                    costume.CostumeType == CostumePart.Helmet).ToList();

            case 4: // 기타 (나머지)
                return ownedCostumes.Where(costume =>
                    costume.CostumeType != CostumePart.Top &&
                    costume.CostumeType != CostumePart.Bottom &&
                    costume.CostumeType != CostumePart.Shoes &&
                    costume.CostumeType != CostumePart.Hair &&
                    costume.CostumeType != CostumePart.Face &&
                    costume.CostumeType != CostumePart.Helmet).ToList();

            default:
                return ownedCostumes;
        }
    }

    /// <summary>
    /// 특정 부위의 코스튬만 착용하기
    /// </summary>

    public bool EquipPartCostume(string costumeUid, CostumePart costumeType)
    {
        // UID로 코스튬 찾기
        CostumeItem newCostume = AllCostumeDatas.FirstOrDefault(item => item.Uid == costumeUid);

        if (newCostume == null)
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}를 찾을 수 없습니다.");
            return false;
        }

        // 코스튬 타입 확인
        if (newCostume.CostumeType != costumeType)
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}는 {costumeType} 유형이 아닙니다.");
            return false;
        }

        // 코스튬 소유 확인
        if (!IsOwend(costumeUid))
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}를 소유하고 있지 않습니다.");
            return false;
        }

        // 해당 부위의 기존 코스튬 제거
        _temporaryCostumes.RemoveAll(item => item.CostumeType == costumeType);

        // 해당 부위의 스프라이트 리셋
        foreach (CostumeItem costume in _temporaryCostumes)
        {
            foreach (var partData in costume.Parts)
            {
                // 같은 부위의 스프라이트를 사용하는 다른 코스튬도 제거
                if (newCostume.Parts.Any(p => p.Part == partData.Part))
                {
                    _characterRenderer.ResetPartItem(partData.Part);
                }
            }
        }

        // 새 코스튬 부위 적용
        foreach (var partData in newCostume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        // 새 코스튬 임시 목록에 추가
        _temporaryCostumes.Add(newCostume);

        // 임시 장착 목록 업데이트 (UI 테스트용)
        TestEquipedCostumes.RemoveAll(uid =>
            AllCostumeDatas.Any(item => item.Uid == uid && item.CostumeType == costumeType));
        TestEquipedCostumes.Add(costumeUid);

        return true;
    }

    /// <summary>
    /// 특정 코스튬 해제하기
    /// </summary>
    public bool UnequipCostume(string costumeUid)
    {
        // UID로 코스튬 찾기
        CostumeItem costume = AllCostumeDatas.FirstOrDefault(item => item.Uid == costumeUid);

        if (costume == null)
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}를 찾을 수 없습니다.");
            return false;
        }

        // 코스튬 부위 리셋
        foreach (var partData in costume.Parts)
        {
            _characterRenderer.ResetPartItem(partData.Part);
        }

        // 임시 장착 목록 업데이트
        TestEquipedCostumes.Remove(costumeUid);

        // 기본 아이템 적용
        CostumeItem defaultItem = null;

        switch (costume.CostumeType)
        {
            case CostumePart.Hair:
                defaultItem = _defaultItems[0];
                break;
            case CostumePart.Top:
                defaultItem = _defaultItems[1];
                break;
            case CostumePart.Bottom:
                defaultItem = _defaultItems[2];
                break;
            case CostumePart.Shoes:
                defaultItem = _defaultItems[3];
                break;
        }

        if (defaultItem != null)
        {
            ApplyDefaultItem(defaultItem);
        }

        return true;
    }

    #region 예전코드
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
    public void EquipCostume(CostumeItem costume)
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

        // 저장할 코스튬 UID 리스트 생성
        List<string> costumeUids = new List<string>();
        foreach (var costume in _savedCostumes)
        {
            if (!string.IsNullOrEmpty(costume.Uid))
            {
                costumeUids.Add(costume.Uid);
            }
        }

        GameData gameData = StartBroker.GetGameData();
        gameData.equipedCostumes = costumeUids;

        NetworkBroker.SaveServerData();
        Debug.Log("[CostumeManager: Costume 아이템 저장됨]");
    }

    /// <summary>
    /// 모든 코스튬 아예 전체 뺸걸로 초기화하고 기본 아이템 적용
    /// </summary>
    public void ResetAllCostumes()
    {
        _characterRenderer.ResetAllItems();
        _temporaryCostumes = new List<CostumeItem>();

        // 모든 기본 아이템 적용
        ApplyDefaultItem(_defaultItems[0]);
        ApplyDefaultItem(_defaultItems[1]);
        ApplyDefaultItem(_defaultItems[2]);
        ApplyDefaultItem(_defaultItems[3]);
    }

    /// <summary>
    /// 특정 부위의 코스튬을 리셋하고 해당 부위의 기본 아이템 적용
    /// </summary>
    public void ResetPartCostumes(CostumeItem costumeItem)
    {
        // 해당 유형의 코스튬 아이템 제거
        _temporaryCostumes.RemoveAll(item => item.CostumeType == costumeItem.CostumeType);

        CostumeItem defaultItem = null;

        // 해당 부위 디폴트 아이템 결정
        switch (costumeItem.CostumeType)
        {
            case CostumePart.Hair:
                defaultItem = _defaultItems[0];
                break;
            case CostumePart.Top:
                defaultItem = _defaultItems[1];
                break;
            case CostumePart.Bottom:
                defaultItem = _defaultItems[2];
                break;
            case CostumePart.Shoes:
                defaultItem = _defaultItems[3];
                break;
        }

        // 기본 아이템 적용
        ApplyDefaultItem(defaultItem);
    }

    /// <summary>
    /// 기본 아이템을 캐릭터에 적용하고 임시 목록에 추가하는 함수
    /// </summary>
    private void ApplyDefaultItem(CostumeItem defaultItem)
    {
        if (defaultItem != null)
        {
            // 아이템의 모든 부위를 캐릭터에 적용
            foreach (var partData in defaultItem.Parts)
            {
                _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
            }
        }
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
    #endregion
}
