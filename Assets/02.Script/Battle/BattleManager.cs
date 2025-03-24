using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigInteger = System.Numerics.BigInteger;
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
    private StageInfo _currentStageInfo;//���� ���� ���� ���������� ���� ����... �� ����, ����, �������� �̸�...
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
    private bool _isBattleActive = false; // ���� ���� Ȱ��ȭ ����

    private BattleType battleType;//������ Ÿ��. Default, Boss, Die
    [SerializeField] Camera expandCamera;//���� ���� ī�޶�
    [SerializeField] Camera shrinkCamera;//���� ���� ī�޶�
    [HideInInspector] public Camera currentCamera;//���� ���� ī�޶�
    [SerializeField] StageSelectUI _stageSelectUI;
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
        _isBattleActive = true;
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
            PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[weaponId],weaponType);
        else
            PlayerBroker.OnEquipWeapon(null, weaponType);
    }
    public void SetEvent()
    {

        //Battle
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.SwitchToBattle += SwitchToBattle;
        BattleBroker.SwitchToBoss += SwitchToBoss;
        BattleBroker.SwitchToCompanionBattle += SwitchToCompanionBattle;
        BattleBroker.OnEnemyDead += OnEnemyDead;
        PlayerBroker.OnPlayerDead += OnPlayerDead;
        BattleBroker.GetBattleType += () => battleType;
        BattleBroker.IsCanAttack += IsCanAttack;
        UIBroker.OnMenuUIChange += OnMenuUIChange;  
        BattleBroker.OnStageChange(_gameData.currentStageNum);
        PlayerBroker.OnCompanionPromoteTechSet += OnCompanionPromoteTechSet;
        //EnemyStatus
        EnemyBroker.GetEnemyMaxHp += () =>
        {
            return BigInteger.TryParse(_currentStageInfo.enemyStatusFromStage.maxHp, out var maxHp)
                ? maxHp
                : BigInteger.Zero;
        };

        EnemyBroker.GetEnemyResist += () => _currentStageInfo.enemyStatusFromStage.resist;

        EnemyBroker.GetBossMaxHp += () =>
        {
            return BigInteger.TryParse(_currentStageInfo.bossStatusFromStage.maxHp, out var maxHp)
                ? maxHp
                : BigInteger.Zero;
        };

        EnemyBroker.GetBossResist += () => _currentStageInfo.bossStatusFromStage.resist;

        EnemyBroker.GetBossPower += () =>
        {
            return BigInteger.TryParse(_currentStageInfo.bossStatusFromStage.power, out var power)
                ? power
                : BigInteger.Zero;
        };

        EnemyBroker.GetBossPenetration += () => _currentStageInfo.bossStatusFromStage.penetration;

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
        _isBattleActive = false;
    }

    private void FixedUpdate()
    {
        if (!_isBattleActive)
            return;
        MoveByPlayer();
    }
    private void Update()
    {

        if (!_isBattleActive)
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
                if (battleType == BattleType.Boss || battleType == BattleType.CompanionTech)
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
                case BattleType.CompanionTech:
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
        _gameData.currentStageNum = stageNum;
        
        if (stageNum > _gameData.maxStageNum)
        {
            //���� ����
            _isBattleActive = false;
            switch (stageNum)
            {
                case 1:
                    Debug.Log("���� ����");
                    
                    BattleBroker.SwitchToStory?.Invoke(1);
                    break;
                case 21:
                    Debug.Log("�� ��° ���丮");
                    break;
            }
            _gameData.maxStageNum = stageNum;
        }
        else
        {
            
            BattleBroker.SwitchToBattle();
        }
        StartBroker.SaveLocal();
    }

    private void SwitchToBoss()
    {
        battleType = BattleType.Boss;
        ControlBattle(_currentStageInfo, true);
    }
    private void SwitchToBattle()
    {
        _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        battleType = BattleType.Default;
        ControlBattle(_currentStageInfo);
    }
    private void SwitchToCompanionBattle(int companionIndex, (int, int) tech)
    {
        _currentStageInfo =  StageInfoManager.instance.GetCompanionTechStageInfo(companionIndex, tech);
        battleType = BattleType.CompanionTech;
        ControlBattle(_currentStageInfo, true);
    }
    private void ClearEntireBattle()
    {
        ClearActiveDrop();
        if (_enemies != null)
            ClearActiveEnemy();
    }

    private void ChangeBackground()
    {
        Background background = _currentStageInfo.background;
        foreach (BackgroundPiece piece in _pieces)
        {
            piece.ChangeBackground(background);
        }
    }

    private void InitDefaultPools(StageInfo stageInfo)
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        if (stageInfo.enemy_0)
            _ePool0.InitializePool(stageInfo.enemy_0, stageInfo.enemyNum);
        if (stageInfo.enemy_1)
            _ePool1.InitializePool(stageInfo.enemy_1, stageInfo.enemyNum);
    }

    private void InitBossPools(StageInfo stageInfo)
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        _ePool0.InitializePool(stageInfo.boss, 1);
    }

    private EnemyController[] MakeDefaultEnemies()
    {
        EnemyController[] result = new EnemyController[_enemyBundleNum];
        int currentNum = 0;
        int enemyNum = _currentStageInfo.enemyNum;
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
            
            _gameData.currentStageNum++;
            
            OnBossStageClear();
        }
        else if (battleType == BattleType.CompanionTech)
        {
            int companionNum = _currentStageInfo.companionTechIndex.companionNum;
            int techIndex_0 = _currentStageInfo.companionTechIndex.techIndex_0;
            int techIndex_1 = _currentStageInfo.companionTechIndex.techIndex_1;
            PlayerBroker.OnCompanionPromoteTechSet(companionNum, techIndex_0, techIndex_1);
            StartBroker.SaveLocal();
            OnBossStageClear();
        }
    }
    private void OnCompanionPromoteTechSet(int companionNum, int techIndex_0, int techIndex_1)
    {
        _gameData.companionPromoteTech[companionNum][techIndex_1] = techIndex_0;
    }
    private void OnBossStageClear()
    {
        BattleBroker.OnBossClear?.Invoke();
        battleType = BattleType.None;
        _isMove = true;
        BattleBroker.StopCompanionAttack();
        _controller.MoveState(true);
        _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        StartCoroutine(StageEnterAfterWhile());
        BattleBroker.OnBossClear?.Invoke();
    }

    private IEnumerator StageEnterAfterWhile()
    {
        yield return new WaitForSeconds(1.5f);
        BattleBroker.SwitchToBattle();
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
    public void ControlBattle(StageInfo stageInfo, bool _isBossStage = false)
    {
            ClearEntireBattle();
            if (_isBossStage)
            {
                
                InitBossPools(stageInfo);
            }
            else
            {
                InitDefaultPools(stageInfo);
            }
            ChangeBackground();
            _isBattleActive = true; // ���� ���� Ȱ��ȭ
    }
    [ContextMenu("RestartBattle")]
    public void RestartBattle()
    {
        BattleBroker.OnStageChange(_gameData.currentStageNum);
    }
}