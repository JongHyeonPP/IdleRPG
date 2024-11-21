using UnityEngine;

//Battle ���� �ִ� ���� UI�� ����Ѵ�. �� ī�װ��� ���� UI���� �����ϵ� �ֿ� ����� ���� ���õ� ��ũ��Ʈ �ۼ�
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
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
        StartBroker.OnDetectDuplicateLogin += OnDetectDuplicateLogin;
    }
    //�ߺ� �α����� ���� �� �ݹ�Ǵ� �޼���
    private void OnDetectDuplicateLogin()
    {
    }

    private void Start()
    {
    }
    //������ ���� ��ư�� Ŭ������ �� �ߵ��Ǵ� �޼���
    public void OnSaveButtonClicked()
    {
        GameManager.instance.SaveGameDataToCloud();
    }
}
