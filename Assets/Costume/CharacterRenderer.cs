using System.Collections.Generic;
using UnityEngine;

public enum BodyPart
{
    // 머리 관련
    Head,
    FaceHair,
    Hair,
    Helmet,

    // 몸통 관련
    Body,
    Arm_R,
    Arm_L,
    Pant_R,
    Pant_L
}

public class CharacterRenderer : MonoBehaviour
{
    [Header("Character Parts")]
    [SerializeField] private SpriteRenderer _headRenderer;
    [SerializeField] private SpriteRenderer _faceHairRenderer;
    [SerializeField] private SpriteRenderer _hairRenderer;
    [SerializeField] private SpriteRenderer _helmetRenderer;

    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private SpriteRenderer _armRRenderer;
    [SerializeField] private SpriteRenderer _armLRenderer;
    [SerializeField] private SpriteRenderer _pantRRenderer;
    [SerializeField] private SpriteRenderer _pantLRenderer;

    private Dictionary<BodyPart, SpriteRenderer> _partRenderers;

    private void Awake()
    {
        _partRenderers = new Dictionary<BodyPart, SpriteRenderer>
        {
            { BodyPart.Head, _headRenderer },
            { BodyPart.FaceHair, _faceHairRenderer },
            { BodyPart.Hair, _hairRenderer },
            { BodyPart.Helmet, _helmetRenderer },

            { BodyPart.Body, _bodyRenderer },
            { BodyPart.Arm_R, _armRRenderer },
            { BodyPart.Arm_L, _armLRenderer },
            { BodyPart.Pant_R, _pantRRenderer },
            { BodyPart.Pant_L, _pantLRenderer }
        };
    }

    /// <summary>
    /// 특정 부위에 아이템 적용
    /// </summary>
    public void AppItem(BodyPart part, Sprite sprite, Color color)
    {
        if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
        {
            renderer.sprite = sprite;
            renderer.color = color;
        }
    }

    /// <summary>
    /// 특정 부위 아이템 초기화
    /// </summary>
    public void ResetPartItem(BodyPart part)
    {
        if (_partRenderers.TryGetValue(part, out SpriteRenderer renderer) && renderer != null)
        {
            renderer.sprite = null;
        }
    }

    /// <summary>
    /// 모든 부위 아이템 초기화
    /// </summary>
    public void ResetAllItems()
    {
        foreach (var renderer in _partRenderers.Values)
        {
            if (renderer != null)
            {
                renderer.sprite = null;
            }
        }
    }
}
