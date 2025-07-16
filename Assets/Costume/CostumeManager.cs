using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EnumCollection;
using System.Linq;


public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;
    [Header("UI Elements")]
    [SerializeField] private GameObject _costumeSlotPrefab;                                      // �ڽ�Ƭ ������ ������
    [SerializeField] private Transform _costumeGridParent;                                       // �ڽ�Ƭ ������ �θ� ��ġ

    [Header("Character Data")]
    [SerializeField] private CharacterRenderer _characterRenderer;                               // ĳ���� ������
    [SerializeField] private List<CostumeItem> _availableCostumes;                               // ���� �ڽ�Ƭ ����Ʈ

    [SerializeField] private List<CostumeItem> _temporaryCostumes = new List<CostumeItem>();     // �ӽ÷� ������ �ڽ�Ƭ ����Ʈ
    [SerializeField] private List<CostumeItem> _savedCostumes = new List<CostumeItem>();         // ����� �ڽ�Ƭ ����Ʈ

    public CostumeItem[] AllCostumeDatas;                                                        // ��� �ڽ�Ƭ �����͵�
    public List<CostumeItem> TemporaryCostumes => _temporaryCostumes;

    [Header("Default Items")]
    [Tooltip("0: ���, 1: ����, 2: ����, 3: �Ź�")]
    [SerializeField] private CostumeItem[] _defaultItems;                                        // �� ������ �⺻ ������ ����

    public List<string> TestEquipedCostumes = new();                                 // ������ �ڽ�Ƭ��
    public List<string> TestOwnedCostumes = new();

    private void Awake()
    {
        Instance = this;

        // �׽�Ʈ��
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

        // ����� �ڽ�Ƭ ���� ��������
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
            .Where(costume => IsOwend(costume.Uid))
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
        if (!IsOwend(costumeUid))
        {
            Debug.LogWarning($"�ڽ�Ƭ UID {costumeUid}�� �����ϰ� ���� �ʽ��ϴ�.");
            return false;
        }

        // �ش� ������ ���� �ڽ�Ƭ ����
        _temporaryCostumes.RemoveAll(item => item.CostumeType == costumeType);

        // �ش� ������ ��������Ʈ ����
        foreach (CostumeItem costume in _temporaryCostumes)
        {
            foreach (var partData in costume.Parts)
            {
                // ���� ������ ��������Ʈ�� ����ϴ� �ٸ� �ڽ�Ƭ�� ����
                if (newCostume.Parts.Any(p => p.Part == partData.Part))
                {
                    _characterRenderer.ResetPartItem(partData.Part);
                }
            }
        }

        // �� �ڽ�Ƭ ���� ����
        foreach (var partData in newCostume.Parts)
        {
            _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
        }

        // �� �ڽ�Ƭ �ӽ� ��Ͽ� �߰�
        _temporaryCostumes.Add(newCostume);

        // �ӽ� ���� ��� ������Ʈ (UI �׽�Ʈ��)
        TestEquipedCostumes.RemoveAll(uid =>
            AllCostumeDatas.Any(item => item.Uid == uid && item.CostumeType == costumeType));
        TestEquipedCostumes.Add(costumeUid);

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
        TestEquipedCostumes.Remove(costumeUid);

        // �⺻ ������ ����
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

    #region �����ڵ�
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
                icon.color = costume.IconColor;
                name.text = costume.Name;
            }

            // Ŭ���ϸ� �ش� �ڽ�Ƭ ����
            slotButton.onClick.AddListener(() => EquipCostume(costume));
        }
    }

    /// <summary>
    /// Ư�� �ڽ�Ƭ ���� (���� ������ ���� ����)
    /// </summary>
    public void EquipCostume(CostumeItem costume)
    {
        if (_characterRenderer == null) return;

        // �̹� �ӽ� ����Ʈ�� �ִٸ� ����, �ƴϸ� �߰�
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
    /// ���� �ڽ�Ƭ ���¸� �����Ŵ
    /// </summary>
    public void SaveCostumeChanges()
    {
        _savedCostumes = new List<CostumeItem>(_temporaryCostumes);
        // _savedCostumes�� ������ ����

        // ������ �ڽ�Ƭ UID ����Ʈ ����
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
        Debug.Log("[CostumeManager: Costume ������ �����]");
    }

    /// <summary>
    /// ��� �ڽ�Ƭ �ƿ� ��ü �A�ɷ� �ʱ�ȭ�ϰ� �⺻ ������ ����
    /// </summary>
    public void ResetAllCostumes()
    {
        _characterRenderer.ResetAllItems();
        _temporaryCostumes = new List<CostumeItem>();

        // ��� �⺻ ������ ����
        ApplyDefaultItem(_defaultItems[0]);
        ApplyDefaultItem(_defaultItems[1]);
        ApplyDefaultItem(_defaultItems[2]);
        ApplyDefaultItem(_defaultItems[3]);
    }

    /// <summary>
    /// Ư�� ������ �ڽ�Ƭ�� �����ϰ� �ش� ������ �⺻ ������ ����
    /// </summary>
    public void ResetPartCostumes(CostumeItem costumeItem)
    {
        // �ش� ������ �ڽ�Ƭ ������ ����
        _temporaryCostumes.RemoveAll(item => item.CostumeType == costumeItem.CostumeType);

        CostumeItem defaultItem = null;

        // �ش� ���� ����Ʈ ������ ����
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

        // �⺻ ������ ����
        ApplyDefaultItem(defaultItem);
    }

    /// <summary>
    /// �⺻ �������� ĳ���Ϳ� �����ϰ� �ӽ� ��Ͽ� �߰��ϴ� �Լ�
    /// </summary>
    private void ApplyDefaultItem(CostumeItem defaultItem)
    {
        if (defaultItem != null)
        {
            // �������� ��� ������ ĳ���Ϳ� ����
            foreach (var partData in defaultItem.Parts)
            {
                _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
            }
        }
    }

    /// <summary>
    /// �������� ���� ��������� ����ϰ� ������ ���� ���·� ����
    /// </summary>
    public void CancelCostumeChanges()
    {
        // �ӽ� ����Ʈ�� ������ ����� ���·� ����
        _temporaryCostumes = new List<CostumeItem>(_savedCostumes);

        // ĳ���� ��������Ʈ ������
        _characterRenderer.ResetAllItems();
        foreach (var savedCostume in _savedCostumes)
        {
            foreach (var partData in savedCostume.Parts)
            {
                _characterRenderer.AppItem(partData.Part, partData.CostumeSprite, partData.CostumeColor);
            }
        }

        Debug.Log("�ڽ�Ƭ ������ ���� ���·� ���ư�");
    }
    #endregion
}
