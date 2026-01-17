using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EnumCollection;
using System.Linq;

public class CostumeManager : MonoSingleton<CostumeManager>
{
    [Header("Character Data")]
    [SerializeField] private CostumeCharacterRenderer _characterRenderer;                        // 캐릭터 랜더러

    public CostumeItem[] AllCostumeDatas;                                                        // 모든 코스튬 데이터들

    [Header("Default Items")]
    [Tooltip("0: 헤어, 1: 상의, 2: 하의, 3: 신발")]
    [SerializeField] private CostumeItem[] _defaultItems;                                        // 각 부위별 기본 아이템 설정

    public List<string> EquipedCostumes = new();                                                 // 장착한 코스튬 UID 리스트
    public List<string> OwnedCostumes = new();                                                   // 소유한 코스튬 UID 리스트

    // 조회용
    public Dictionary<string, CostumeItem> ByUid = new Dictionary<string, CostumeItem>();

    #region Unity Lifecycle, Init

    protected override void Awake()
    {
        base.Awake();

        // 리소스에서 코스튬 데이터 로드
        LoadAll();
    }

    private void Start()
    {
        // 코스튬 서버에서 받아오기
        GameData gameData = StartBroker.GetGameData();

        // null 처리
        OwnedCostumes = gameData.ownedCostumes ?? new List<string>();
        EquipedCostumes = gameData.equipedCostumes ?? new List<string>();

        // 캐릭터 렌더러 초기화
        _characterRenderer.Init();

        ClearCostume(gameData);
    }

    // 기본 세팅
    public void ClearCostume(GameData gamedata) 
    {
        // 장착 정보가 없으면 디폴트 아이템으로 세팅
        if (EquipedCostumes == null || EquipedCostumes.Count == 0)
        {
            // 새 리스트로 초기화
            EquipedCostumes = new List<string>();

            foreach (var defaultItem in _defaultItems)
            {
                if (defaultItem == null) continue;

                // 기본 아이템 적용
                AddDefaultItem(defaultItem);

                // 장착 리스트 ID 추가
                if (!string.IsNullOrEmpty(defaultItem.Uid) &&
                    !EquipedCostumes.Contains(defaultItem.Uid))
                {
                    EquipedCostumes.Add(defaultItem.Uid);
                }

                // 소유 아이템 추가 -> 보고 아니면 빼도 될듯
                if (!string.IsNullOrEmpty(defaultItem.Uid) &&
                    !OwnedCostumes.Contains(defaultItem.Uid))
                {
                    OwnedCostumes.Add(defaultItem.Uid);
                }
            }

            // GameData 현재 반영
            gamedata.equipedCostumes = new List<string>(EquipedCostumes);
            gamedata.ownedCostumes = new List<string>(OwnedCostumes);

            // 외형/데이터 동기화
            UpdateAppearanceData();
            UpdateCostumeData();
        }
        else
        {
            // 장착 정보가 있으면 세팅해주기
            SetEquipedAll(EquipedCostumes);
            UpdateAppearanceData();
        }
    }

    // 로컬 코스튬 정보 로드
    public void LoadAll()
    {
        ByUid.Clear();

        // Resources/Costume/CostumeItem
        // 모든 CostumeItem 로드
        var loaded = Resources.LoadAll<CostumeItem>("Costume/CostumeItem"); // 하위 폴더까지 전부
        AllCostumeDatas = loaded != null ? loaded : new CostumeItem[0];

        //CostumeItem 매핑
        for (int i = 0; i < AllCostumeDatas.Length; i++)
        {
            var item = AllCostumeDatas[i];
            if (item == null) continue;

            if (!string.IsNullOrEmpty(item.Uid) && !ByUid.ContainsKey(item.Uid))
                ByUid.Add(item.Uid, item);
        }
    }

    #endregion

    #region GameData

    // 서버 데이터 업데이트
    public void UpdateCostumeData()
    {
        GameData gameData = StartBroker.GetGameData();
        gameData.equipedCostumes = EquipedCostumes;
        gameData.ownedCostumes = OwnedCostumes;
    }

    public void UpdateAppearanceData() => _characterRenderer.UpdateGameAppearanceData();

    #endregion

    #region Query Helpers

    // 장착 리스트에 포함되어 있는지 확인
    public bool IsEquipped(string uid)
    {
        return EquipedCostumes.Contains(uid);
    }

    // 소유 리스트에 포함되어 있는지 확인
    public bool IsOwned(string uid)
    {
        return OwnedCostumes.Contains(uid);
    }

    // 장착 코스튬 가져오기
    public List<string> GetOwnedCostumes()
    {
        return OwnedCostumes ?? new List<string>();
    }

    // 캐릭터에 장착
    void SetEquipedAll(List<string> equipedItem)
    {
        if (equipedItem == null || equipedItem.Count == 0)
            return;

        // 원본 리스트는 EquipPartCostume에서 수정되므로, 스냅샷(복사본) 만들어서 순회
        var snapshot = equipedItem.ToArray();    // 또는 ToList()

        foreach (var uid in snapshot)
        {
            // UID로 코스튬 찾기 (딕셔너리 우선, 없으면 전체 배열에서 탐색)
            CostumeItem item = null;

            if (!string.IsNullOrEmpty(uid) && ByUid.TryGetValue(uid, out var dicItem))
            {
                item = dicItem;
            }
            else
            {
                item = AllCostumeDatas.FirstOrDefault(x => x.Uid == uid);
            }

            if (item != null)
            {
                // 부위별로 다시 장착 처리
                EquipPartCostume(item.Uid, item.CostumeType);
            }
            else
            {
                Debug.LogWarning($"장착 목록에 있는 UID '{uid}'에 해당하는 코스튬이 없습니다.");
            }
        }
    }

    // 디폴트 아이템 적용 
    private void AddDefaultItem(CostumeItem defaultItem)
    {
        if (defaultItem == null || _characterRenderer == null) return;

        foreach (var partData in defaultItem.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        Debug.Log($"[CostumeManager] 기본 아이템 '{defaultItem.Name}' 적용됨");
    }

    #endregion

    #region Filter Methods


    // UI 필터 타입에 따라 코스튬 목록 필터링
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

    // 소유한 코스튬 중 필터 타입에 맞는 목록 반환
    public List<CostumeItem> GetOwnedCostumesByFilterType(int filterType)
    {
        // 먼저 모든 소유 코스튬 가져오기
        List<CostumeItem> ownedCostumes = AllCostumeDatas
            .Where(costume => IsOwned(costume.Uid))
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

    #endregion

    #region Equip , Unequip
    // 특정 부위의 코스튬만 착용하기
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
        if (!IsOwned(costumeUid))
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}를 소유하고 있지 않습니다.");
            return false;
        }

        // 새 코스튬 부위 적용
        foreach (var partData in newCostume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        // 동일 부위 기존 장착 UID 제거 후 새 UID 추가
        EquipedCostumes.RemoveAll(uid =>
            AllCostumeDatas.Any(item => item.Uid == uid && item.CostumeType == costumeType));
        EquipedCostumes.Add(costumeUid);

        //데이터 업데이트
        UpdateAppearanceData();
        UpdateCostumeData();

        return true;
    }


    // 특정 코스튬 해제하기
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

        // 장착 목록에서 제거
        EquipedCostumes.Remove(costumeUid);

        // 기본 아이템 적용
        CostumeItem defaultItem = _defaultItems.FirstOrDefault(item => item.CostumeType == costume.CostumeType);
        if (defaultItem != null)
        {
            // 헤어/헬멧 고려
            if (costume.CostumeType == CostumePart.Hair)
            {
                bool isHelmetEquipped = EquipedCostumes.Any(uid =>
                {
                    var item = AllCostumeDatas.FirstOrDefault(x => x.Uid == uid);
                    return item != null && item.CostumeType == CostumePart.Helmet;
                });

                if (!isHelmetEquipped)
                {
                    AddDefaultItem(defaultItem);
                }
                else
                {
                    Debug.Log("헬멧 착용 중이므로 기본 머리 적용하지 않음");
                }
            }
            else if (costume.CostumeType == CostumePart.Helmet)
            {
                // 헬멧 디폴트만 별도로 다시 적용
                defaultItem = _defaultItems.FirstOrDefault(item => item.CostumeType == CostumePart.Helmet);
                AddDefaultItem(defaultItem);
            }
            else
            {
                AddDefaultItem(defaultItem);
            }
        }

        //데이터 업데이트
        UpdateAppearanceData();
        UpdateCostumeData();

        return true;
    }

    #endregion
}
