using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    [SerializeField] GameObject upperUi;
    [SerializeField] GameObject canvasPerm;
    [SerializeField] GameObject duplicateLoginPanel;
    [SerializeField] TMP_Text nameText;
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
        upperUi.SetActive(true);
        nameText.gameObject.SetActive(true);
        duplicateLoginPanel.SetActive(false);
        NetworkManager.OnDetectDuplicateLogin += OnDetectDuplicateLogin;
    }

    private void OnDetectDuplicateLogin()
    {
        duplicateLoginPanel.SetActive(true);
    }

    private void Start()
    {
        nameText.text = GameManager.userName;
    }
    public void OnSaveButtonClicked()
    {
        GameManager.instance.SaveGameDataToCloud();
    }
}
