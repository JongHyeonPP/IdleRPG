using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EnumCollection;
using System.Linq;


public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;

    [Header("Character Data")]
    [SerializeField] private CostumeCharacterRenderer _characterRenderer;                        // 캐릭터 랜더러

    public CostumeItem[] AllCostumeDatas;                                                        // 모든 코스튬 데이터들

    [Header("Default Items")]
    [Tooltip("0: 헤어, 1: 상의, 2: 하의, 3: 신발")]
    [SerializeField] private CostumeItem[] _defaultItems;                                        // 각 부위별 기본 아이템 설정

    public List<string> EquipedCostumes = new();                                             // 장착한 코스튬들
    public List<string> OwnedCostumes = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        //저장 로드만 여기서 해보면 될듯 ..?


        // 저장된 코스튬 정보 가져오기
        GameData gameData = StartBroker.GetGameData();
        OwnedCostumes = gameData.ownedCostumes;

        //Test Code       
        //OwnedCostumes = new() { "0", "1", "2", "100", "200", "300", "101", "105" };
        //gameData.ownedCostumes = OwnedCostumes;
        // gameData.equipedCostumes = EquipedCostumes;

        //  gameData.ownedCostumes = OwnedCostumes;
        /*        gameData.ownedCostumes = new(){"0", "1", "2","100","200","300"};
                gameData.equipedCostumes = new(){"100"};*/

        _characterRenderer.Init();
        if (EquipedCostumes.Count <= 0)
        {
            SetDefaultAll();
            SetEquipedAll(gameData.equipedCostumes);
            UpdateGameAppearanceData();
        }
    }

    public void UpdateCostumeData() 
    {
        GameData gameData = StartBroker.GetGameData();
        gameData.equipedCostumes = EquipedCostumes;
        gameData.ownedCostumes = OwnedCostumes;
    }

    public bool IsEquipped(string uid)
    {
        //  return _temporaryCostumes.Any(item => item.Uid == uid);
        return EquipedCostumes.Contains(uid);

    }
    public bool IsOwned(string uid)
    {
/*        GameData gameData = StartBroker.GetGameData();
        return gameData.ownedCostumes.Contains(uid);*/
        return OwnedCostumes.Contains(uid);
    }

    public List<string> GetOwnedCostumes()
    {
        GameData gameData = StartBroker.GetGameData();
        return gameData.ownedCostumes;

    }

    void SetEquipedAll(List<string> equipedItem)
    {
        foreach (var uid in equipedItem)
        {
            CostumeItem item = AllCostumeDatas.FirstOrDefault(x => x.Uid == uid);
            if (item != null)
            {
                EquipPartCostume(item.Uid, item.CostumeType);
            }
            else
            {
                Debug.LogWarning($"장착 목록에 있는 UID '{uid}'에 해당하는 코스튬이 없습니다.");
            }
        }
    }

    void SetDefaultAll()
    {
        foreach (var item in _defaultItems)
        {
            ApplyDefaultItem(item);
        }
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
        if (!IsOwned(costumeUid))
        {
            Debug.LogWarning($"코스튬 UID {costumeUid}를 소유하고 있지 않습니다.");
            return false;
        }

        //// 해당 부위의 기존 코스튬 제거
        //_temporaryCostumes.RemoveAll(item => item.CostumeType == costumeType);

        //// 해당 부위의 스프라이트 리셋
        //foreach (CostumeItem costume in _temporaryCostumes)
        //{
        //    foreach (var partData in costume.Parts)
        //    {
        //        // 같은 부위의 스프라이트를 사용하는 다른 코스튬도 제거
        //        if (newCostume.Parts.Any(p => p.Part == partData.Part))
        //        {
        //            _characterRenderer.ResetPartItem(partData.Part);
        //        }
        //    }
        //}

        // 새 코스튬 부위 적용
        foreach (var partData in newCostume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        // 새 코스튬 임시 목록에 추가
        //_temporaryCostumes.Add(newCostume);

        // 임시 장착 목록 업데이트 (UI 테스트용)
        EquipedCostumes.RemoveAll(uid =>
            AllCostumeDatas.Any(item => item.Uid == uid && item.CostumeType == costumeType));
        EquipedCostumes.Add(costumeUid);

        //로컬업데이트
        UpdateGameAppearanceData();

        //서버 데이터 업데이트 
        UpdateCostumeData();

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
        EquipedCostumes.Remove(costumeUid);

        // 이부분이 이상한듯?
        // 기본 아이템 적용
        CostumeItem defaultItem = _defaultItems.FirstOrDefault(item => item.CostumeType == costume.CostumeType);
        if (defaultItem != null)
        {
            // 헬멧 착용 상태일 경우 기본 헤어 적용 안함
            if (costume.CostumeType == CostumePart.Hair)
            {
                bool isHelmetEquipped = EquipedCostumes.Any(uid =>
                {
                    var item = AllCostumeDatas.FirstOrDefault(x => x.Uid == uid);
                    return item != null && item.CostumeType == CostumePart.Helmet;
                });

                if (!isHelmetEquipped)
                {
                    ApplyDefaultItem(defaultItem);
                }
                else
                {
                    Debug.Log("헬멧 착용 중이므로 기본 머리 적용하지 않음");
                }
            }
            else if (costume.CostumeType == CostumePart.Helmet)
            {
                defaultItem = _defaultItems.FirstOrDefault(item => item.CostumeType == CostumePart.Helmet);
                ApplyDefaultItem(defaultItem);
            }
            else
            {
                ApplyDefaultItem(defaultItem);
            }
        }

        //서버 데이터 업데이트 
        UpdateCostumeData();

        return true;
    }

    private void ApplyDefaultItem(CostumeItem defaultItem)
    {
        if (defaultItem == null || _characterRenderer == null) return;

        // 아이템의 모든 부위를 캐릭터에 적용
        foreach (var partData in defaultItem.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        Debug.Log($"기본 아이템 '{defaultItem.Name}' 적용됨");
    }
    public void UpdateGameAppearanceData() => _characterRenderer.UpdateGameAppearanceData();

}