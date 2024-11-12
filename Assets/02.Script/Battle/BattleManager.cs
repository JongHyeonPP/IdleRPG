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

    public EnemyController[] MakeEnemies() // Ȱ��ȭ�� ���� ��ȯ
    {
        EnemyController[] result = new EnemyController[enemyBundleNum];
        int currentNum = 0;
        int enemyNum = DetermineEnemyNum();

        // ù ��° ���� 0��° ��ġ�� �������� ����
        result[0] = GetEnemyFromPool();
        PositionEnemy(result[0], 0);
        currentNum++;

        int index = 1; // ������ �ڸ��� 1��°���� ä��

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
            // �ε����� ��ȯ������ �������� �迭 ���� ������ ����
            index = (index + 1) % enemyBundleNum;
        }
        return result;
    }

    // �� ���� Ǯ���� EnemyController ��ü�� �������� �޼���
    private EnemyController GetEnemyFromPool()
    {
        if (pool_1.pool == null)
        {
            // pool_1�� ���� �� pool_0������ ����
            return pool_0.GetFromPool();
        }
        else
        {
            // �� Ǯ���� 50% Ȯ���� ����
            return UtilityManager.CalculateProbability(0.5f) ? pool_0.GetFromPool() : pool_1.GetFromPool();
        }
    }

    // ���� ��ġ�� �����ϴ� �޼���
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
