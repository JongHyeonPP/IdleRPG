using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("SetInInspector")]
    [SerializeField] EnemyStorage storage;//������ ������ �ϰ������� �����ϰ� �ִ� ���
    //�� ���� ���� ���� �������� ������Ʈ Ǯ�� �� ���� ����
    [SerializeField] EnemyPool _pool_0;
    [SerializeField] EnemyPool _pool_1;
    [SerializeField] Transform _spawnSpot;//Ǯ���� ������Ʈ�� Ȱ��ȭ�Ǹ鼭 ���� ��ġ ����
    PlayerContoller _controller;//GameManager�κ��� ���� controller ����
    [SerializeField] List<BackgroundPiece> _pieces;//��ġ�� ���������� �����ϸ鼭 ���� ��� �̹�����
    EnemyController[] _enemies = new EnemyController[enemyBundleNum];//���� ���ͼ� ���� ��� ���� ���� ����
    private bool isMove;//ĳ���Ͱ� �����̰� �ִ���. �����δ� ����� ����� �����̰� �ִ������ �� �� �ִ�.
    private float speed = 3f;//�����̴� �ӵ�
    private float enemySpace = 1f;// enemies�� �迭 �� ĭ�� ������ �������� �Ǵ� x ����
    private static int enemyBundleNum = 10;//���� ��ġ�� �� �ִ� �迭�� ũ��. ������ ���� �� �� �Ҵ������ DetermineEnemyNum�� �����Ѵ�.
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
        if (_pool_1.pool == null)
        {
            // pool_1�� ���� �� pool_0������ ����
            return _pool_0.GetFromPool();
        }
        else
        {
            // �� Ǯ���� 50% Ȯ���� ����
            return UtilityManager.CalculateProbability(0.5f) ? _pool_0.GetFromPool() : _pool_1.GetFromPool();
        }
    }

    // ���� ��ġ�� �����ϴ� �޼���
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
