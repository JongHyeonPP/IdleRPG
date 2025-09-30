using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배경 오브젝트를 관리하고, 플레이어 이동에 따라 스크롤되는 컴포넌트.
/// MediatorManager를 통해 플레이어 이동 신호를 받아 배경을 움직이며,
/// 스테이지 전환 시 새로운 배경으로 교체한다.
/// </summary>
public class BackgroundPiece : MonoBehaviour, IMoveByPlayer
{
    // Inspector에서 할당되는 배경 오브젝트들
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

    // 배경 enum → GameObject 매핑
    private Dictionary<Background, GameObject> backgroundDict = new();

    // 현재 활성화되어 있는 배경 오브젝트
    private GameObject currentBackground;

    private void Awake()
    {
        // Mediator에 자신 등록 (플레이어 이동 시 호출되도록)
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);

        // 배경 Dictionary 초기화
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

        // 시작 시 모든 배경 비활성화
        foreach (var obj in backgroundDict.Values)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {
        // 배경이 일정 위치 밖으로 나가면 오른쪽으로 재배치
        // → 무한 스크롤 효과를 주기 위함
        if (transform.position.x < -20f)
        {
            transform.localPosition += Vector3.right * 63.98f;
        }
    }

    /// <summary>
    /// 배경 전환: 현재 배경을 끄고, 새 배경을 켠다.
    /// </summary>
    public void ChangeBackground(Background newBackground)
    {
        // 현재 배경 비활성화
        if (currentBackground != null)
        {
            currentBackground.SetActive(false);
        }

        // 새로운 배경 활성화
        if (backgroundDict.TryGetValue(newBackground, out GameObject newBackgroundObject) && newBackgroundObject != null)
        {
            newBackgroundObject.SetActive(true);
            currentBackground = newBackgroundObject;
        }
        else
        {
            Debug.LogWarning($"Background '{newBackground}' does not have a valid GameObject.");
        }
    }

    /// <summary>
    /// 플레이어 이동에 따라 배경 이동 (IMoveByPlayer 인터페이스 구현)
    /// </summary>
    public void MoveByPlayer(Vector3 translation)
    {
        transform.Translate(translation);
    }

    private void OnDestroy()
    {
        // 객체 파괴 시 Mediator에서 해제
        MediatorManager<IMoveByPlayer>.UnregisterMediator(this);
    }
}
