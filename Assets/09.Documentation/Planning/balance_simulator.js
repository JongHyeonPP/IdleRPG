// Remote Config 데이터 (실제 값)
const REMOTE_CONFIG = {
    REINFORCE_VALUE_GOLD: {
        MaxHp: { baseInc: 4, step: 100, stepInc: 1, startValue: 100 },
        Power: { baseInc: 1, step: 100, stepInc: 0.5, startValue: 10 },
        HpRecover: { baseInc: 0.2, step: 200, stepInc: 0.1, startValue: 1 },
        Critical: { baseInc: 0.001, step: 999, stepInc: 0, startValue: 0 },
        CriticalDamage: { baseInc: 0.01, step: 100, stepInc: 0.01, startValue: 1.2 },
        GoldAscend: { baseInc: 0.01, step: 999, stepInc: 0, startValue: 0 }
    },
    REINFORCE_VALUE_STATUS: {
        MaxHp: { baseInc: 5, step: 200, stepInc: 2, startValue: 0 },
        Power: { baseInc: 1, step: 100, stepInc: 1, startValue: 0 },
        HpRecover: { baseInc: 1, step: 300, stepInc: 1, startValue: 0 },
        Critical: { baseInc: 1, step: 999, stepInc: 0, startValue: 0 },
        CriticalDamage: { baseInc: 0.01, step: 100, stepInc: 0.01, startValue: 0 },
        GoldAscend: { baseInc: 0.01, step: 999, stepInc: 0, startValue: 0 }
    },
    REINFORCE_PRICE_GOLD: {
        MaxHp: { baseInc: 5, step: 100, stepInc: 3, startValue: 100 },
        Power: { baseInc: 10, step: 100, stepInc: 5, startValue: 150 },
        HpRecover: { baseInc: 3, step: 150, stepInc: 2, startValue: 80 },
        Critical: { baseInc: 8, step: 200, stepInc: 4, startValue: 200 },
        CriticalDamage: { baseInc: 12, step: 200, stepInc: 6, startValue: 300 }
    },
    LEVEL_UP_REQUIRE_EXP: "{level} * {level} * 10 + {level} * 50",
    GOLD_DROP_FORMULA: {
        Formula: "{stageNum} * 5 + ({stageNum} * {stageNum} * 0.1)",
        Range: 0.2,
        Bonus: [10, 20, 30, 50, 100, 150, 200, 250, 300],
        BonusValue: 0.5
    },
    EXP_DROP_FORMULA: {
        Formula: "{stageNum} * 3 + ({stageNum} * {stageNum} * 0.05)",
        Range: 0.2,
        Bonus: [5, 15, 25, 45, 75, 125, 175, 225, 275],
        BonusValue: 0.3
    },
    OFFLINE_REWARD_INFO: {
        RequireOfflineSecond: 300,
        MaxAccumulatedTime: 28800,
        Exp: "{second} * {maxStageNum} * 0.05",
        Gold: "{second} * {maxStageNum} * 0.1",
        Dia: "{second} * 0.01",
        Clover: "{second} * 0.005"
    },
    PROMOTE_MULTIPLIER: {
        Stone: { mult: 1, unlockStage: 1 },
        Bronze: { mult: 1.5, unlockStage: 30 },
        Iron: { mult: 3, unlockStage: 60 },
        Silver: { mult: 8, unlockStage: 120 },
        Gold: { mult: 15, unlockStage: 200 }
    },
    COMPANION_UNLOCK: {
        0: 20,  // 궁수
        1: 40,  // 전사
        2: 60   // 법사
    },
    // 적 HP 공식 (문서 기준)
    ENEMY_HP_FORMULA: "250 + {stageNum} * 150 + ({stageNum} / 40) * ({stageNum} / 40) * 500",
    BOSS_HP_MULTIPLIER: 8.3,
    RESIST_FORMULA: "Math.min(0.4, Math.max(0, ({stageNum} - 50) * 0.002))"
};

// ===== 계산 함수 =====

// 강화 값 계산 (ReinforceManager 로직)
function calcReinforceValue(rule, level) {
    let total = rule.startValue;
    let inc = rule.baseInc;
    for (let i = 1; i <= level; i++) {
        total += inc;
        if (i % rule.step === 0) {
            inc += rule.stepInc;
        }
    }
    return total;
}

// 공식 평가
function evaluateFormula(formula, vars) {
    let expr = formula;
    for (const [key, val] of Object.entries(vars)) {
        expr = expr.replace(new RegExp(`\\{${key}\\}`, 'g'), val);
    }
    return eval(expr);
}

// 골드 드랍 계산
function calcGoldDrop(stageNum) {
    const cfg = REMOTE_CONFIG.GOLD_DROP_FORMULA;
    let base = evaluateFormula(cfg.Formula, { stageNum });
    if (cfg.Bonus.includes(stageNum)) {
        base *= (1 + cfg.BonusValue);
    }
    return Math.floor(base);
}

// EXP 드랍 계산
function calcExpDrop(stageNum) {
    const cfg = REMOTE_CONFIG.EXP_DROP_FORMULA;
    let base = evaluateFormula(cfg.Formula, { stageNum });
    if (cfg.Bonus.includes(stageNum)) {
        base *= (1 + cfg.BonusValue);
    }
    return Math.floor(base);
}

// 레벨업 필요 EXP
function calcRequiredExp(level) {
    return evaluateFormula(REMOTE_CONFIG.LEVEL_UP_REQUIRE_EXP, { level });
}

// 적 HP 계산
function calcEnemyHp(stageNum) {
    return evaluateFormula(REMOTE_CONFIG.ENEMY_HP_FORMULA, { stageNum });
}

// 보스 HP 계산
function calcBossHp(stageNum) {
    return calcEnemyHp(stageNum) * REMOTE_CONFIG.BOSS_HP_MULTIPLIER;
}

// 저항 계산
function calcResist(stageNum) {
    return Math.min(0.4, Math.max(0, (stageNum - 50) * 0.002));
}

// 승급 배율
function getPromoteMultiplier(stageNum) {
    const ranks = ['Gold', 'Silver', 'Iron', 'Bronze', 'Stone'];
    for (const rank of ranks) {
        if (stageNum >= REMOTE_CONFIG.PROMOTE_MULTIPLIER[rank].unlockStage) {
            return REMOTE_CONFIG.PROMOTE_MULTIPLIER[rank].mult;
        }
    }
    return 1;
}

// 해금된 동료 수
function getUnlockedCompanions(stageNum) {
    let count = 0;
    for (const [idx, unlockStage] of Object.entries(REMOTE_CONFIG.COMPANION_UNLOCK)) {
        if (stageNum >= unlockStage) count++;
    }
    return count;
}

// ===== 시뮬레이션 =====

function simulateStage(stageNum, goldReinforceLevel, statusPoints) {
    // 플레이어 Power 계산
    const powerFromGold = calcReinforceValue(REMOTE_CONFIG.REINFORCE_VALUE_GOLD.Power, goldReinforceLevel);
    const powerFromStatus = calcReinforceValue(REMOTE_CONFIG.REINFORCE_VALUE_STATUS.Power, statusPoints);
    const promoteMult = getPromoteMultiplier(stageNum);
    const totalPower = (powerFromGold + powerFromStatus) * promoteMult;

    // 적 데이터
    const enemyHp = calcEnemyHp(stageNum);
    const resist = calcResist(stageNum);

    // DPS 계산 (가정: 스킬 배율 1.5, 공격속도 1.5/초)
    const skillMult = 1.5;
    const attacksPerSec = 1.5;
    const dps = totalPower * skillMult * (1 - resist) * attacksPerSec;

    // 킬타임
    const killTime = enemyHp / dps;

    return {
        stageNum,
        enemyHp: Math.floor(enemyHp),
        resist: (resist * 100).toFixed(1) + '%',
        promoteMult,
        powerFromGold: Math.floor(powerFromGold),
        powerFromStatus: Math.floor(powerFromStatus),
        totalPower: Math.floor(totalPower),
        dps: Math.floor(dps),
        killTime: killTime.toFixed(2) + '초',
        status: killTime <= 3 ? '✅ 양호' : killTime <= 5 ? '⚠️ 느림' : '❌ 정체'
    };
}

// 스테이지별 예상 강화 레벨 추정
function estimateReinforceLevel(stageNum) {
    // 누적 골드 계산
    let totalGold = 0;
    for (let s = 1; s <= stageNum; s++) {
        totalGold += calcGoldDrop(s) * 10; // 스테이지당 10킬 가정
    }

    // Power 강화에 50% 투자
    const goldForPower = totalGold * 0.5;

    // 강화 레벨 역산
    let level = 0;
    let spent = 0;
    while (true) {
        const cost = calcReinforceValue(REMOTE_CONFIG.REINFORCE_PRICE_GOLD.Power, level + 1);
        if (spent + cost > goldForPower) break;
        spent += cost;
        level++;
    }
    return level;
}

// 스테이지별 예상 스탯 포인트
function estimateStatusPoints(stageNum) {
    // 레벨 추정 (대략 스테이지와 비슷)
    return Math.floor(stageNum * 0.8);
}

// ===== 전체 시뮬레이션 =====
function runFullSimulation() {
    const stages = [1, 10, 20, 30, 40, 50, 60, 80, 100, 120, 150, 200, 250, 300];
    const results = [];

    for (const stage of stages) {
        const goldLv = estimateReinforceLevel(stage);
        const statusPts = estimateStatusPoints(stage);
        results.push(simulateStage(stage, goldLv, statusPts));
    }

    return results;
}

// 테스트 출력
console.table(runFullSimulation());
