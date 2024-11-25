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
    //MainStageNum�� �����ϰ� �ű⿡ �´� ����� ����� �����Ѵ�.
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
            default: // 70 �̻�
                background = Background.WinterForest; // �⺻�� �Ǵ� ���ϴ� ��
                break;
        }
        foreach (BackgroundPiece piece in _pieces)
        {
            piece.ChangeBackground(background);
        }
    }
}