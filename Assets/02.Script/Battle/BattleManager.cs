using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    
    public static BattleManager instance;

    [SerializeField] EnemyStorage storage;//적들의 정보를 일괄적으로 보관하고 있는 장소
    //두 가지 적이 섞여 나오도록 오브젝트 풀을 두 가지 생성
    [SerializeField] EnemyPool _pool_0;
    [SerializeField] EnemyPool _pool_1;
    [SerializeField] Transform _spawnSpot;//풀링된 오브젝트가 활성화되면서 나올 위치 정보
    [SerializeField] Transform _poolParent;//비활성화 돼 풀에 들어간 오브젝트가 들어갈 공간
    PlayerContoller _controller;//GameManager로부터 얻어온 controller 정보
    [SerializeField] List<BackgroundPiece> _pieces;//위치를 지속적으로 변경하면서 보일 배경 이미지들
    private EnemyController[] _enemies = null;//현재 나와서 전투 대기 중인 적들 정보
    private int _lastEnemyIndex;//_enemies 배열 중 끝에 있는 적의 인덱스
    private float enemyPlayerDistance = 1.5f;//멈춰서 공격하는 시점의 적과 나의 간격
    private bool isMove;//캐릭터가 움직이고 있는지. 실제로는 적들과 배경이 움직이고 있는지라고 할 수 있다.
    private float speed = 3f;//움직이는 속도
    private float _enemySpace = 1f;// enemies의 배열 한 칸당 실제로 떨어지게 되는 x 간격
    private int _enemyBundleNum = 10;//적이 위치할 수 있는 배열의 크기. 실제로 적이 몇 명 할당될지는 DetermineEnemyNum가 정의한다.
    private int _currentTargetIndex;//현재 마주보고 있는 캐릭터
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
        _pool_0.poolParent = _pool_1.poolParent = _poolParent;
        StartBattle();
        GameManager.instance.AutoSaveStart();
    }

    public void StartBattle()
    {
        isMove = true;
        _controller.MoveState(true);
        _currentTargetIndex = 0;
        InitPools();
        StartCoroutine(BattleLoop());
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
                EnemyBackgroundMove();
                isMove = true;
            }
        }
    }
    void NoTargetCase()
    {
        //할당된 적을 다 처치했으면 다시 할당한다.
        if (_enemies == null ||_currentTargetIndex>=_lastEnemyIndex)
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
    public void EnemyBackgroundMove()
    {
        foreach (EnemyController enemy in _enemies)
        {
            if (enemy)
                enemy.transform.Translate(speed * Time.deltaTime * Vector2.left);
        }
        foreach (var x in _pieces)
        {
            x.Move(speed);
        }
    }
    //풀을 비우고 새로운 적들을 생성해서 풀에 할당한다.
    public void InitPools()
    {
        _pool_0.ClearPool();
        _pool_1.ClearPool();
        switch (GameManager.mainStageNum)
        {
            case 0:
                _pool_0.InitializePool(storage.pinkPig);
                break;
        }
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
        if (_pool_1.pool == null)
        {
            // pool_1이 없을 때 pool_0에서만 선택
            return _pool_0.GetFromPool();
        }
        else
        {
            // 두 풀에서 50% 확률로 선택
            return UtilityManager.CalculateProbability(0.5f) ? _pool_0.GetFromPool() : _pool_1.GetFromPool();
        }
    }

    // 적의 위치를 설정하는 메서드
    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(_spawnSpot);
        enemy.transform.localPosition = new Vector2(_enemySpace * index, 0);
    }

    private static int DetermineEnemyNum()
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
}
