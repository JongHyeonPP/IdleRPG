using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    
    public static BattleManager instance;

    [SerializeField] EnemyStorage storage;//������ ������ �ϰ������� �����ϰ� �ִ� ���
    //�� ���� ���� ���� �������� ������Ʈ Ǯ�� �� ���� ����
    [SerializeField] EnemyPool _pool_0;
    [SerializeField] EnemyPool _pool_1;
    [SerializeField] Transform _spawnSpot;//Ǯ���� ������Ʈ�� Ȱ��ȭ�Ǹ鼭 ���� ��ġ ����
    [SerializeField] Transform _poolParent;//��Ȱ��ȭ �� Ǯ�� �� ������Ʈ�� �� ����
    PlayerContoller _controller;//GameManager�κ��� ���� controller ����
    [SerializeField] List<BackgroundPiece> _pieces;//��ġ�� ���������� �����ϸ鼭 ���� ��� �̹�����
    private EnemyController[] _enemies = null;//���� ���ͼ� ���� ��� ���� ���� ����
    private int _lastEnemyIndex;//_enemies �迭 �� ���� �ִ� ���� �ε���
    private float enemyPlayerDistance = 1.5f;//���缭 �����ϴ� ������ ���� ���� ����
    private bool isMove;//ĳ���Ͱ� �����̰� �ִ���. �����δ� ����� ����� �����̰� �ִ������ �� �� �ִ�.
    private float speed = 3f;//�����̴� �ӵ�
    private float _enemySpace = 1f;// enemies�� �迭 �� ĭ�� ������ �������� �Ǵ� x ����
    private int _enemyBundleNum = 10;//���� ��ġ�� �� �ִ� �迭�� ũ��. ������ ���� �� �� �Ҵ������ DetermineEnemyNum�� �����Ѵ�.
    private int _currentTargetIndex;//���� ���ֺ��� �ִ� ĳ����
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
                //Ÿ���� ���� ���
                TargetCase();
            }
            else
            {
                //Ÿ���� ���� ���
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
        //�Ҵ�� ���� �� óġ������ �ٽ� �Ҵ��Ѵ�.
        if (_enemies == null ||_currentTargetIndex>=_lastEnemyIndex)
        {
            _enemies = MakeEnemies();
            _currentTargetIndex = 0;
        }
        //���� Ÿ���� ã�´�.
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
    //Ǯ�� ���� ���ο� ������ �����ؼ� Ǯ�� �Ҵ��Ѵ�.
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
    // Ȱ��ȭ�� ���� ��ȯ
    public EnemyController[] MakeEnemies()
    {
        EnemyController[] result = new EnemyController[_enemyBundleNum];
        int currentNum = 0; // ���� ���� �󸶳� �������
        int enemyNum = DetermineEnemyNum();

        int index = 0; // ���� �迭�� �� �ε���

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
            // �ε����� ��ȯ������ �������� �迭 ���� ������ ����
            index = (index + 1) % _enemyBundleNum;
        }

        // lastEnemyIndex�� ����
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
