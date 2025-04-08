using System;
using UnityEngine;

public class MainBattleCamera : MonoBehaviour
{
    [SerializeField] CameraInfo expandInfo;
    [SerializeField] CameraInfo shrinkInfo;
    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        UIBroker.OnMenuUIChange += OnMenuUIChange;
    }
    private void SetSize(CameraInfo cameraInfo)
    {
        mainCamera.transform.position = new(cameraInfo.position.x, cameraInfo.position.y, cameraInfo.position.z);
        mainCamera.orthographicSize = expandInfo.orthographicSize;
        UIBroker.SetPlayerBarPosition();
    }
    [Serializable]
    public class CameraInfo
    {
        public Vector3 position;
        public float orthographicSize;
    }
    private void OnMenuUIChange(int index)
    {
        switch (index)
        {
            default:
                SetSize(expandInfo);
                break;
            case 0:
            case 1:
            case 2:
                SetSize(shrinkInfo);
                break;
        }
    }
}
