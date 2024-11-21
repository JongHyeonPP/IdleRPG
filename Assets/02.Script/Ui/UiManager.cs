using UnityEngine;

//Battle 씬에 있는 뼈대 UI를 담당한다. 각 카테고리로 나눈 UI들의 관리하되 주요 기능은 각각 관련된 스크립트 작성
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
    //중복 로그인이 됐을 때 콜백되는 메서드
    private void OnDetectDuplicateLogin()
    {
    }

    private void Start()
    {
    }
    //서버에 저장 버튼을 클릭했을 때 발동되는 메서드
    public void OnSaveButtonClicked()
    {
        GameManager.instance.SaveGameDataToCloud();
    }
}
