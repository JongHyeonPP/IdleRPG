using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigInteger = System.Numerics.BigInteger;
using Random = UnityEngine.Random;
public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    private GameData _gameData;

    [Header("Enemy Pool")]
    //두 가지 적이 섞여 나오도록 오브젝트 풀을 두 가지 생성
    [SerializeField] EnemyPool _ePool0;
    [SerializeField] EnemyPool _ePool1;
    [SerializeField] DropPool _dPool;
    [SerializeField] HashSet<GoldDrop> activeGold = new();
    [SerializeField] HashSet<ExpDrop> activeExp = new();
    [SerializeField] Transform _spawnSpot;//풀링된 오브젝트가 활성화되면서 나올 위치 정보
    [SerializeField] Transform _poolParent;//비활성화 돼 풀에 들어간 오브젝트가 들어갈 공간

    [Header("Etc")]
    private StageInfo _currentStageInfo;//현재 진행 중인 스테이지의 전투 정보... 적 종류, 개수, 스테이지 이름...
    private PlayerController _controller;//GameManager로부터 얻어온 controller 정보
    [SerializeField] CompanionController[] _companions;
    [SerializeField] List<BackgroundPiece> _pieces;//위치를 지속적으로 변경하면서 보일 배경 이미지들
    private EnemyController[] _enemies = null;//현재 나와서 전투 대기 중인 적들 정보
    private int _lastEnemyIndex;//_enemies 배열 중 끝에 있는 적의 인덱스
    private float _enemyPlayerDistance = 1.5f;//멈춰서 공격하는 시점의 적과 나의 간격
    private float _bossPlayerDistance = 2f;//멈춰서 공격하는 시점의 보스와 나의 간격
    private bool _isMove;//캐릭터가 움직이고 있는지. 실제로는 적들과 배경이 움직이고 있는지라고 할 수 있다.
    private float _speed = 2.5f;//움직이는 속도
    private float _enemySpace = 1f;// enemies의 배열 한 칸당 실제로 떨어지게 되는 x 간격
    private int _enemyBundleNum = 10;//적이 위치할 수 있는 배열의 크기. 실제로 적이 몇 명 할당될지는 DetermineEnemyNum가 정의한다.
    private int _currentTargetIndex;//현재 마주보고 있는 캐릭터의 enemies에서의 인덱스
    private bool _isBattleActive = false; // 전투 루프 활성화 여부

    private BattleType battleType;//전투의 타입. Default, Boss, Die
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
        BattleBroker.OnStageChange(_gameData.currentStageNum);
        BattleBroker.RefreshStageSelectUI(_gameData.currentStageNum);//CurrentStageUI갱신
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

        PlayerBroker.OnCompanionPromoteTechSet += OnCompanionPromoteTechSet;
        BattleBroker.GetNeedExp = GetNeedExp;
        //Drop
        BattleBroker.OnExpByDrop += GetExpByDrop;
        BattleBroker.OnGoldByDrop += GetGoldByDrop;
        BattleBroker.GetStageRewardValue = GetStageReward;
        //EnemyStatus
        EnemyBroker.GetEnemyMaxHp = (enemyType) =>
        {
            switch (enemyType)
            {
                case EnemyType.Enemy:
                    return BigInteger.Parse(_currentStageInfo.enemyStatusFromStage.maxHp);
                case EnemyType.Boss:
                    return BigInteger.Parse(_currentStageInfo.bossStatusFromStage.maxHp);
                case EnemyType.Chest:
                    return BigInteger.Parse(_currentStageInfo.chestStatusFromStage.maxHp);
            }
            return 0;
        };

        EnemyBroker.GetEnemyResist = (enemyType) => 
        {
            switch (enemyType)
            {
                case EnemyType.Enemy:
                    return _currentStageInfo.enemyStatusFromStage.resist;
                case EnemyType.Boss:
                    return _currentStageInfo.bossStatusFromStage.resist;
                case EnemyType.Chest:
                    return _currentStageInfo.chestStatusFromStage.resist;
            }
            return 0f;
        };

        EnemyBroker.GetEnemyPower = (enemyType) =>
        {
            switch (enemyType)
            {
                case EnemyType.Enemy:
                    return 0;
                case EnemyType.Boss:
                    return BigInteger.Parse(_currentStageInfo.bossStatusFromStage.power);
                case EnemyType.Chest:
                    return 0;
            }
            return 0;
        };

        EnemyBroker.GetEnemyPenetration = (enemyType) =>
        {
            switch (enemyType)
            {
                case EnemyType.Enemy:
                    return 0f;
                case EnemyType.Boss:
                    return _currentStageInfo.bossStatusFromStage.penetration;
                case EnemyType.Chest:
                    return 0f;
            }
            return 0f;
        };
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
            // 타겟이 있을 경우
            TargetCase();
        }
        else
        {
            // 타겟이 없을 경우
            NoTargetCase();
        }

        // 매 프레임마다 MoveByPlayer 호출

    }
    private void TargetCase()
    {
        float xDistance = _controller.target.transform.position.x - _controller.transform.position.x;
        if (xDistance < ((battleType == BattleType.Default) ? _enemyPlayerDistance : _bossPlayerDistance))//멈추고 공격 시작
        {
            if (_isMove)
            {
                _controller.MoveState(false);
                _controller.StartAttack();
                BattleBroker.ControllCompanionMove?.Invoke(2);
                if (battleType == BattleType.Boss || battleType == BattleType.CompanionTech)
                    _controller.target.StartAttack();
            }
            _isMove = false;
        }
        else//움직인다.
        {
            _controller.MoveState(true);
            _isMove = true;
            BattleBroker.ControllCompanionMove.Invoke(1);
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
        List<IMoveByPlayer> rObjs = MediatorManager<IMoveByPlayer>.GetRegisteredObjects();

        foreach (IMoveByPlayer rObj in rObjs)
        {
            if (_isMove)
            {
                rObj.MoveByPlayer(_speed * Time.fixedDeltaTime * Vector2.left);
            }
            else
            {
                rObj.MoveByPlayer(Vector3.zero);
            }
        }
    }
    private void OnStageChange(int stageNum)
    {
        _gameData.currentStageNum = stageNum;
        if (stageNum > _gameData.maxStageNum)
        {
            _gameData.maxStageNum = stageNum;
            if (stageNum == 1 || stageNum == 21)
            {
                _isBattleActive = false;
                //전투 멈춤
                switch (stageNum)
                {
                    case 1:
                        Debug.Log("최초 접속");

                        BattleBroker.SwitchToStory?.Invoke(1);
                        break;
                    case 21:
                        Debug.Log("두 번째 스토리");
                        break;
                }
            }
            else
            {
                BattleBroker.SwitchToBattle();
            }
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
        enemy.SetHpBarPosition();
        enemy.enemyHpBar.SetDisplay(true);
    }
    private void OnEnemyDead(Vector3 position)
    {
        DropItem(position);
        if (battleType == BattleType.Boss)
        {
            _gameData.currentStageNum++;
            OnBossCompanionStageClear();
        }
        else if (battleType == BattleType.CompanionTech)
        {
            int companionIndex = _currentStageInfo.companionTechInfo.companionNum;
            int techIndex_0 = _currentStageInfo.companionTechInfo.techIndex_0;
            int techIndex_1 = _currentStageInfo.companionTechInfo.techIndex_1;
            PlayerBroker.OnCompanionPromoteTechSet(companionIndex, techIndex_0, techIndex_1);
            CompanionTechData techData = CompanionManager.instance.GetCompanionTechData(companionIndex, techIndex_0, techIndex_1);
            _gameData.dia += techData.dia;
            _gameData.clover += techData.clover;
            BattleBroker.OnDiaSet();
            BattleBroker.OnCloverSet();
            OnBossCompanionStageClear();
        }
    }
    private void OnCompanionPromoteTechSet(int companionNum, int techIndex_0, int techIndex_1)
    {
        _gameData.companionPromoteTech[companionNum][techIndex_1] = techIndex_0;
    }
    private void OnBossCompanionStageClear()
    {
        battleType = BattleType.None;
        _isMove = true;
        BattleBroker.ControllCompanionMove(0);
        _controller.MoveState(true);
        StartCoroutine(StageEnterAfterWhile());
    }

    private IEnumerator StageEnterAfterWhile()
    {
        yield return new WaitForSeconds(1.5f);
        BattleBroker.OnStageChange(_gameData.currentStageNum);
        BattleBroker.OnBossClear?.Invoke();
        BattleBroker.RefreshStageSelectUI(_gameData.currentStageNum);
    }
    private void DropItem(Vector3 position)
    {
        //int dropNum = _currentStageInfo.enemyStatusFromStage.
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
    //private void ClearActiveDrop()
    //{
    //    foreach (ExpDrop x in activeExp)
    //    {
    //        _dPool.ReturnToPool(x);
    //    }
    //    activeExp.Clear();

    //    foreach (GoldDrop x in activeGold)
    //    {
    //        _dPool.ReturnToPool(x);
    //    }
    //    activeGold.Clear();
    //}

    private void ClearActiveEnemy()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy)
            {
                MediatorManager<IMoveByPlayer>.UnregisterMediator(enemy);
                enemy.StopAllCoroutines();
                if (enemy.enemyHpBar)
                    enemy.enemyHpBar.pool.ReturnToPool(enemy.enemyHpBar);
                Destroy(enemy.gameObject);
            }
        }
        _enemies = null;
    }
    public void ControlBattle(StageInfo stageInfo, bool _isBossStage = false)
    {
        if (_enemies != null)
            ClearActiveEnemy();
        if (_isBossStage)
        {

            InitBossPools(stageInfo);
        }
        else
        {
            InitDefaultPools(stageInfo);
        }
        ChangeBackground();
        _isBattleActive = true; // 전투 루프 활성화
    }
    public void GetGoldByDrop(int value)
    {
        _gameData.gold += value;
        BattleBroker.OnGoldSet();
    }
    public void GetExpByDrop(int value)
    {
        
        _gameData.exp += value;

        while (true)
        {
            BigInteger needExp = BattleBroker.GetNeedExp();
            if (_gameData.exp < needExp)
                break;
            if (_gameData.exp >= needExp)
            {
                _gameData.exp -= needExp;
                _gameData.level++;
                _gameData.statPoint++;
                BattleBroker.OnStatPointSet();
            }
        }
        BattleBroker.OnLevelExpSet();
    }
    private BigInteger GetNeedExp()
    {
        return _gameData.level * 100;
    }
    private int GetStageReward(DropType dropType)
    {
        int baseValue = 0;

        if (battleType == BattleType.Default)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    baseValue = _currentStageInfo.enemyStatusFromStage.gold;
                    break;
                case DropType.Exp:
                    baseValue = _currentStageInfo.enemyStatusFromStage.exp;
                    break;
            }
        }
        else if (battleType == BattleType.Boss)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    baseValue = _currentStageInfo.bossStatusFromStage.gold;
                    break;
                case DropType.Exp:
                    baseValue = _currentStageInfo.bossStatusFromStage.exp;
                    break;
            }
        }

        // ±10% 범위의 랜덤 int 생성
        int min = Mathf.FloorToInt(baseValue * 0.9f);
        int max = Mathf.CeilToInt(baseValue * 1.1f) + 1; // Random.Range의 max는 exclusive
        int result = Random.Range(min, max);
        return result;
    }

    [ContextMenu("RestartBattle")]
    public void RestartBattle()
    {
        SwitchToBattle();
    }
}