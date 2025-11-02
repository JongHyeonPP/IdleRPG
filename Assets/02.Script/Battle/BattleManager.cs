using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Services.RemoteConfig;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    private GameData _gameData;

    [Header("Enemy Pool")]
    [SerializeField] EnemyPool _ePool0, _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] Transform _spawnSpot, _poolParent;

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
    private bool _isBattleRunning = true;
    private BattleType _battleType;
    private BattleType _nextBattleType = BattleType.Default;

    private readonly float _defaultSpeed = 2.5f;
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
        _controller = (PlayerController)BattleBroker.GetPlayerController();

        _ePool0.poolParent = _ePool1.poolParent = _poolParent;

        SetEvent();
        _controller.MoveState(true);
        _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        BattleBroker.OnStageChange();
        UIBroker.RefreshStageSelectUI();

        //Awake에서 해당 이벤트들이 연결된 이후 Start에서 실행
        if (!string.IsNullOrEmpty(_gameData.playerWeaponId))
            PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[_gameData.playerWeaponId], WeaponType.Melee);
        if (_gameData.companionWeaponIdArr != null)
        {
            if (!string.IsNullOrEmpty(_gameData.companionWeaponIdArr[0]))
                PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[_gameData.companionWeaponIdArr[0]], WeaponType.Bow);
            if (!string.IsNullOrEmpty(_gameData.companionWeaponIdArr[1]))
                PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[_gameData.companionWeaponIdArr[1]], WeaponType.Shield);
            if (!string.IsNullOrEmpty(_gameData.companionWeaponIdArr[2]))
                PlayerBroker.OnEquipWeapon(WeaponManager.instance.weaponDict[_gameData.companionWeaponIdArr[2]], WeaponType.Staff);
        }
    }

    private void FixedUpdate()
    {
        if (_isBattleActive)
            MoveByPlayer();
    }

    private void Update()
    {
        if (_isBattleActive && _isBattleRunning)
            BattleLoop();
    }

    private void BattleLoop()
    {
        if (_controller.target != null)
            HandleTargetCase();
        else
            HandleNoTargetCase();
    }

    private void HandleTargetCase()
    {
        float dist = _controller.target.transform.position.x - _controller.transform.position.x;
        float range = (_battleType == BattleType.Default) ? _enemyPlayerDistance : _bossPlayerDistance;
        bool withinRange = dist < range;

        if (withinRange && _isMove)
        {
            _controller.MoveState(false);
            _controller.StartAttack();
            BattleBroker.ControllCompanionMove?.Invoke(2);

            if (_battleType is BattleType.Boss or BattleType.CompanionTech or BattleType.Adventure or BattleType.Dungeon or BattleType.Promote)

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
            _enemies = _battleType switch
            {
                BattleType.Default => MakeDefaultEnemies(),
                BattleType.Boss or BattleType.CompanionTech or BattleType.Adventure or BattleType.Dungeon or BattleType.Promote => MakeBoss(),
                _ => _enemies
            };
            _currentTargetIndex = 0;
        }

        while (_currentTargetIndex < _enemies.Length)
        {
            var target = _enemies[_currentTargetIndex];
            if (target != null && !target.isDead)
            {
                _controller.target = target;
                break;
            }
            _currentTargetIndex++;
        }
    }

    private void MoveByPlayer()
    {
        float playerSpeed = ((PlayerController)BattleBroker.GetPlayerController()).currentSpeed;
        foreach (var mover in MediatorManager<IMoveByPlayer>.GetRegisteredObjects())
            mover.MoveByPlayer(_isMove ? _defaultSpeed * playerSpeed * Time.fixedDeltaTime * Vector2.left : Vector3.zero);
    }

    private void SetEvent()
    {
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.SwitchToBattle += SwitchToBattle;
        BattleBroker.SwitchToBoss += SwitchToBoss;
        BattleBroker.SwitchToCompanionBattle += SwitchToCompanionBattle;
        BattleBroker.SwitchToAdventure += SwitchToAdventure;
        BattleBroker.SwitchToDungeon += SwitchToDungeon;
        BattleBroker.SwitchToPromoteBattle += SwitchToPromoteBattle;
        BattleBroker.OnEnemyDead += OnEnemyDead;
        PlayerBroker.OnPlayerDead +=  OnPlayerDead;

        BattleBroker.GetBattleType += () => _battleType;
        BattleBroker.IsCanAttack += () => !_isMove && _controller.target != null;

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

        
        BattleBroker.GetEnemyArray += () => _enemies;
    }

    private void OnPlayerDead()
    {
        _isBattleActive = false;
        switch (_battleType)
        {
            case BattleType.Dungeon:
                
                break;
        }
    }

    private void OnStageChange()
    {
        if (_gameData.currentStageNum > _gameData.maxStageNum)
        {
            _gameData.maxStageNum = _gameData.currentStageNum;
            UIBroker.RefreshStageSelectUI();
            if (_gameData.currentStageNum is 1 or 21)
            {
                _isBattleActive = false;
                BattleBroker.SwitchToStory?.Invoke(_gameData.currentStageNum == 1 ? 1 : 2);
                return;
            }
        }

        switch (_nextBattleType)
        {
            case BattleType.Adventure:
                var adventureInfo = _currentStageInfo.adventrueInfo;
                BattleBroker.SwitchToAdventure?.Invoke(adventureInfo.adventureIndex_0, adventureInfo.adventureIndex_1);
                break;
            case BattleType.CompanionTech:
                var techInfo = _currentStageInfo.companionTechInfo;
                BattleBroker.SwitchToCompanionBattle?.Invoke(techInfo.companionNum, (techInfo.techIndex_0, techInfo.techIndex_1));
                break;
            case BattleType.Dungeon:
                var dungeonInfo = _currentStageInfo.adventrueInfo;
                BattleBroker.SwitchToDungeon?.Invoke(dungeonInfo.adventureIndex_0, dungeonInfo.adventureIndex_1);
                break;
            case BattleType.Boss:
                BattleBroker.SwitchToBoss?.Invoke();
                break;
            default:
                BattleBroker.SwitchToBattle?.Invoke();
                break;
        }
        _nextBattleType = BattleType.None;
    }

    private void SwitchToBattle()
    {
        _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        _battleType = BattleType.Default;
        StartBattle(_currentStageInfo);
    }

    private void SwitchToBoss()
    {
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _battleType = BattleType.Boss;
        StartBattle(_currentStageInfo, true);
    }

    private void SwitchToCompanionBattle(int idx, (int, int) tech)
    {
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _currentStageInfo = StageInfoManager.instance.GetCompanionTechStageInfo(idx, tech);
        _battleType = BattleType.CompanionTech;
        StartBattle(_currentStageInfo, true);
    }

    private void SwitchToAdventure(int index_0, int index_1)
    {
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _currentStageInfo = StageInfoManager.instance.GetAdventureStageInfo(index_0)[index_1];
        _battleType = BattleType.Adventure;
        StartBattle(_currentStageInfo, true);
    }
    private void SwitchToDungeon(int index_0, int index_1)
    {
        NetworkBroker.QueueResourceReport(0, null, Resource.None, Source.Dungeon);
        _gameData.scroll -= StageInfoManager.instance.dungeonEntranceFee;
        PlayerBroker.OnScrollSet();
        NetworkBroker.SaveServerData();
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _currentStageInfo = StageInfoManager.instance.GetDungeonStageInfo(index_0)[index_1];
        _battleType = BattleType.Dungeon;
        StartBattle(_currentStageInfo, true);
    }
    private void SwitchToPromoteBattle(Rank rank)
    {
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _currentStageInfo = StageInfoManager.instance.GetPromoteStageInfo(rank);
        _battleType = BattleType.Promote;
        StartBattle(_currentStageInfo, true);
    }
    private void StartBattle(StageInfo info, bool isBoss = false)
    {
        ClearEnemies();
        if (isBoss) InitBossPools(info);
        else InitDefaultPools(info);

        ChangeBackground(info.background);
        _isBattleActive = true;
        _isBattleRunning = true;
    }

    private void ChangeBackground(Background bg)
    {
        foreach (var piece in _pieces)
            piece.ChangeBackground(bg);
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
        EnemyController[] result = new EnemyController[_enemyBundleNum];
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
        if (_ePool1.pool == null) return _ePool0.GetFromPool();
        return UtilityManager.CalculateProbability(0.5f) ? _ePool0.GetFromPool() : _ePool1.GetFromPool();
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

        switch (_battleType)
        {
            case BattleType.Boss:
                _gameData.currentStageNum++;
                _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
                _nextBattleType = BattleType.Default;
                DelayOnEnd();
                break;

            case BattleType.CompanionTech:
                var techInfo = _currentStageInfo.companionTechInfo;
                _gameData.companionPromoteTech[techInfo.companionNum][techInfo.techIndex_1] = techInfo.techIndex_0;
                var companionReward = BattleBroker.GetCompanionReward(techInfo.techIndex_0, techInfo.techIndex_1);
                _gameData.dia += companionReward.Item1;
                _gameData.clover += companionReward.Item2;
                PlayerBroker.OnDiaSet();
                PlayerBroker.OnCloverSet();
                NetworkBroker.QueueResourceReport(0, $"{techInfo.techIndex_0}_{techInfo.techIndex_1}", Resource.None, Source.Companion);

                _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
                _nextBattleType = BattleType.Default;
                DelayOnEnd();
                break;

            case BattleType.Adventure:
                var adventureInfo = _currentStageInfo.adventrueInfo;
                _gameData.adventureProgess[adventureInfo.adventureIndex_0]++;
                var reward = BattleBroker.GetAdventureReward(adventureInfo.adventureIndex_0, adventureInfo.adventureIndex_1);
                _gameData.dia += reward.Item1;
                _gameData.clover += reward.Item2;
                
                PlayerBroker.OnDiaSet();
                PlayerBroker.OnCloverSet();
                PlayerBroker.OnScrollSet();
                NetworkBroker.QueueResourceReport(0, $"{adventureInfo.adventureIndex_0}_{adventureInfo.adventureIndex_1}", Resource.Dia, Source.Adventure);
                var stageInfoArr = StageInfoManager.instance.GetAdventureStageInfo(adventureInfo.adventureIndex_0);
                if (BattleBroker.GetAdventureRetry() && stageInfoArr != null && stageInfoArr.Length - 1 > adventureInfo.adventureIndex_1)
                {
                    _currentStageInfo = stageInfoArr[adventureInfo.adventureIndex_1 + 1];
                }
                else
                {
                    _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
                    _nextBattleType = BattleType.Default;
                }

                DelayOnEnd();
                break;
            case BattleType.Dungeon:
                {
                    var dungeonInfo = _currentStageInfo.adventrueInfo;
                    int dungeonIndex_0 = dungeonInfo.adventureIndex_0;
                    int dungeonIndex_1 = dungeonInfo.adventureIndex_1;

                    var dungeonRewardObj = StageInfoManager.instance.GetDungeonReward(dungeonIndex_0, dungeonIndex_1);
                    if (dungeonRewardObj is DungeonReward dungeonReward)
                    {
                        switch (dungeonReward.resource)
                        {
                            case Resource.Gold:
                                _gameData.gold += dungeonReward.amount;
                                PlayerBroker.OnGoldSet();
                                NetworkBroker.QueueResourceReport(0, $"{dungeonIndex_0}_{dungeonIndex_1}", Resource.Gold, Source.Dungeon);
                                break;

                            case Resource.Clover:
                                _gameData.clover += dungeonReward.amount;
                                PlayerBroker.OnCloverSet();
                                NetworkBroker.QueueResourceReport(0, $"{dungeonIndex_0}_{dungeonIndex_1}", Resource.Clover, Source.Dungeon);
                                break;

                            case Resource.Fragment:
                                if (dungeonReward.rarity.HasValue)
                                {
                                    Rarity rarity = dungeonReward.rarity.Value;
                                    if (!_gameData.skillFragment.ContainsKey(rarity))
                                        _gameData.skillFragment[rarity] = 0;

                                    _gameData.skillFragment[rarity] += dungeonReward.amount;
                                    PlayerBroker.OnFragmentSet();
                                    NetworkBroker.QueueResourceReport(0, $"{dungeonIndex_0}_{dungeonIndex_1}", Resource.Fragment, Source.Dungeon);
                                }
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("DungeonReward is null or invalid.");
                    }

                    // 다음 전투 설정
                    //if (BattleBroker.GetDungeonRetry())
                    //{
                    //    _nextBattleType = BattleType.Dungeon;
                    //}
                    _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
                    _nextBattleType = BattleType.Default;

                    DelayOnEnd();
                    break;
                }
            case BattleType.Promote:
                _gameData.playerRankIndex = _currentStageInfo.stageNum + 1;
                _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
                _nextBattleType = BattleType.Default;
                PlayerBroker.UpdatePromoteLockState();
                DelayOnEnd();
                break;
        }

        NetworkBroker.SaveServerData();
    }

    private void DropItem(Vector3 pos)
    {
        DropBase drop;

        // 드롭 확률 원본 (Remote Config에서 가져오기)
        string probJson = RemoteConfigService.Instance.appConfig.GetJson("DROP_PROBABILITY", "None");
        var fullProbDict = JsonConvert.DeserializeObject<Dictionary<string, float>>(probJson);

        // 드롭 대상 및 가중치 리스트 구성
        List<(string type, float weight)> dropCandidates = new()
        {
            ("Gold", fullProbDict["Gold"]),
            ("Exp", fullProbDict["Exp"])
        };

        // Fragment 조건 확인
        if (CurrencyManager.instance.currentFragmentValue.count > 0)
            dropCandidates.Add(("Fragment", fullProbDict["Fragment"]));

        // Weapon 조건 확인
        if (!string.IsNullOrEmpty(CurrencyManager.instance.currentWeaponValue))
            dropCandidates.Add(("Weapon", fullProbDict["Weapon"]));

        // 전체 가중치 합계 계산
        float totalWeight = 0f;
        foreach (var candidate in dropCandidates)
            totalWeight += candidate.weight;

        // 정규화된 확률 리스트 생성
        List<float> normalizedWeights = new();
        foreach (var candidate in dropCandidates)
            normalizedWeights.Add(candidate.weight / totalWeight);

        // 확률 기반 인덱스 선택
        int selectedIndex = UtilityManager.AllocateProbability(normalizedWeights.ToArray());

        // 선택된 드롭 타입에 따라 드롭 생성
        string selectedType = dropCandidates[selectedIndex].type;
        switch (selectedType)
        {
            case "Gold":
                drop = _dPool.GetFromPool<GoldDrop>();
                drop.transform.position = pos + Vector3.up * 0.5f;
                break;
            case "Exp":
                drop = _dPool.GetFromPool<ExpDrop>();
                drop.transform.position = pos + Vector3.up * 0.2f;
                break;
            case "Fragment":
                drop = _dPool.GetFromPool<FragmentDrop>();
                drop.transform.position = pos + Vector3.up * 0.5f;
                break;
            case "Weapon":
                drop = _dPool.GetFromPool<WeaponDrop>();
                drop.transform.position = pos + Vector3.up * 0.5f;
                break;
            default:
                Debug.LogWarning("Invalid drop type selected.");
                return;
        }

        drop.StartDropMove();
    }


    private void SetPromoteTech(int compNum, int t0, int t1) { }

    private void DelayOnEnd()
    {
        BattleBroker.OnBossClear?.Invoke();
        _isBattleRunning = false;
        _isMove = true;
        _controller.MoveState(true);
        BattleBroker.ControllCompanionMove(1);
        StartCoroutine(StageEnterAfterDelay());
    }

    private IEnumerator StageEnterAfterDelay()
    {
        UIBroker.PopUpStageClear();
        yield return new WaitForSeconds(2f);
        UIBroker.FadeInOut(2f, 0.5f, 1f);
        yield return new WaitForSeconds(2f);
        BattleBroker.OnStageChange();
    }

    private void ClearEnemies()
    {
        if (_enemies == null) return;

        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;

            MediatorManager<IMoveByPlayer>.UnregisterMediator(enemy);
            enemy.StopAllCoroutines();
            if (enemy.enemyHpBar != null)
                enemy.enemyHpBar.pool.ReturnToPool(enemy.enemyHpBar);
            Destroy(enemy.gameObject);
        }

        _enemies = null;
    }

    [ContextMenu("RestartBattle")]
    public void RestartBattle() => SwitchToBattle();
}
