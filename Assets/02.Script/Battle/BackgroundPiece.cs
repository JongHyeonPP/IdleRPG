using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundPiece : MonoBehaviour, IMoveByPlayer
{
    // ��� ������Ʈ��
    [SerializeField] private GameObject plainObject;
    [SerializeField] private GameObject beachObject;
    [SerializeField] private GameObject caveObject;
    [SerializeField] private GameObject desertObject;
    [SerializeField] private GameObject desertRuinsObject;
    [SerializeField] private GameObject elfCityObject;
    [SerializeField] private GameObject forestObject;
    [SerializeField] private GameObject iceFieldObject;
    [SerializeField] private GameObject lavaObject;
    [SerializeField] private GameObject mysteriousForestObject;
    [SerializeField] private GameObject plainsObject;
    [SerializeField] private GameObject redRockObject;
    [SerializeField] private GameObject ruinsObject;
    [SerializeField] private GameObject swampObject;
    [SerializeField] private GameObject vineForestObject;
    [SerializeField] private GameObject winterForestObject;

    private Dictionary<Background, GameObject> backgroundDict = new();

    // ���� Ȱ��ȭ�� ��� ������Ʈ
    private GameObject currentBackground;

    private void Awake()
    {
        // Mediator ���
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);

        // ��� ���� �ʱ�ȭ
        backgroundDict = new Dictionary<Background, GameObject>
        {
            { Background.Beach, beachObject },
            { Background.Cave, caveObject },
            { Background.Desert, desertObject },
            { Background.DesertRuins, desertRuinsObject },
            { Background.ElfCity, elfCityObject },
            { Background.Forest, forestObject },
            { Background.IceField, iceFieldObject },
            { Background.Lava, lavaObject },
            { Background.MysteriousForest, mysteriousForestObject },
            { Background.Plains, plainsObject },
            { Background.RedRock, redRockObject },
            { Background.Ruins, ruinsObject },
            { Background.Swamp, swampObject },
            { Background.VineForest, vineForestObject },
            { Background.WinterForest, winterForestObject }
        };
        foreach (var x in backgroundDict.Values)
        {
            x.SetActive(false);
        }
    }

    private void Update()
    {
        // X ��ǥ�� Ư�� ��ġ�� ����� ���ġ
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 63.98f;
        }
    }

    public void ChangeBackground(Background newBackground)
    {
        // ���� Ȱ��ȭ�� ��� ��Ȱ��ȭ
        if (currentBackground != null)
        {
            currentBackground.SetActive(false);
        }

        // ���ο� ��� Ȱ��ȭ
        if (backgroundDict.TryGetValue(newBackground, out GameObject newBackgroundObject) && newBackgroundObject != null)
        {
            newBackgroundObject.SetActive(true);
            currentBackground = newBackgroundObject; // Ȱ��ȭ�� ��� ������Ʈ
        }
        else
        {
            Debug.LogWarning($"Background '{newBackground}' does not have a valid GameObject.");
        }
    }

    public void MoveByCharacter(Vector3 translation)
    {
        transform.Translate(translation);
    }
    private void OnDestroy()
    {
        MediatorManager<IMoveByPlayer>.UnregisterMediator(this);
    }
}
