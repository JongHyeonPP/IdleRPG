using UnityEngine;

public class RenderCamera : MonoBehaviour
{
    private Camera _camera;
    private RenderTexture _renderTexture;

    [SerializeField] private GameObject playerPrefab; 
    [SerializeField] private GameObject pigPrefab;
    private GameObject playerInstance;  
    private GameObject pigInstance;    
    private Transform _currentTarget;
    public Transform playerspawnPoint;
    public Transform otherspawnPoint;
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        CreateRenderTexture();
    }

    public void SetCharacterDisplayTarget(string targetName)
    {
        if (targetName == "³ª" && playerInstance != null)
        {
            _currentTarget = playerInstance.transform;
        }
        else if (targetName == "µÅÁö" && pigInstance != null)
        {
            _currentTarget = pigInstance.transform;
        }
        

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (_currentTarget == null) return;

        _camera.transform.position = _currentTarget.position + new Vector3(0, 2, -3);
        _camera.transform.LookAt(_currentTarget);
    }

    private void CreateRenderTexture()
    {
        _renderTexture = new RenderTexture(512, 512, 16);
        _renderTexture.Create();

        _camera.targetTexture = _renderTexture;
    }

    public RenderTexture GetRenderTexture()
    {
        return _renderTexture;
    }
    public void ClearTarget(string targetName)
    {
        if (targetName == "³ª" && playerInstance != null)
        {
            Destroy(playerInstance);
        }
        else if (targetName == "µÅÁö" && pigInstance != null)
        {
            Destroy(pigInstance);
        }

        _currentTarget = null;
        _camera.targetTexture = null; 
    }
    public void SpawnStoryPlayer(int index)
    {
        if (index == 1)
        {
            if (playerPrefab != null && playerspawnPoint != null)
            {
                playerInstance = Instantiate(playerPrefab, playerspawnPoint.position, playerspawnPoint.rotation);
            }

            if (pigPrefab != null && otherspawnPoint != null)
            {
                pigInstance = Instantiate(pigPrefab, otherspawnPoint.position, otherspawnPoint.rotation);
            }
        }
        
    }
}
