using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("SetInInspector")]
    [SerializeField] EnemyStorage storage;
    [SerializeField] EnemyPool pool_0;
    [SerializeField] EnemyPool pool_1;
    [SerializeField] Transform spawnSpot;
    PlayerContoller controller;
    [SerializeField] List<BackgroundPiece> pieces;

    EnemyController[] enemies = new EnemyController[enemyBundleNum];
    bool isMove;
    public const float speed = 3f;
    public const int enemyBundleNum = 10;
    private float enemySpace = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        controller = GameManager.instance.controller;
        StartBattle();
        GameManager.instance.AutoSaveStart();
    }

    public void StartBattle()
    {
        isMove = true;
        controller.MoveState(true);
        InitPools();
        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (true)
        {
            if (controller.target == null)
            {
                enemies = MakeEnemies();
                foreach (var x in enemies)
                {
                    if (x != null)
                    {
                        controller.target = x;
                        break;
                    }
                }
            }
            else
            {
                float xDistance = controller.target.transform.position.x - controller.transform.position.x;
                if (xDistance < 0.5f)
                {
                    if (isMove)
                    {
                        controller.MoveState(false);
                        controller.StartAttack();
                    }
                    isMove = false;
                }
                else
                {
                    controller.MoveState(true);
                    EnemyBackgroundMove();
                    isMove = true;
                }
            }
            yield return null;
        }
    }

    public void EnemyBackgroundMove()
    {
        foreach (EnemyController enemy in enemies)
        {
            if (enemy)
                enemy.transform.Translate(speed * Time.deltaTime * Vector2.left);
        }
        foreach (var x in pieces)
        {
            x.Move();
        }
    }

    public void InitPools()
    {
        pool_0.ClearPool();
        pool_1.ClearPool();
        switch (GameManager.mainStageNum)
        {
            case 0:
                pool_0.InitializePool(storage.pinkPig, 1);
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
        if (pool_1.pool == null)
        {
            // pool_1이 없을 때 pool_0에서만 선택
            return pool_0.GetFromPool();
        }
        else
        {
            // 두 풀에서 50% 확률로 선택
            return UtilityManager.CalculateProbability(0.5f) ? pool_0.GetFromPool() : pool_1.GetFromPool();
        }
    }

    // 적의 위치를 설정하는 메서드
    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(spawnSpot);
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
    public void UpdownTest(bool isUp)
    {
        GameManager.instance.ChangeStatLevel(StatusType.Power, 0, isUp);
    }
}
