using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    
    public static BattleManager instance;

    [Header("Enemy Pool")]
    [SerializeField] EnemyStorage storage;//������ ������ �ϰ������� �����ϰ� �ִ� ���
    //�� ���� ���� ���� �������� ������Ʈ Ǯ�� �� ���� ����
    [SerializeField] EnemyPool _ePool0;
    [SerializeField] EnemyPool _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] Transform _spawnSpot;//Ǯ���� ������Ʈ�� Ȱ��ȭ�Ǹ鼭 ���� ��ġ ����
    [SerializeField] Transform _poolParent;//��Ȱ��ȭ �� Ǯ�� �� ������Ʈ�� �� ����
    [Header("Drop Pool")]
    [SerializeField] GameObject _goldPrefab;
    [SerializeField] GameObject _expPrefab;
    [Header("Etc")]
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
        _ePool0.poolParent = _ePool1.poolParent = _poolParent;
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
        BattleBroker.OnEnemyDead += OnEnemyDead;
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
                MoveByPlayer();
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
    public void MoveByPlayer()
    {
        List<IMoveByPlayer> ros = MediatorManager<IMoveByPlayer>.GetRegisteredObjects();
        foreach (IMoveByPlayer ro in ros)
        {
            if (ro.Transform.gameObject.activeSelf)
                ro.Transform.Translate(speed * Time.deltaTime * Vector2.left);
        }
    }
    //Ǯ�� ���� ���ο� ������ �����ؼ� Ǯ�� �Ҵ��Ѵ�.
    public void InitPools()
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
        if (_ePool1.pool == null)
        {
            // pool_1�� ���� �� pool_0������ ����
            return _ePool0.GetFromPool();
        }
        else
        {
            // �� Ǯ���� 50% Ȯ���� ����
            return UtilityManager.CalculateProbability(0.5f) ? _ePool0.GetFromPool() : _ePool1.GetFromPool();
        }
    }

    // ���� ��ġ�� �����ϴ� �޼���
    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(_spawnSpot);
        enemy.transform.localPosition = new Vector2(_enemySpace * index, 0);
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
    private void OnEnemyDead(Vector3 position)
    {
        DropBase dropBase;
        switch (UtilityManager.CalculateProbability(1f))
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
}
