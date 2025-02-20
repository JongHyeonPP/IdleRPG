using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CostumeManager : MonoSingleton<CostumeManager>
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _costumeSlotPrefab;                                      // �ڽ�Ƭ ������ ������
    [SerializeField] private Transform _costumeGridParent;                                       // �ڽ�Ƭ ������ �θ� ��ġ

    [Header("Character Data")]
    [SerializeField] private CharacterRenderer _characterRenderer;                               // ĳ���� ������
    [SerializeField] private List<CostumeItem> _availableCostumes;                               // ���� �ڽ�Ƭ ����Ʈ

    [SerializeField] private List<CostumeItem> _temporaryCostumes = new List<CostumeItem>();     // �ӽ÷� ������ �ڽ�Ƭ ����Ʈ
    [SerializeField] private List<CostumeItem> _savedCostumes = new List<CostumeItem>();         // ����� �ڽ�Ƭ ����Ʈ

    public CostumeItem[] AllCostumeDatas;                                                        // ��� �ڽ�Ƭ �����͵�

    private void Start()
    {
        // ���� �ڽ�Ƭ �������� ��� ��������
        // ������ new�� �ʱ�ȭ
        if(_savedCostumes == null)  
            _savedCostumes = new List<CostumeItem>(_temporaryCostumes);

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
    private void EquipCostume(CostumeItem costume)
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

        Debug.Log("Costume �����");
    }

    /// <summary>
    /// ��� �ڽ�Ƭ �ƿ� ��ü �A�ɷ� �ʱ�ȭ
    /// </summary>
    public void ResetAllCostumes()
    {
        _characterRenderer.ResetAllItems();
        _temporaryCostumes = new List<CostumeItem>();
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
}
