using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    private GameData _gameData;
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
    private PlayerController _controller;//GameManager�κ��� ���� controller ����
    [SerializeField] CompanionController[] _companions;
    [SerializeField] List<BackgroundPiece> _pieces;//��ġ�� ���������� �����ϸ鼭 ���� ��� �̹�����
    private EnemyController[] _enemies = null;//���� ���ͼ� ���� ��� ���� ���� ����
    private int _lastEnemyIndex;//_enemies �迭 �� ���� �ִ� ���� �ε���
    private float _enemyPlayerDistance = 1.5f;//���缭 �����ϴ� ������ ���� ���� ����
    private float _bossPlayerDistance = 2.5f;//���缭 �����ϴ� ������ ������ ���� ����
    private bool _isMove;//ĳ���Ͱ� �����̰� �ִ���. �����δ� ����� ����� �����̰� �ִ������ �� �� �ִ�.
    private float _speed = 2.5f;//�����̴� �ӵ�
    private float _enemySpace = 1f;// enemies�� �迭 �� ĭ�� ������ �������� �Ǵ� x ����
    private int _enemyBundleNum = 10;//���� ��ġ�� �� �ִ� �迭�� ũ��. ������ ���� �� �� �Ҵ������ DetermineEnemyNum�� �����Ѵ�.
    private int _currentTargetIndex;//���� ���ֺ��� �ִ� ĳ������ enemies������ �ε���
    private bool isBattleActive = false; // ���� ���� Ȱ��ȭ ����
    private StageInfo currentStageInfo;//���� ���� ���� ���������� ���� ����... �� ����, ����, �������� �̸�...
    private BattleType battleType;//������ Ÿ��. Default, Boss, Die
    [SerializeField] Camera expandCamera;//���� ���� ī�޶�
    [SerializeField] Camera shrinkCamera;//���� ���� ī�޶�
    [HideInInspector] public Camera currentCamera;//���� ���� ī�޶�
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _gameData = StartBroker.GetGameData();
    }

    private void Start()
    {

        _controller = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _ePool0.poolParent = _ePool1.poolParent = _poolParent;
        GameManager.instance.AutoSaveStart();
        _isMove = true;
        _controller.MoveState(true);
        isBattleActive = true;
        SetEvent();
        ControlView(false);
        SetWeaponSprite(_gameData.playerWeaponId, WeaponType.Melee);
        for (int i = 0; i < 3; i++)
        {
            WeaponType weaponType;
            switch (i)
            {
                default:
                    weaponType = WeaponType.Bow;
                    break;
                case 1:
                    weaponType = WeaponType.Shield;
                    break;
                case 2:
                    weaponType = WeaponType.Staff;
                    break;
            }
            SetWeaponSprite(_gameData.companionWeaponIdArr[i], weaponType);
        }
    }
    private void SetWeaponSprite(string weaponId, WeaponType weaponType)
    {
        if (weaponId != null)
            PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[weaponId], weaponType);
        else
            PlayerBroker.OnEquipWeapon(null, weaponType);
    }
    public void SetEvent()
    {
        //Event ����
        BattleBroker.OnStageEnter += OnStageEnter;
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.OnBossEnter += OnBossEnter;
        BattleBroker.OnEnemyDead += OnEnemyDead;
        PlayerBroker.OnPlayerDead += OnPlayerDead;
        BattleBroker.GetBattleType += () => battleType;
        BattleBroker.IsCanAttack += IsCanAttack;
        UIBroker.OnMenuUIChange += OnMenuUIChange;
        BattleBroker.OnStageChange(_gameData.currentStageNum);
        BattleBroker.SwitchBattle += () => ControlBattle(true);
        BattleBroker.SwitchToStory += (stageId) => ControlBattle(false);
    }
    private void OnMenuUIChange(int index)
    {
        switch (index)
        {
            default:

                ControlView(true);
                break;
            case 0:
            case 2:
                ControlView(false);
                break;
        }
    }


    private bool IsCanAttack()
    {
        return !_isMove && _controller.target != null;
    }

    private void OnPlayerDead()
    {
        isBattleActive = false;
    }

    private void FixedUpdate()
    {
        if (!isBattleActive)
            return;
        MoveByPlayer();
    }
    private void Update()
    {

        if (!isBattleActive)
            return;
        BattleLoop();
    }

    private void BattleLoop()
    {
        if (battleType == BattleType.None)
        {
            return;
        }
        if (_controller.target)
        {
            // Ÿ���� ���� ���
            TargetCase();
        }
        else
        {
            // Ÿ���� ���� ���
            NoTargetCase();
        }

        // �� �����Ӹ��� MoveByPlayer ȣ��

    }
    private void TargetCase()
    {
        float xDistance = _controller.target.transform.position.x - _controller.transform.position.x;
        if (xDistance < ((battleType == BattleType.Default) ? _enemyPlayerDistance : _bossPlayerDistance))
        {
            if (_isMove)
            {
                _controller.MoveState(false);
                _controller.StartAttack();
                BattleBroker.StartCompanionAttack?.Invoke(_controller.target);
                if (battleType == BattleType.Boss)
                    _controller.target.StartAttack();
            }
            _isMove = false;
        }
        else
        {
            _controller.MoveState(true);
            _isMove = true;
            BattleBroker.StopCompanionAttack?.Invoke();
        }
    }

    private void NoTargetCase()
    {
        if (_enemies == null || _currentTargetIndex >= _lastEnemyIndex)
        {
            switch (battleType)
            {
                case BattleType.Default:
                    //
                    //Manager.instance.SetStageStatus();
                    _enemies = MakeDefaultEnemies();
                    break;
                case BattleType.Boss:
                    _enemies = MakeBoss();
                    break;
            }
            _currentTargetIndex = 0;
        }
        EnemyController target = null;
        while (true)
        {
            if (_enemies[_currentTargetIndex] != null && !_enemies[_currentTargetIndex].isDead)
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
            if (_isMove)
            {
                ro.MoveByCharacter(_speed * Time.fixedDeltaTime * Vector2.left);
            }
            else
            {
                ro.MoveByCharacter(Vector3.zero);
            }
        }
    }
    private void OnStageChange(int stageNum)
    {
        currentStageInfo = StageInfoManager.instance.GetStageInfo(stageNum);
        BattleBroker.OnStageEnter();
        if (stageNum > _gameData.maxStageNum)
        {
            //switch (stageNum)
            //{
            //    case 1:
            //        Debug.Log("���� ����");
            //        BattleBroker.SwitchToStory?.Invoke(1);
            //        break;
            //    case 21:
            //        Debug.Log("�� ��° ���丮");
            //        break;
            //}
            _gameData.maxStageNum = stageNum;
        }
        StartBroker.SaveLocal();
    }
    private void OnStageEnter()
    {
        battleType = BattleType.Default;
        ChangeBackground();
        ClearEntireBattle();
        InitDefaultPools();
        isBattleActive = true; // ���� ���� Ȱ��ȭ
    }

    private void OnBossEnter()
    {
        ClearEntireBattle();
        InitBossPools();
        battleType = BattleType.Boss;
        isBattleActive = true; // ���� ���� Ȱ��ȭ
    }

    private void ClearEntireBattle()
    {
        ClearActiveDrop();
        if (_enemies != null)
            ClearActiveEnemy();
        isBattleActive = false; // ���� ���� ��Ȱ��ȭ
    }

    private void ChangeBackground()
    {
        Background background = currentStageInfo.background;
        foreach (BackgroundPiece piece in _pieces)
        {
            piece.ChangeBackground(background);
        }
    }

    private void InitDefaultPools()
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        if (currentStageInfo.enemy_0)
            _ePool0.InitializePool(currentStageInfo.enemy_0, currentStageInfo.enemyNum);
        if (currentStageInfo.enemy_1)
            _ePool1.InitializePool(currentStageInfo.enemy_1, currentStageInfo.enemyNum);
    }

    private void InitBossPools()
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        _ePool0.InitializePool(currentStageInfo.boss, 1);
    }

    private EnemyController[] MakeDefaultEnemies()
    {
        EnemyController[] result = new EnemyController[_enemyBundleNum];
        int currentNum = 0;
        int enemyNum = currentStageInfo.enemyNum;
        int index = 0;

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
            index = (index + 1) % _enemyBundleNum;
        }

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
    private EnemyController[] MakeBoss()
    {
        EnemyController[] result = new EnemyController[3];
        EnemyController enemyFromPool = _ePool0.GetFromPool();
        enemyFromPool.target = _controller;
        result[^1] = enemyFromPool;
        PositionEnemy(enemyFromPool, result.Length-1);
        enemyFromPool.SetCurrentInfo(result, result.Length - 1);
        return result;
    }
    private EnemyController GetEnemyFromPool()
    {
        if (_ePool1.pool == null)
        {
            return _ePool0.GetFromPool();
        }
        else
        {
            return UtilityManager.CalculateProbability(0.5f) ? _ePool0.GetFromPool() : _ePool1.GetFromPool();
        }
    }
    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(_spawnSpot);
        enemy.transform.localPosition = new Vector2(_enemySpace * index, 0);
    }
    private void OnEnemyDead(Vector3 position)
    {
        DropItem(position);
        if (battleType == BattleType.Boss)
        {
            OnBossStageClear();
           BattleBroker.OnBossClear?.Invoke();
        }
    }
    private void OnBossStageClear()
    {
        battleType = BattleType.None;
        _gameData.currentStageNum++;
        _isMove = true;
        _controller.MoveState(true);
        StartCoroutine(StageEnterAfterWhile());
    }

    private IEnumerator StageEnterAfterWhile()
    {
        yield return new WaitForSeconds(1.5f);
        BattleBroker.OnStageChange(_gameData.currentStageNum);
    }

    private void DropItem(Vector3 position)
    {
        DropBase dropBase;
        if (UtilityManager.CalculateProbability(0.5f))
        {
            var dropGold = _dPool.GetFromPool<GoldDrop>();
            activeGold.Add(dropGold);
            dropBase = dropGold;
            dropBase.transform.position = position + Vector3.up * 0.5f;
            dropBase.StartDropMove();
        }
        else
        {
            var dropExp = _dPool.GetFromPool<ExpDrop>();
            activeExp.Add(dropExp);
            dropBase = dropExp;
            dropBase.transform.position = position + Vector3.up * 0.2f;
            dropBase.StartDropMove();
        }
    }
    private void ClearActiveDrop()
    {
        foreach (ExpDrop x in activeExp)
        {
            _dPool.ReturnToPool(x);
        }
        activeExp.Clear();

        foreach (GoldDrop x in activeGold)
        {
            _dPool.ReturnToPool(x);
        }
        activeGold.Clear();
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
    //private void OnDestroy()
    //{
    //    ClearEvent();
    //}
    //private void ClearEvent()
    //{
    //    // BattleBroker �̺�Ʈ ����
    //    BattleBroker.OnStageEnter -= OnStageEnter;
    //    BattleBroker.OnStageChange -= OnStageChange;
    //    BattleBroker.OnBossEnter -= OnBossEnter;
    //    BattleBroker.OnEnemyDead -= OnEnemyDead;
    //    BattleBroker.GetBattleType -= () => battleType;

    //    // PlayerBroker �̺�Ʈ ����
    //    PlayerBroker.OnPlayerDead -= OnPlayerDead;
    //}
    private void ControlView(bool isExpand)
    {
        if (isExpand)//���� ������ ������
        {
            expandCamera.gameObject.SetActive(true);
            shrinkCamera.gameObject.SetActive(false);
            currentCamera = expandCamera;
            UIBroker.SetBarPosition?.Invoke(expandCamera);
        }
        else//���� ������ ������
        {
            expandCamera.gameObject.SetActive(false);
            shrinkCamera.gameObject.SetActive(true);
            currentCamera = shrinkCamera;
            UIBroker.SetBarPosition?.Invoke(shrinkCamera);
        }
        
    }
    public void ControlBattle(bool isActive)
    {
        isBattleActive = isActive;
    }
    [ContextMenu("RestartBattle")]
    public void RestartBattle()
    {
        BattleBroker.OnStageChange(_gameData.currentStageNum);
    }
}