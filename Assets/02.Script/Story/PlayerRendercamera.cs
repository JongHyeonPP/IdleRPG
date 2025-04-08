using System.Collections.Generic;
using UnityEngine;

public class PlayerRendercamera : MonoBehaviour
{
    private Camera _renderCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject[] targetObjects;
    private string[] _targetLayers;
    private Dictionary<string, int> _targetNameIndex;
    private void Awake()
    {
        _renderCamera = GetComponent<Camera>();
        UIBroker.SwitchRenderTargetLayer += SwitchRenderTargetLayer;
        //   SwitchRenderTargetLayer(new string[] { "RenderTexture_0", "RenderTexture_1", "RenderTexture_2", "RenderTexture_3" });
        SetTargetLayers();
        InitializeTargetIndex();
        SwitchRenderTargetLayer(_targetLayers);
        _renderCamera.targetTexture = renderTexture;
    }

    private void SwitchRenderTargetLayer(string[] targetLayers )
    {
        _renderCamera.cullingMask = LayerMask.GetMask(targetLayers);
    }
    private void SetTargetLayers()
    {
        _targetLayers = new string[] { "RenderTexture_0", "RenderTexture_1", "RenderTexture_2", "RenderTexture_3", "RenderTexture_4" };
    }
    private void InitializeTargetIndex()
    {
        _targetNameIndex = new Dictionary<string, int>
        {
            { "³ª", 0 },  
            { "µÅÁö", 4 } 
        };
    }
    public void SetCharacterDisplayTarget(string targetName)
    {
        if (!_targetNameIndex.ContainsKey(targetName))
        {
            return;
        }

        int targetIndex = _targetNameIndex[targetName];
        SetCharacterDisplayTarget(targetIndex);
    }
    public void SetCharacterDisplayTarget(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= targetObjects.Length) return;

        GameObject target = targetObjects[targetIndex];
        if (targetIndex == _targetNameIndex["³ª"])
        {
            _renderCamera.transform.localPosition = new Vector3(-1.85f, 1.56f, -1);
        }
        else if (targetIndex == _targetNameIndex["µÅÁö"])
        {
            _renderCamera.transform.localPosition = new Vector3(-0.16f, 1.56f, -1);
        }

        _renderCamera.transform.LookAt(target.transform);
        _renderCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        if (targetIndex == _targetNameIndex["³ª"])  
        {
            _renderCamera.cullingMask = LayerMask.GetMask("RenderTexture_0");
        }
        else if (targetIndex == _targetNameIndex["µÅÁö"]) 
        {
            _renderCamera.cullingMask = LayerMask.GetMask("RenderTexture_4");
        }
    }
    public void ClearTarget()
    {
        _renderCamera.targetTexture = null;
    }
    public RenderTexture GetRenderTexture()
    {
        return renderTexture;
    }
}
