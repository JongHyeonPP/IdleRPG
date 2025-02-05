using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Camera storyCamera;
    public Transform storyBackground;
    public Vector3 storyCameraOffset = new Vector3(0, 5, -10);
    public Vector3 storyBackgroundOffset = Vector3.zero;

    private void Start()
    {
        mainCamera.enabled = true;
        storyCamera.enabled = false;
    }

    public void SwitchToCamera(bool ismaincamera)//true는 메인카메라활성화,false는 스토리카메라활성화
    {
        mainCamera.enabled = ismaincamera;
        storyCamera.enabled = !ismaincamera;
        if (!ismaincamera)
        {
          PositionStoryCamera();
        }
    }

    private void PositionStoryCamera()
    {
        if (storyBackground != null)
        {
           storyCamera.transform.position = storyBackground.position + storyCameraOffset;
           storyCamera.transform.LookAt(storyBackground.position + storyBackgroundOffset);
        }

    }
    

}
