using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("SetInInspector")]
    [SerializeField] EnemyStorage storage;//적들의 정보를 일괄적으로 보관하고 있는 장소
    //두 가지 적이 섞여 나오도록 오브젝트 풀을 두 가지 생성
    [SerializeField] EnemyPool _pool_0;
    [SerializeField] EnemyPool _pool_1;
    [SerializeField] Transform _spawnSpot;//풀링된 오브젝트가 활성화되면서 나올 위치 정보
    PlayerContoller _controller;//GameManager로부터 얻어온 controller 정보
    [SerializeField] List<BackgroundPiece> _pieces;//위치를 지속적으로 변경하면서 보일 배경 이미지들
    EnemyController[] _enemies = new EnemyController[enemyBundleNum];//현재 나와서 전투 대기 중인 적들 정보
    private bool isMove;//캐릭터가 움직이고 있는지. 실제로는 적들과 배경이 움직이고 있는지라고 할 수 있다.
    private float speed = 3f;//움직이는 속도
    private float enemySpace = 1f;// enemies의 배열 한 칸당 실제로 떨어지게 되는 x 간격
    private static int enemyBundleNum = 10;//적이 위치할 수 있는 배열의 크기. 실제로 적이 몇 명 할당될지는 DetermineEnemyNum가 정의한다.
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
        StartBattle();
        GameManager.instance.AutoSaveStart();
    }

    public void StartBattle()
    {
        isMove = true;
        _controller.MoveState(true);
        InitPools();
        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (true)
        {
            if (_controller.target == null)
            {
                _enemies = MakeEnemies();
                foreach (var x in _enemies)
                {
                    if (x != null)
                    {
                        _controller.target = x;
                        break;
                    }
                }
            }
            else
            {
                float xDistance = _controller.target.transform.position.x - _controller.transform.position.x;
                if (xDistance < 0.5f)
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
            yield return null;
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

    public void InitPools()
    {
        _pool_0.ClearPool();
        _pool_1.ClearPool();
        switch (GameManager.mainStageNum)
        {
            case 0:
                _pool_0.InitializePool(storage.pinkPig, 1);
                break;
        }
    }

    public EnemyController[] MakeEnemies() // 활성화된 적들 반환
    {
        EnemyController[] result = new EnemyController[enemyBundleNum];
        int currentNum = 0;
        int enemyNum = DetermineEnemyNum();

        // 첫 번째 적을 0번째 위치에 고정으로 생성
        result[0] = GetEnemyFromPool();
        PositionEnemy(result[0], 0);
        currentNum++;

        int index = 1; // 나머지 자리는 1번째부터 채움

        while (currentNum < enemyNum)
        {
            if (result[index] == null && UtilityManager.CalculateProbability(0.5f))
            {
                EnemyController selected = GetEnemyFromPool();
                if (selected != null)
                {
                    result[index] = selected;
                    PositionEnemy(selected, index);
                    currentNum++;
                }
            }
            // 인덱스를 순환적으로 증가시켜 배열 범위 내에서 유지
            index = (index + 1) % enemyBundleNum;
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
        enemy.transform.localPosition = new Vector2(enemySpace * index, 0);
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
        return enemyNum;
    }
}
