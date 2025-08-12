using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EnumCollection;
using System.Linq;


public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;

    [Header("Character Data")]
    [SerializeField] private CostumeCharacterRenderer _characterRenderer;                        // ĳ���� ������

    public CostumeItem[] AllCostumeDatas;                                                        // ��� �ڽ�Ƭ �����͵�

    [Header("Default Items")]
    [Tooltip("0: ���, 1: ����, 2: ����, 3: �Ź�")]
    [SerializeField] private CostumeItem[] _defaultItems;                                        // �� ������ �⺻ ������ ����

    public List<string> EquipedCostumes = new();                                             // ������ �ڽ�Ƭ��
    public List<string> OwnedCostumes = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        //���� �ε常 ���⼭ �غ��� �ɵ� ..?


        // ����� �ڽ�Ƭ ���� ��������
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
                Debug.LogWarning($"���� ��Ͽ� �ִ� UID '{uid}'�� �ش��ϴ� �ڽ�Ƭ�� �����ϴ�.");
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
    /// UI ���� Ÿ�Կ� ���� �ڽ�Ƭ ��� ���͸�
    /// </summary>
    public List<CostumeItem> GetCostumesByFilterType(int filterType)
    {
        if (AllCostumeDatas == null || AllCostumeDatas.Length == 0)
            return new List<CostumeItem>();

        List<CostumeItem> allCostumes = AllCostumeDatas.ToList();

        switch (filterType)
        {
            case 0: // ��ü
                return allCostumes;

            case 1: // ����
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Top).ToList();

            case 2: // ���� (����+�Ź�)
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Bottom ||
                    costume.CostumeType == CostumePart.Shoes).ToList();

            case 3: // �ܸ� (���, ��, ��� ��)
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Hair ||
                    costume.CostumeType == CostumePart.Face ||
                    costume.CostumeType == CostumePart.Helmet).ToList();

            case 4: // ��Ÿ (������)
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
    /// ������ �ڽ�Ƭ �� ���� Ÿ�Կ� �´� ��� ��ȯ
    /// </summary>
    public List<CostumeItem> GetOwnedCostumesByFilterType(int filterType)
    {
        // ���� ��� ���� �ڽ�Ƭ ��������
        List<CostumeItem> ownedCostumes = AllCostumeDatas
            .Where(costume => IsOwned(costume.Uid))
            .ToList();

        // ���� ����
        switch (filterType)
        {
            case 0: // ��ü
                return ownedCostumes;

            case 1: // ����
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Top).ToList();

            case 2: // ���� (����+�Ź�)
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Bottom ||
                    costume.CostumeType == CostumePart.Shoes).ToList();

            case 3: // �ܸ� (���, ��, ��� ��)
                return ownedCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Hair ||
                    costume.CostumeType == CostumePart.Face ||
                    costume.CostumeType == CostumePart.Helmet).ToList();

            case 4: // ��Ÿ (������)
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
    /// Ư�� ������ �ڽ�Ƭ�� �����ϱ�
    /// </summary>

    public bool EquipPartCostume(string costumeUid, CostumePart costumeType)
    {
        // UID�� �ڽ�Ƭ ã��
        CostumeItem newCostume = AllCostumeDatas.FirstOrDefault(item => item.Uid == costumeUid);

        if (newCostume == null)
        {
            Debug.LogWarning($"�ڽ�Ƭ UID {costumeUid}�� ã�� �� �����ϴ�.");
            return false;
        }

        // �ڽ�Ƭ Ÿ�� Ȯ��
        if (newCostume.CostumeType != costumeType)
        {
            Debug.LogWarning($"�ڽ�Ƭ UID {costumeUid}�� {costumeType} ������ �ƴմϴ�.");
            return false;
        }

        // �ڽ�Ƭ ���� Ȯ��
        if (!IsOwned(costumeUid))
        {
            Debug.LogWarning($"�ڽ�Ƭ UID {costumeUid}�� �����ϰ� ���� �ʽ��ϴ�.");
            return false;
        }

        //// �ش� ������ ���� �ڽ�Ƭ ����
        //_temporaryCostumes.RemoveAll(item => item.CostumeType == costumeType);

        //// �ش� ������ ��������Ʈ ����
        //foreach (CostumeItem costume in _temporaryCostumes)
        //{
        //    foreach (var partData in costume.Parts)
        //    {
        //        // ���� ������ ��������Ʈ�� ����ϴ� �ٸ� �ڽ�Ƭ�� ����
        //        if (newCostume.Parts.Any(p => p.Part == partData.Part))
        //        {
        //            _characterRenderer.ResetPartItem(partData.Part);
        //        }
        //    }
        //}

        // �� �ڽ�Ƭ ���� ����
        foreach (var partData in newCostume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        // �� �ڽ�Ƭ �ӽ� ��Ͽ� �߰�
        //_temporaryCostumes.Add(newCostume);

        // �ӽ� ���� ��� ������Ʈ (UI �׽�Ʈ��)
        EquipedCostumes.RemoveAll(uid =>
            AllCostumeDatas.Any(item => item.Uid == uid && item.CostumeType == costumeType));
        EquipedCostumes.Add(costumeUid);

        //���þ�����Ʈ
        UpdateGameAppearanceData();

        //���� ������ ������Ʈ 
        UpdateCostumeData();

        return true;
    }

    /// <summary>
    /// Ư�� �ڽ�Ƭ �����ϱ�
    /// </summary>
    public bool UnequipCostume(string costumeUid)
    {
        // UID�� �ڽ�Ƭ ã��
        CostumeItem costume = AllCostumeDatas.FirstOrDefault(item => item.Uid == costumeUid);

        if (costume == null)
        {
            Debug.LogWarning($"�ڽ�Ƭ UID {costumeUid}�� ã�� �� �����ϴ�.");
            return false;
        }

        // �ڽ�Ƭ ���� ����
        foreach (var partData in costume.Parts)
        {
            _characterRenderer.ResetPartItem(partData.Part);
        }

        // �ӽ� ���� ��� ������Ʈ
        EquipedCostumes.Remove(costumeUid);

        // �̺κ��� �̻��ѵ�?
        // �⺻ ������ ����
        CostumeItem defaultItem = _defaultItems.FirstOrDefault(item => item.CostumeType == costume.CostumeType);
        if (defaultItem != null)
        {
            // ��� ���� ������ ��� �⺻ ��� ���� ����
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
                    Debug.Log("��� ���� ���̹Ƿ� �⺻ �Ӹ� �������� ����");
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

        //���� ������ ������Ʈ 
        UpdateCostumeData();

        return true;
    }

    private void ApplyDefaultItem(CostumeItem defaultItem)
    {
        if (defaultItem == null || _characterRenderer == null) return;

        // �������� ��� ������ ĳ���Ϳ� ����
        foreach (var partData in defaultItem.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        Debug.Log($"�⺻ ������ '{defaultItem.Name}' �����");
    }
    public void UpdateGameAppearanceData() => _characterRenderer.UpdateGameAppearanceData();

}