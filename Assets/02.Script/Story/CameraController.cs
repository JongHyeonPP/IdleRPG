using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    public Camera storyCamera;
   
    private StoryPlayerController _playercontroller;
    private Transform _playerTransform;
    private bool isStoryCameraActive = false;
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = BattleManager.instance.currentCamera;
        }
        mainCamera.enabled = true;
        storyCamera.enabled = false;
    }
    
    public void SwitchToCamera(bool ismaincamera)//true는 메인카메라활성화,false는 스토리카메라활성화
    {
        Debug.Log("Switching to camera: " + (ismaincamera ? "Main Camera" : "Story Camera"));
        if (mainCamera == null)
        {
            mainCamera = BattleManager.instance.currentCamera;
        }
        mainCamera.enabled = ismaincamera;
        storyCamera.enabled = !ismaincamera;
        isStoryCameraActive = !ismaincamera;
        if (!ismaincamera)
        {
          PositionStoryCamera();
        }
    }
    

    private void PositionStoryCamera()
    {
        foreach (var playercontroller in FindObjectsOfType<StoryPlayerController>())
        {
            if (playercontroller != null)
            {
                _playercontroller = playercontroller;
                _playerTransform = playercontroller.transform;
            }
        }
        storyCamera.transform.position = new Vector3(-4.71f, 17f, storyCamera.transform.position.z); 

       
    }
   
}
