using EnumCollection;
using System.Collections.Generic;
using System.Numerics;
using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    private GameData _gameData;

    [Header("Enemy Pool")]
    [SerializeField] EnemyPool _ePool0, _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] Transform _spawnSpot, _poolParent;
    [SerializeField] HashSet<GoldDrop> activeGold = new();
    [SerializeField] HashSet<ExpDrop> activeExp = new();

    [Header("Etc")]
    [SerializeField] CompanionController[] _companions;
    [SerializeField] List<BackgroundPiece> _pieces;
    [SerializeField] StageSelectUI _stageSelectUI;

    private PlayerController _controller;
    private StageInfo _currentStageInfo;
    private EnemyController[] _enemies;
    private int _lastEnemyIndex;
    private int _currentTargetIndex;
    private bool _isMove = true;
    private bool _isBattleActive = true;
    private BattleType battleType;

    private readonly float _speed = 2.5f;
    private readonly float _enemySpace = 1f;
    private readonly float _enemyPlayerDistance = 1.5f;
    private readonly float _bossPlayerDistance = 2f;
    private readonly int _enemyBundleNum = 10;

    private void Awake()
    {
        instance ??= this;
        _gameData = StartBroker.GetGameData();
    }

    private void Start()
    {
        _controller = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        _ePool0.poolParent = _ePool1.poolParent = _poolParent;

        SetEvent();
        _controller.MoveState(true);
        BattleBroker.OnStageChange();
        BattleBroker.RefreshStageSelectUI(_gameData.currentStageNum);
        InitWeaponSprites();
    }

    private void InitWeaponSprites()
    {
        SetWeaponSprite(_gameData.playerWeaponId, WeaponType.Melee);
        WeaponType[] types = { WeaponType.Bow, WeaponType.Shield, WeaponType.Staff };
        for (int i = 0; i < 3; i++)
            SetWeaponSprite(_gameData.companionWeaponIdArr[i], types[i]);
    }

    private void SetWeaponSprite(string id, WeaponType type)
    {
        PlayerBroker.OnEquipWeapon(id != null ? WeaponManager.instance.weaponDict[id] : null, type);
    }

    private void FixedUpdate()
    {
        if (_isBattleActive) MoveByPlayer();
    }

    private void Update()
    {
        if (_isBattleActive) BattleLoop();
    }

    private void BattleLoop()
    {
        if (battleType == BattleType.None) return;

        if (_controller.target)
            HandleTargetCase();
        else
            HandleNoTargetCase();
    }

    private void HandleTargetCase()
    {
        float dist = _controller.target.transform.position.x - _controller.transform.position.x;
        bool withinRange = dist < ((battleType == BattleType.Default) ? _enemyPlayerDistance : _bossPlayerDistance);

        if (withinRange && _isMove)
        {
            _controller.MoveState(false);
            _controller.StartAttack();
            BattleBroker.ControllCompanionMove?.Invoke(2);

            if (battleType is BattleType.Boss or BattleType.CompanionTech)
                _controller.target.StartAttack();

            _isMove = false;
        }
        else if (!withinRange)
        {
            _controller.MoveState(true);
            _isMove = true;
            BattleBroker.ControllCompanionMove?.Invoke(1);
        }
    }

    private void HandleNoTargetCase()
    {
        if (_enemies == null || _currentTargetIndex >= _lastEnemyIndex)
        {
            _enemies = battleType switch
            {
                BattleType.Default => MakeDefaultEnemies(),
                BattleType.Boss or BattleType.CompanionTech or BattleType.Adventure => MakeBoss(),
                _ => _enemies
            };
            _currentTargetIndex = 0;
        }

        while (_currentTargetIndex < _enemies.Length)
        {
            var target = _enemies[_currentTargetIndex];
            if (target && !target.isDead)
            {
                _controller.target = target;
                break;
            }
            _currentTargetIndex++;
        }
    }

    private void MoveByPlayer()
    {
        foreach (var mover in MediatorManager<IMoveByPlayer>.GetRegisteredObjects())
            mover.MoveByPlayer(_isMove ? _speed * Time.fixedDeltaTime * Vector2.left : Vector3.zero);
    }

    private void SetEvent()
    {
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.SwitchToBattle += SwitchToBattle;
        BattleBroker.SwitchToBoss += SwitchToBoss;
        BattleBroker.SwitchToCompanionBattle += SwitchToCompanionBattle;
        BattleBroker.SwitchToAdventure += SwitchToAdventure;
        BattleBroker.OnEnemyDead += OnEnemyDead;
        PlayerBroker.OnPlayerDead += () => _isBattleActive = false;

        BattleBroker.GetBattleType += () => battleType;
        BattleBroker.IsCanAttack += () => !_isMove && _controller.target;

        PlayerBroker.OnCompanionPromoteTechSet += SetPromoteTech;

        EnemyBroker.GetEnemyMaxHp = type => BigInteger.Parse(type switch
        {
            EnemyType.Enemy => _currentStageInfo.enemyStatusFromStage.maxHp,
            EnemyType.Boss => _currentStageInfo.bossStatusFromStage.maxHp,
            EnemyType.Chest => _currentStageInfo.chestStatusFromStage.maxHp,
            _ => "0"
        });

        EnemyBroker.GetEnemyResist = type => type switch
        {
            EnemyType.Enemy => _currentStageInfo.enemyStatusFromStage.resist,
            EnemyType.Boss => _currentStageInfo.bossStatusFromStage.resist,
            EnemyType.Chest => _currentStageInfo.chestStatusFromStage.resist,
            _ => 0f
        };

        EnemyBroker.GetEnemyPower = type => type switch
        {
            EnemyType.Boss => BigInteger.Parse(_currentStageInfo.bossStatusFromStage.power),
            _ => 0
        };

        EnemyBroker.GetEnemyPenetration = type => type switch
        {
            EnemyType.Boss => _currentStageInfo.bossStatusFromStage.penetration,
            _ => 0f
        };
    }

    private void OnStageChange()
    {
        if (_gameData.currentStageNum > _gameData.maxStageNum)
        {
            _gameData.maxStageNum = _gameData.currentStageNum;
            if (_gameData.currentStageNum is 1 or 21)
            {
                _isBattleActive = false;
                BattleBroker.SwitchToStory?.Invoke(_gameData.currentStageNum == 1 ? 1 : 2);
                return;
            }
        }

        BattleBroker.SwitchToBattle();
    }

    private void SwitchToBattle()
    {
        _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        battleType = BattleType.Default;
        StartBattle(_currentStageInfo);
    }

    private void SwitchToBoss()
    {
        battleType = BattleType.Boss;
        StartBattle(_currentStageInfo, true);
    }

    private void SwitchToCompanionBattle(int idx, (int, int) tech)
    {
        _currentStageInfo = StageInfoManager.instance.GetCompanionTechStageInfo(idx, tech);
        battleType = BattleType.CompanionTech;
        StartBattle(_currentStageInfo, true);
    }
    private void SwitchToAdventure(int index_0, int index_1)
    {
        _currentStageInfo = StageInfoManager.instance.GetAdventureStageInfo(index_0)[index_1];
        battleType = BattleType.Adventure;
        StartBattle(_currentStageInfo, true);
    }

    private void StartBattle(StageInfo info, bool isBoss = false)
    {
        ClearEnemies();
        if (isBoss) InitBossPools(info);
        else InitDefaultPools(info);

        ChangeBackground(info.background);
        _isBattleActive = true;
    }

    private void ChangeBackground(Background bg)
    {
        foreach (var piece in _pieces) piece.ChangeBackground(bg);
    }

    private void InitDefaultPools(StageInfo info)
    {
        _ePool0.ClearPool();
        _ePool1.ClearPool();
        if (info.enemy_0) _ePool0.InitializePool(info.enemy_0, info.enemyNum);
        if (info.enemy_1) _ePool1.InitializePool(info.enemy_1, info.enemyNum);
    }

    private void InitBossPools(StageInfo info)
    {
        _ePool0.ClearPool();
        _ePool0.InitializePool(info.boss, 1);
    }

    private EnemyController[] MakeDefaultEnemies()
    {
        var result = new EnemyController[_enemyBundleNum];
        int count = 0, index = 0;

        while (count < _currentStageInfo.enemyNum)
        {
            if ((index == 0 || UtilityManager.CalculateProbability(0.5f)) && result[index] == null)
            {
                var enemy = GetEnemyFromPool();
                if (enemy != null)
                {
                    result[index] = enemy;
                    PositionEnemy(enemy, index);
                    enemy.SetCurrentInfo(result, index);
                    count++;
                }
            }
            index = (index + 1) % _enemyBundleNum;
        }

        _lastEnemyIndex = Array.FindLastIndex(result, e => e != null);
        return result;
    }

    private EnemyController[] MakeBoss()
    {
        var enemy = _ePool0.GetFromPool();
        enemy.target = _controller;
        PositionEnemy(enemy, 2);
        enemy.SetCurrentInfo(new[] { null, null, enemy }, 2);
        return new[] { null, null, enemy };
    }

    private EnemyController GetEnemyFromPool()
    {
        return _ePool1.pool == null ? _ePool0.GetFromPool() :
               UtilityManager.CalculateProbability(0.5f) ? _ePool0.GetFromPool() : _ePool1.GetFromPool();
    }

    private void PositionEnemy(EnemyController enemy, int index)
    {
        enemy.transform.SetParent(_spawnSpot);
        enemy.transform.localPosition = new Vector2(_enemySpace * index, 0);
        enemy.SetHpBarPosition();
        enemy.enemyHpBar.SetDisplay(true);
    }

    private void OnEnemyDead(Vector3 pos)
    {
        DropItem(pos);
        switch (battleType)
        {
            case BattleType.Boss:
                _gameData.currentStageNum++;
                DelayOnEnd();
                break;
            case BattleType.CompanionTech:
                StageInfo.CompanionTechInfo techInfo = _currentStageInfo.companionTechInfo;
                _gameData.companionPromoteTech[techInfo.companionNum][techInfo.techIndex_1] = techInfo.techIndex_0;
                (int, int) companionReward = BattleBroker.GetCompanionReward(techInfo.techIndex_0, techInfo.techIndex_1);
                _gameData.dia += companionReward.Item1;
                _gameData.clover += companionReward.Item2;
                BattleBroker.OnDiaSet();
                BattleBroker.OnCloverSet();
                NetworkBroker.QueueResourceReport(companionReward.Item1, Resource.Dia, Source.Companion);
                NetworkBroker.QueueResourceReport(companionReward.Item2, Resource.Clover, Source.Companion);
                NetworkBroker.SaveServerData();
                DelayOnEnd();
                break;
            case BattleType.Adventure:
                StageInfo.AdventureInfo adventureInfo = _currentStageInfo.adventrueInfo;
                _gameData.adventureProgess[adventureInfo.adventureIndex_0]++;
                (int, int) adventureReward = BattleBroker.GetAdventureReward(adventureInfo.adventureIndex_0, adventureInfo.adventureIndex_1);
                _gameData.dia += adventureReward.Item1;
                _gameData.clover += adventureReward.Item2;
                _gameData.scroll -= StageInfoManager.instance.adventureEntranceFee;
                BattleBroker.OnDiaSet();
                BattleBroker.OnCloverSet();
                BattleBroker.OnScrollSet();
                NetworkBroker.QueueResourceReport(adventureInfo.adventureIndex_0, Resource.None, Source.Adventure);
                NetworkBroker.SaveServerData();
                DelayOnEnd();
                break;
        }
    }

    private void SetPromoteTech(int compNum, int t0, int t1)
    {
        
    }

    private void DelayOnEnd()
    {
        battleType = BattleType.None;
        _isMove = true;
        _controller.MoveState(true);
        BattleBroker.ControllCompanionMove(0);
        StartCoroutine(StageEnterAfterDelay());
    }

    private IEnumerator StageEnterAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        BattleBroker.OnStageChange();
        NetworkBroker.SaveServerData();
        BattleBroker.OnBossClear?.Invoke();
        BattleBroker.RefreshStageSelectUI(_gameData.currentStageNum);
    }

private void DropItem(Vector3 pos)
{
    DropBase drop;

    if (UtilityManager.CalculateProbability(0.5f))
    {
        var dropGold = _dPool.GetFromPool<GoldDrop>();
        activeGold.Add(dropGold);
        drop = dropGold;
        drop.transform.position = pos + Vector3.up * 0.5f;
    }
    else
    {
        var dropExp = _dPool.GetFromPool<ExpDrop>();
        activeExp.Add(dropExp);
        drop = dropExp;
        drop.transform.position = pos + Vector3.up * 0.2f;
    }

    drop.StartDropMove();
}


    private void ClearEnemies()
    {
        if (_enemies == null) return;
        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;
            MediatorManager<IMoveByPlayer>.UnregisterMediator(enemy);
            enemy.StopAllCoroutines();
            if (enemy.enemyHpBar) enemy.enemyHpBar.pool.ReturnToPool(enemy.enemyHpBar);
            Destroy(enemy.gameObject);
        }
        _enemies = null;
    }

    [ContextMenu("RestartBattle")]
    public void RestartBattle() => SwitchToBattle();
}
