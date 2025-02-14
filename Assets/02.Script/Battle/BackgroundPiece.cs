using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundPiece : MonoBehaviour, IMoveByPlayer
{
    // 배경 오브젝트들
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

    // 현재 활성화된 배경 오브젝트
    private GameObject currentBackground;

    private void Awake()
    {
        // Mediator 등록
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);

        // 배경 맵핑 초기화
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
        // X 좌표가 특정 위치를 벗어나면 재배치
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 63.98f;
        }
    }

    public void ChangeBackground(Background newBackground)
    {
        // 현재 활성화된 배경 비활성화
        if (currentBackground != null)
        {
            currentBackground.SetActive(false);
        }

        // 새로운 배경 활성화
        if (backgroundDict.TryGetValue(newBackground, out GameObject newBackgroundObject) && newBackgroundObject != null)
        {
            newBackgroundObject.SetActive(true);
            currentBackground = newBackgroundObject; // 활성화된 배경 업데이트
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
