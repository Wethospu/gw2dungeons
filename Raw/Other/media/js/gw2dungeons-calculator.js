;
"use strict";
var STAT_MAX = 926;

// Calculates base health at given level and profession.
function getHealth(level, profession) {
    return getBaseHealth(level, profession) + STAT_MAX * 10 * getStatScale(level);
}

// Calculates base health at given level and profession.
function getBaseHealth(level, profession) {
    // Base health gained per level.
    // Groups are: 0-19, 20-39, 40-59, 60-79, 80.
    var values;
    if (profession == "thief" || profession == "guardian" || profession == "elementalist")
        values = [5, 12.5, 25, 37.5, 50];
    else if (profession == "engineer" || profession == "ranger" || profession == "mesmer" || profession == "revenant")
        values = [18, 45, 90, 135, 180];
    else // Warrior & Necromancer.
        values = [28, 70, 140, 210, 280];
    // Needed for level ups beyond known values.
    var lastDifference = values[values.length - 1] - values[values.length - 2];

    // Downlevel player to -1 to allow using loop (= 20 level ups for level 19 like with other tiers).
    levelUps = level + 1;
    var health = -values[0];
    // Level up tier by tier (20 levels at time).
    for (var tier = 0; levelUps > 0; tier++) {
        // Check can we level full amount (20) or if we run out of level ups.
        var min = Math.min(levelUps, 20);
        // Normal level up (0-99).
        if (tier < values.length)
            health += values[tier] * min;
        // Future level up (100++).
        else {
            futureValue = values[values.length - 1] + (tier + 1 - values.length) * lastDifference;
            health += futureValue * min;
        }
        // Remove used level ups.
        levelUps -= min;
    }
    return health;
}

// Calculates base armor at given level and profession.
function getBaseArmor(level, profession) {
    var armor = STAT_MAX;
    if (profession == "elementalist" || profession == "mesmer" || profession == "necromancer")
        armor += 920;
    else if (profession == "ranger" || profession == "thief" || profession == "engineer")
        armor += 1064;
    else // Warrior,  Guardian and Revenant.
        armor += 1211;
    return Math.round(getArmorScale(level) * armor);
}

// Returns how stats scale.
// This is based on how many stats you get on level up.
/*
1: 24	0.02591792656
4: 54	0.05831533477
10: 89	0.09611231101
16: 124	0.13390928725
22: 164	0.17710583153
28: 219	0.23650107991
34: 279	0.30129589632
40: 349	0.37688984881
46: 422	0.45572354211
52: 502	0.54211663067
58: 582	0.62850971922
64: 664	0.71706263498
70: 746	0.80561555075
76: 831	0.89740820734
80: 926 1
*/
function getStatScale(targetLevel) {
	var stat = 0;
	if (targetLevel < 4)
		stat = 24;
	else if (targetLevel < 10)
		stat = 54;
	else if (targetLevel < 16)
		stat = 89
	else if (targetLevel < 22)
		stat = 124;
	else if (targetLevel < 28)
		stat = 164;
	else if (targetLevel < 34)
		stat = 219;
	else if (targetLevel < 40)
		stat = 279;
	else if (targetLevel < 46)
		stat = 349;
	else if (targetLevel < 52)
		stat = 422;
	else if (targetLevel < 58)
		stat = 502;
	else if (targetLevel < 64)
		stat = 582;
	else if (targetLevel < 70)
		stat = 664;
	else if (targetLevel < 76)
		stat = 746;
	else if (targetLevel < 80)
		stat = 831;
	else
		stat = STAT_MAX;
    return stat / STAT_MAX;
}

// Defense scales differently than toughness.
// No difference between armor classes.

// These cause weird behavior:
// Level 80 masterwork has less armor than level 35 masterwork
// Level 35 masterwork breatplate has 107/26 = 4.12 defense/stat ratio
// Level 80 masterwork breatplate has 298/82 = 3.63 defense/stat ratio
// Level 80 exotic breatplate has 363/101 = 3.59 defense/stat ratio
// TODO: Use ((2127*armorscale)-(916*statscale))/1211 to get right armor scale.
// TODO: Add extra setting for toughness to separate defense and toughness.

function getArmorScale(level) {
    if (level > 78)
        return 1;
    if (level > 76)
        return 0.960;
    if (level == 76)
        return 0.925;
    if (level > 70)
        return 0.906;
    if (level > 65)
        return 0.811;
    if (level > 60)
        return 0.727;
    if (level > 55)
        return 0.646;
    if (level > 50)
        return 0.539;
    if (level > 45)
        return 0.469;
    if (level > 40)
        return 0.408;
    if (level > 35)
        return 0.351;
    if (level > 30)
        return 0.299;
    else
        return 0.252
}

function getTooltipArmor(level) {
    if (level > 78)
        return 2597;
    if (level > 76)
        return 2432;
    if (level == 76)
        return 2275;
    if (level > 70)
        return 2224;
    if (level > 65)
        return 1923;
    if (level > 60)
        return 1563;
    if (level > 55)
        return 1446
    if (level > 50)
        return 1241;
    if (level > 45)
        return 1047;
    if (level > 40)
        return 882;
    if (level > 35)
        return 727;
    if (level > 30)
        return 617;
    else
        return 516
}


// Returns how agony scales.
function getAgonyScale(level, resist) {
    if (level > 49)
        var base = 0.84;
    else if (level > 39)
        var base = 0.66;
    else if (level > 29)
        var base = 0.48;
    else if (level > 19)
        var base = 0.3;
    else if (level > 9)
        var base = 0.12;
    else
        return 0;
    if (resist < 0)
        resist = 0;
    base -= resist * 0.012;
    if (base < 0.01)
        base = 0.01;
    return base;
}

var healthValues;
var dungeonLevels = [30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 76, 78, 80];
var agonyPerSecond;
var playerArmor = 2137;

// Updates all character values.
function updateValues(playerArmorNew, playerHealth, playerLevel, playerClass, playerAgony, playerFractal, wvwDefense, wvwHealth) {
	// WvW defense is currently slightly bugged. All values (except 1%) are actually 1%-unit less.
	if (wvwDefense > 1)
		wvwDefense = wvwDefense - 1;
	// Armor gets ceiled.
    playerArmor = Math.ceil(playerArmorNew * (1 + wvwDefense / 100));
    // Separate base health and vitality health.
    var playerBaseHealth = getBaseHealth(playerLevel, playerClass);
    var playerVitality = playerHealth - playerBaseHealth;
    healthValues = new Array();
    // Down/upscale for each dungeon.
    for (var i = 0; i < dungeonLevels.length; i++) {
        var dungeonLevel = dungeonLevels[i];
		// High levels players get +2 to their level at story dungeons.
		if (dungeonLevel == 30 || dungeonLevel == 40 || dungeonLevel == 50 || dungeonLevel == 60 || dungeonLevel == 70
			|| dungeonLevel == 76 || dungeonLevel == 78)
		{
			if (playerLevel + 1 == dungeonLevel)
				dungeonLevel += 1;
			else if (playerLevel > dungeonLevel)
				dungeonLevel += 2;
		}
		// High level players get +1 to their level at TA exp.
		if (dungeonLevel == 55)
		{
			if (playerLevel > dungeonLevel)
				dungeonLevel += 1;
		}
        // Calculate base health for player in the dungeon.
        var health = getBaseHealth(dungeonLevel, playerClass);
        // Add scaled vitality health.
        health = health + playerVitality / getStatScale(playerLevel) * getStatScale(dungeonLevel);
        // Add wvw bonus.
        health = (1 + wvwHealth / 100) * health;
        healthValues.push(Math.ceil(health));
    }
    agonyPerSecond = getAgonyScale(playerFractal, playerAgony);
}

function getPercentage(damage, dungeonLevel) {
    return Math.ceil(1000 * damage / healthValues[dungeonLevels.indexOf(dungeonLevel)]) / 10;
}

function calculatePercentDamage(damage, dungeonLevel) {
    return Math.ceil(damage * healthValues[dungeonLevels.indexOf(dungeonLevel)] / 10 ) / 10;
}

function getDamage(damage, potion, dungeonLevel) {
    // Not accurate because of weird armor scaling.
    damage = damage / Math.floor(getArmorScale(dungeonLevel) * playerArmor);
	// Damage gets floored.
    return Math.floor(damage * (1 - potion));
}

function getAgonyDamage(second) {
	// Agony probably gets floored.
    return Math.floor(healthValues[dungeonLevels.indexOf(80)] * agonyPerSecond * second);
}

////// These values must match GW2Helper.cs ///////
//// HEALTH SCALING /////

function fractalHealthScaling(baseHealth, scale, level, type) {
    if (type == 1)
        return baseHealth;
    else if (type == 2)
        return fractalNormalHealthCalculator(baseHealth, scale, level);
    else if (type == 3)
        return fractalChampionHealthCalculator(baseHealth, scale);
    else if (type == 4)
        return fractalLegendaryHealthCalculator(baseHealth, scale);
    else if (type == 5)
        return fractalLevelHealthCalculator(baseHealth, level);
}

// Nornal / veteran enemies get health every scale 0.0428557 of scale 0 value.
function fractalNormalHealthCalculator(health, scale, level) {
    health = Number(health);
    level = Number(level);
    scale = Number(scale);
    health = health * (1 + 0.0428557 * scale);
    health = Math.ceil(health);
    return fractalLevelHealthCalculator(health, level);
}

// Legendary enemies get health every scale 0.013333333 or 0.026666667 of scale 0 value.
// First is small and then comes pattern of two bigs + two smalls.
function fractalChampionHealthCalculator(health, scale) {
    health = Number(health);
    scale = Number(scale);
    // Apply first separately.
    scale = scale - 1;
    // Pattern of 4 scales. Cut tail and analyze it separately.
    tail = scale % 4;
    pattern = scale - tail;
    bigScaling = pattern / 2;
    smallScaling = pattern / 2 + 1;
    if (tail > 0)
        bigScaling = bigScaling + 1;
    if (tail > 1)
        bigScaling = bigScaling + 1;
    if (tail > 2)
        smallScaling = smallScaling + 1;
    health = health * (1 + 0.02666667 * bigScaling + 0.01333333 * smallScaling);
    return Math.ceil(health);
}

// Legendary enemies get health every scale 0.0125 or 0.0250 of scale 0 value.
// First is big and then comes pattern of two bigs + two smalls.
function fractalLegendaryHealthCalculator(health, scale) {
    health = Number(health);
    scale = Number(scale);
    // Apply first separately.
    scale = scale - 1;
    // Pattern of 4 scales. Cut tail and analyze it separately.
    tail = scale % 4;
    pattern = scale - tail;
    bigScaling = pattern / 2 + 1;
    smallScaling = pattern / 2;
    if (tail > 0)
        bigScaling = bigScaling + 1;
    if (tail > 1)
        bigScaling = bigScaling + 1;
    if (tail > 2)
        smallScaling = smallScaling + 1;
    health = health * (1 + 0.025 * bigScaling + 0.0125 * smallScaling);
    return Math.ceil(health);
}

function fractalLevelHealthCalculator(health, level) {
    health = Number(health);
    level = Number(level);
    if (level == 81)
        health = health * 1.0290;
    else if (level == 82)
        health = health * 1.05848;
    else if (level == 83)
        health = health * 1.08796;
    // Not very accurate.
    else if (level == 84)
        health = health * 1.1179;
    // Not actually tested.
    else if (level == 85)
        health = health * 1.148;
    return Math.ceil(health);
}


//// DAMAGE SCALING /////
function fractalDamageScaling(baseDamage, scale, level, type) {
    if (type == 1)
        return baseDamage;
    else if (type == 2)
        return fractalNormalDamageCalculator(baseDamage, scale, level);
    else if (type == 3)
        return fractalNormalDamageCalculator(baseDamage, scale, 80);
    else if (type == 4)
        return fractalNormalDamageCalculator(baseDamage, scale, 80);
    else if (type == 5)
        return fractalLevelDamageCalculator(baseDamage, level);
}

// Barely tested!
// Nornal / veteran enemies get damage every scale 0.03 of scale 0 value.
function fractalNormalDamageCalculator(damage, scale, level) {
    damage = Number(damage);
    level = Number(level);
    scale = Number(scale);
    damage = damage * (1 + 0.03 * scale);
    damage = Math.ceil(damage);
    return fractalLevelDamageCalculator(damage, level);
}

// Legendary enemies get damage every scale 0.0322 of scale 0 value.
// Probably same big/small thing as with health (couldn't find any good looking values).
function fractalBossDamageCalculator(damage, scale) {
    damage = Number(damage);
    level = Number(level);
    scale = Number(scale);
    damage = damage * (1 + 0.0322 * scale);
    return Math.ceil(damage);
}

// Not tested at all.
function fractalLevelDamageCalculator(damage, level) {
    damage = Number(damage);
    level = Number(level);
    // Enemy level scaling (tweak until you get perfect results!).
    if (level == 81)
        damage = damage * 1.03;
    else if (level == 82)
        damage = damage * 1.06;
    else if (level == 83)
        damage = damage * 1.09;
    else if (level == 84)
        damage = damage * 1.12;
    else if (level == 85)
        damage = damage * 1.15;
    return Math.ceil(damage);
}

//// ARMOR SCALING /////
function fractalLevelArmorCalculator(armor, level) {
    armor = Number(armor);
    level = Number(level);
	// Reverse calculate the toughness multiplier (dumb but expansion probably changes this stuff).
	var multiplier = (armor - 1223) / 1374;
    if (level == 81)
        armor = 1247 + 1413 * multiplier;
    else if (level == 82)
        armor = 1271 + 1453 * multiplier;
    else if (level == 83)
        armor = 1295 + 1493 * multiplier;
	else if (level == 84)
        armor = 1319 + 1534 * multiplier;
	else if (level == 85)
        armor = 1343 + 1575 * multiplier;
    return Math.round(armor);
}

//// LEVEL SCALING /////
function fractalEnemyLevels(scale) {
    if (scale == 1)
        return "80";
    if (scale < 20)
        return "80|81";
    if (scale == 20)
        return "81";
    if (scale < 40)
        return "81|82";
    if (scale == 40)
        return "82";
    if (scale < 60)
        return "82|83";
    if (scale == 60)
        return "83";
    if (scale < 80)
        return "83|84";
    if (scale == 80)
        return "84";
    if (scale < 100)
        return "84|85";
    if (scale == 100)
        return "85";
}