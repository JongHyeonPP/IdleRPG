using UnityEngine;

public class PlayerRendercamera : MonoBehaviour
{
    private Camera _renderCamera;
    private void Awake()
    {
        _renderCamera = GetComponent<Camera>();
        UIBroker.SwitchRenderTargetLayer += SwitchRenderTargetLayer;
        SwitchRenderTargetLayer(new string[] { "RenderTexture_0", "RenderTexture_1", "RenderTexture_2", "RenderTexture_3" });
    }

    private void SwitchRenderTargetLayer(string[] targetLayers )
    {
        _renderCamera.cullingMask = LayerMask.GetMask(targetLayers);
    }
}
