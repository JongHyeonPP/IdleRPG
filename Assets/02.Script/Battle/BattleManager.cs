using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{

    public static BattleManager instance;

    [Header("Enemy Pool")]
    [SerializeField] EnemyStorage storage;//적들의 정보를 일괄적으로 보관하고 있는 장소
    //두 가지 적이 섞여 나오도록 오브젝트 풀을 두 가지 생성
    [SerializeField] EnemyPool _ePool0;
    [SerializeField] EnemyPool _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] Transform _spawnSpot;//풀링된 오브젝트가 활성화되면서 나올 위치 정보
    [SerializeField] Transform _poolParent;//비활성화 돼 풀에 들어간 오브젝트가 들어갈 공간
    [Header("Etc")]
    PlayerContoller _controller;//GameManager로부터 얻어온 controller 정보
    [SerializeField] List<BackgroundPiece> _pieces;//위치를 지속적으로 변경하면서 보일 배경 이미지들
    private EnemyController[] _enemies = null;//현재 나와서 전투 대기 중인 적들 정보
    private int _lastEnemyIndex;//_enemies 배열 중 끝에 있는 적의 인덱스
    private float enemyPlayerDistance = 1f;//멈춰서 공격하는 시점의 적과 나의 간격
    private bool isMove;//캐릭터가 움직이고 있는지. 실제로는 적들과 배경이 움직이고 있는지라고 할 수 있다.
    private float speed = 3f;//움직이는 속도
    private float _enemySpace = 1f;// enemies의 배열 한 칸당 실제로 떨어지게 되는 x 간격
    private int _enemyBundleNum = 10;//적이 위치할 수 있는 배열의 크기. 실제로 적이 몇 명 할당될지는 DetermineEnemyNum가 정의한다.
    private int _currentTargetIndex;//현재 마주보고 있는 캐릭터

    private GameData _gameData;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        _controller = GameManager.controller;
        _ePool0.poolParent = _ePool1.poolParent = _poolParent;
        StartBattle();
        GameManager.instance.AutoSaveStart();
        _gameData = GameManager.instance.gameData;

    }

    public void StartBattle()
    {
        isMove = true;
        _controller.MoveState(true);
        _currentTargetIndex = 0;
        InitPools();
        StartCoroutine(BattleLoop());
        BattleBroker.OnEnemyDead += OnEnemyDead;
    }

    private IEnumerator BattleLoop()
    {
        while (true)
        {
            if (_controller.target)
            {
                //타겟이 있을 경우
                TargetCase();
            }
            else
            {
                //타겟이 없을 경우
                NoTargetCase();
            }
            yield return null;
        }
        void TargetCase()
        {
            float xDistance = _controller.target.transform.position.x - _controller.transform.position.x;
            if (xDistance < enemyPlayerDistance)
            {
                if (isMove)
                {
                    _controller.MoveState(false);
                    _controller.StartAttack();
                }
                isMove = false;
            }
            else
            {
                _controller.MoveState(true);
                MoveByPlayer();
                isMove = true;
            }
        }
    }
    void NoTargetCase()
    {
        //할당된 적을 다 처치했으면 다시 할당한다.
        if (_enemies == null || _currentTargetIndex >= _lastEnemyIndex)
        {
            _enemies = MakeEnemies();
            _currentTargetIndex = 0;
        }
        //다음 타겟을 찾는다.
        EnemyController target = null;
        while (true)
        {
            if (_enemies[_currentTargetIndex] != null)
            {
                target = _enemies[_currentTargetIndex];
                _controller.target = target;
                break;
            }
            _currentTargetIndex++;
        }
    }
    public void MoveByPlayer()
    {
        List<IMoveByPlayer> ros = MediatorManager<IMoveByPlayer>.GetRegisteredObjects();
        foreach (IMoveByPlayer ro in ros)
        {
            if (ro.Transform.gameObject.activeSelf)
                ro.Transform.Translate(speed * Time.deltaTime * Vector2.left);
        }
    }
    //풀을 비우고 새로운 적들을 생성해서 풀에 할당한다.
    private void InitPools()
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        switch (GameManager.mainStageNum)
        {
            case 0:
                _ePool0.InitializePool(storage.pinkPig);
                break;
        }
    }
    private int DetermineEnemyNum()
    {
        int enemyNum = 0;
        int stageNum = GameManager.mainStageNum;
        if (stageNum == 0)
        {
            enemyNum = 4;
        }
        else if (stageNum <= 5)
        {
            enemyNum = 4;
        }
        else
        {
            enemyNum = 5;
        }
        return enemyNum;
    }
    // 활성화된 적들 반환
    public EnemyController[] MakeEnemies()
    {
        EnemyController[] result = new EnemyController[_enemyBundleNum];
        int currentNum = 0; // 현재 적이 얼마나 생겼는지
        int enemyNum = DetermineEnemyNum();

        int index = 0; // 적이 배열에 들어간 인덱스

        while (currentNum < enemyNum)
        {
            if ((index == 0 || UtilityManager.CalculateProbability(0.5f)) && (result[index] == null))
            {
                EnemyController enemyFromPool = GetEnemyFromPool();
                if (enemyFromPool != null)
                {
                    result[index] = enemyFromPool;
                    PositionEnemy(enemyFromPool, index);
                    enemyFromPool.SetCurrentInfo(result, index);
                    currentNum++;
                }
            }
            // 인덱스를 순환적으로 증가시켜 배열 범위 내에서 유지
            index = (index + 1) % _enemyBundleNum;
        }

        // lastEnemyIndex를 설정
        _lastEnemyIndex = -1;
        for (int i = result.Length - 1; i >= 0; i--)
        {
            if (result[i] != null)
            {
                _lastEnemyIndex = i;
                break;
            }
        }

        return result;
    }


    // 두 개의 풀에서 EnemyController 객체를 가져오는 메서드
    private EnemyController GetEnemyFromPool()
    {
        if (_ePool1.pool == null)
        {
            // pool_1이 없을 때 pool_0에서만 선택
            return _ePool0.GetFromPool();
        }
        else
        {
            // 두 풀에서 50% 확률로 선택
            return UtilityManager.CalculateProbability(0.5f) ? _ePool0.GetFromPool() : _ePool1.GetFromPool();
        }
    }

    // 적의 위치를 설정하는 메서드
    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(_spawnSpot);
        enemy.transform.localPosition = new Vector2(_enemySpace * index, 0);
    }


    private void OnEnemyDead(Vector3 position)
    {
        DropBase dropBase;
        switch (UtilityManager.CalculateProbability(0.5f))
        {
            case true:
                dropBase = _dPool.GetFromPool<GoldDrop>();
                break;
            case false:
                dropBase = _dPool.GetFromPool<ExpDrop>();
                break;
        }
        dropBase.transform.position = position + Vector3.up * 0.2f;
        dropBase.AddForceDiagonally();
    }
    //MainStageNum을 변경하고 거기에 맞는 적들과 배경을 세팅한다.
    private void ChangeMainStage(int stageNum)
    {
        Background background;
        switch (stageNum / 5)
        {
            case 0: // 0~4
                background = Background.Beach;
                break;
            case 1: // 5~9
                background = Background.Cave;
                break;
            case 2: // 10~14
                background = Background.Desert;
                break;
            case 3: // 15~19
                background = Background.DesertRuins;
                break;
            case 4: // 20~24
                background = Background.ElfCity;
                break;
            case 5: // 25~29
                background = Background.Forest;
                break;
            case 6: // 30~34
                background = Background.IceField;
                break;
            case 7: // 35~39
                background = Background.Lava;
                break;
            case 8: // 40~44
                background = Background.MysteriousForest;
                break;
            case 9: // 45~49
                background = Background.Plains;
                break;
            case 10: // 50~54
                background = Background.RedRock;
                break;
            case 11: // 55~59
                background = Background.Ruins;
                break;
            case 12: // 60~64
                background = Background.Swamp;
                break;
            case 13: // 65~69
                background = Background.VineForest;
                break;
            default: // 70 이상
                background = Background.WinterForest; // 기본값 또는 원하는 값
                break;
        }
        foreach (BackgroundPiece piece in _pieces)
        {
            piece.ChangeBackground(background);
        }
    }
}