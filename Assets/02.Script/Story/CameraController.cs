using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    public Camera storyCamera;
    public Vector3 storyCameraOffset = new Vector3(0, 5, -10);
    public Vector3 storyBackgroundOffset = Vector3.zero;
    private Transform storyBackground;
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = BattleManager.instance.currentCamera;
        }
        mainCamera.enabled = true;
        storyCamera.enabled = false;
    }
    public void SetStoryBackground(Transform background)
    {
        storyBackground = background; 
    }
    public void SwitchToCamera(bool ismaincamera)//true�� ����ī�޶�Ȱ��ȭ,false�� ���丮ī�޶�Ȱ��ȭ
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
