using TMPro;
using UnityEngine;

//Battle ���� �ִ� ���� UI�� ����Ѵ�. �� ī�װ��� ���� UI���� �����ϵ� �ֿ� ����� ���� ���õ� ��ũ��Ʈ �ۼ�
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    //UGUI ������Ʈ���̶� ������Ѵ�.
    [SerializeField] GameObject _upperUi;
    [SerializeField] GameObject _duplicateLoginPanel;
    [SerializeField] TMP_Text _nameText;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitUiState();
        }
    }
    private void InitUiState()
    {
        _upperUi.SetActive(true);
        _nameText.gameObject.SetActive(true);
        _duplicateLoginPanel.SetActive(false);
        NetworkManager.OnDetectDuplicateLogin += OnDetectDuplicateLogin;
    }
    //�ߺ� �α����� ���� �� �ݹ�Ǵ� �޼���
    private void OnDetectDuplicateLogin()
    {
        _duplicateLoginPanel.SetActive(true);
    }

    private void Start()
    {
        _nameText.text = GameManager.userName;
    }
    //������ ���� ��ư�� Ŭ������ �� �ߵ��Ǵ� �޼���
    public void OnSaveButtonClicked()
    {
        GameManager.instance.SaveGameDataToCloud();
    }
}
