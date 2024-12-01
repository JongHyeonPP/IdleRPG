using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BattleManager : MonoBehaviour
{

    public static BattleManager instance;

    [Header("Enemy Pool")]
    //�� ���� ���� ���� �������� ������Ʈ Ǯ�� �� ���� ����
    [SerializeField] EnemyPool _ePool0;
    [SerializeField] EnemyPool _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] HashSet<GoldDrop> activeGold = new();
    [SerializeField] HashSet<ExpDrop> activeExp = new();
    [SerializeField] Transform _spawnSpot;//Ǯ���� ������Ʈ�� Ȱ��ȭ�Ǹ鼭 ���� ��ġ ����
    [SerializeField] Transform _poolParent;//��Ȱ��ȭ �� Ǯ�� �� ������Ʈ�� �� ����
    [Header("Etc")]
    PlayerContoller _controller;//GameManager�κ��� ���� controller ����
    [SerializeField] List<BackgroundPiece> _pieces;//��ġ�� ���������� �����ϸ鼭 ���� ��� �̹�����
    private EnemyController[] _enemies = null;//���� ���ͼ� ���� ��� ���� ���� ����
    private int _lastEnemyIndex;//_enemies �迭 �� ���� �ִ� ���� �ε���
    private float enemyPlayerDistance = 1f;//���缭 �����ϴ� ������ ���� ���� ����
    private bool isMove;//ĳ���Ͱ� �����̰� �ִ���. �����δ� ����� ����� �����̰� �ִ������ �� �� �ִ�.
    private float speed = 3f;//�����̴� �ӵ�
    private float _enemySpace = 1f;// enemies�� �迭 �� ĭ�� ������ �������� �Ǵ� x ����
    private int _enemyBundleNum = 10;//���� ��ġ�� �� �ִ� �迭�� ũ��. ������ ���� �� �� �Ҵ������ DetermineEnemyNum�� �����Ѵ�.
    private int _currentTargetIndex;//���� ���ֺ��� �ִ� ĳ����

    private GameData _gameData;

    private StageInfo currentStageInfo;
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
        GameManager.instance.AutoSaveStart();
        _gameData = GameManager.instance.gameData;
        BattleBroker.OnMainStageChange += OnChangeMainStage;
        currentStageInfo = StageManager.instance.GetStageInfo(GameManager.instance.gameData.currentStageNum);
        StartBattle();
        
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
        if (_enemies == null || _currentTargetIndex >= _lastEnemyIndex)
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
    private void InitPools()
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        if (currentStageInfo.enemy_0)
            _ePool0.InitializePool(currentStageInfo.enemy_0);
        if (currentStageInfo.enemy_1)
            _ePool0.InitializePool(currentStageInfo.enemy_1);
    }
    private void ClearActiveEnemy()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy)
            {
                MediatorManager<IMoveByPlayer>.UnregisterMediator(enemy);
                Destroy(enemy.gameObject);
            }
        }
        _enemies = null;
    }
    // Ȱ��ȭ�� ���� ��ȯ
    public EnemyController[] MakeEnemies()
    {
        EnemyController[] result = new EnemyController[_enemyBundleNum];
        int currentNum = 0; // ���� ���� �󸶳� �������
        int enemyNum = currentStageInfo.enemyNum;
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


    private void OnEnemyDead(Vector3 position)
    {
        DropBase dropBase;
        switch (UtilityManager.CalculateProbability(0.5f))
        {
            case true:
                var dropGold = _dPool.GetFromPool<GoldDrop>();
                activeGold.Add(dropGold);
                dropBase = dropGold;
                break;
            case false:
                var dropExp = _dPool.GetFromPool<ExpDrop>();
                activeExp.Add(dropExp);
                dropBase = dropExp;
                break;
        }
        dropBase.transform.position = position + Vector3.up * 0.2f;
        dropBase.AddForceDiagonally();
    }
    //MainStageNum�� �����ϰ� �ű⿡ �´� ����� ����� �����Ѵ�.
    private void OnChangeMainStage(int stageNum)
    {
        currentStageInfo = StageManager.instance.GetStageInfo(stageNum);
        ClearActiveDrop();
        ChangeBackground();
        ClearActiveEnemy();
        InitPools();
    }

    private void ClearActiveDrop()
    {
        //Exp
        foreach (ExpDrop x in activeExp)
        {
            _dPool.ReturnToPool(x);
        }
        activeExp.Clear();
        //Gold
        foreach (GoldDrop x in activeGold)
        {
            _dPool.ReturnToPool(x);
        }
        activeGold.Clear();
    }

    private void ChangeBackground()
    {
        Background background = currentStageInfo.background;
        foreach (BackgroundPiece piece in _pieces)
        {
            piece.ChangeBackground(background);
        }
    }
}