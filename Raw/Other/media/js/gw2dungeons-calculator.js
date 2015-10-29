﻿;
"use strict";
// Returns val1 / val2 as a percent value.
function toPercent(val1, val2) {
	return Math.ceil(1000 * val1 / val2) / 10;
}

//// PLAYER ATTRIBUTE CALCULATIONS ////

var STAT_MAX = 1000;

// This is based on how many stats you get on level up.
// Additionally attributes from gear have increased downscaling based on level.
function getPlayerAttribute(attribute, playerLevel, targetLevel) {
	var playerBaseAttribute = [
		0.030, 0.037, 0.044, 0.051, 0.058, 0.065, 0.072, 0.079, 0.086, 0.093,
		0.100, 0.100, 0.110, 0.110, 0.120, 0.120, 0.130, 0.130, 0.140, 0.140,
		0.150, 0.150, 0.164, 0.164, 0.178, 0.178, 0.193, 0.193, 0.209, 0.209,
		0.225, 0.225, 0.245, 0.245, 0.265, 0.265, 0.285, 0.285, 0.305, 0.305,
		0.325, 0.325, 0.349, 0.349, 0.373, 0.373, 0.398, 0.398, 0.424, 0.424,
		0.450, 0.450, 0.480, 0.480, 0.510, 0.510, 0.540, 0.540, 0.570, 0.570,
		0.600, 0.600, 0.634, 0.634, 0.668, 0.668, 0.703, 0.703, 0.739, 0.739,
		0.775, 0.775, 0.819, 0.819, 0.863, 0.863, 0.908, 0.908, 0.954, 0.954,
		1.000
	]
	var playerGearAttribute = [
		0.100, 0.100, 0.100, 0.100, 0.100, 0.100, 0.115, 0.130, 0.145, 0.160,
		0.175, 0.190, 0.205, 0.220, 0.235, 0.250, 0.265, 0.280, 0.305, 0.310,
		0.325, 0.340, 0.360, 0.380, 0.400, 0.420, 0.440, 0.460, 0.480, 0.500,
		0.520, 0.530, 0.540, 0.550, 0.560, 0.570, 0.580, 0.590, 0.600, 0.610,
		0.620, 0.630, 0.640, 0.650, 0.660, 0.670, 0.680, 0.690, 0.700, 0.710,
		0.720, 0.730, 0.740, 0.750, 0.760, 0.770, 0.780, 0.800, 0.820, 0.840,
		0.860, 0.880, 0.900, 0.920, 0.940, 0.960, 0.970, 0.980, 0.990, 1.000,
		1.000, 1.000, 1.000, 1.000, 1.000, 1.000, 1.000, 1.000, 1.000, 1.000,
		1.000
	]
	var fromGear = attribute - STAT_MAX * playerBaseAttribute[playerLevel];
	fromGear = fromGear / playerBaseAttribute[playerLevel] * playerBaseAttribute[targetLevel] / playerGearAttribute[playerLevel] * playerGearAttribute[targetLevel];
	return fromGear + STAT_MAX * playerBaseAttribute[targetLevel];
}

// Calculates base health at given level and profession.
function getPlayerBaseHealth(level, profession) {
    return getProfessionHealth(level, profession) + 10 * getPlayerAttribute(STAT_MAX, 80, level);
}

function getPlayerHealth(targetLevel) {
	var health = getSetting("health");
	var playerLevel = getSetting("level");
	var profession = getSetting("profession");
	// Separate base health and vitality health.
	health -= getProfessionHealth(playerLevel, profession);
	health = 10 * getPlayerAttribute(health / 10, playerLevel, targetLevel) + getProfessionHealth(targetLevel, profession);
	return  Math.ceil((1 + getSetting("healthMultiplier") / 100) * health);
}

// Calculates base health at given level and profession.
function getProfessionHealth(level, profession) {
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
    levelUps = 1 + level;
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

// Calculates base defense at given level and profession.
function getPlayerBaseDefense(level, profession) {
    if (profession == "elementalist" || profession == "mesmer" || profession == "necromancer")
        var defense = 920;
    else if (profession == "ranger" || profession == "thief" || profession == "engineer")
        var defense = 1064;
    else // Warrior,  Guardian and Revenant.
        var defense = 1211;
    return Math.round(getDefenseScale(defense, level, 80));
}


// Item level doesn't matter.
function getDefenseScale(defense, targetLevel, playerLevel) {
	// Whole things to seem bugged so just use attribute scaling values.
	var playerDefense = [
		0.030, 0.037, 0.044, 0.051, 0.058, 0.065, 0.072, 0.079, 0.086, 0.093,
		0.100, 0.100, 0.110, 0.110, 0.120, 0.120, 0.130, 0.130, 0.140, 0.140,
		0.150, 0.150, 0.164, 0.164, 0.178, 0.178, 0.193, 0.193, 0.209, 0.209,
		0.225, 0.225, 0.245, 0.245, 0.265, 0.265, 0.285, 0.285, 0.305, 0.305,
		0.325, 0.325, 0.349, 0.349, 0.373, 0.373, 0.398, 0.398, 0.424, 0.424,
		0.450, 0.450, 0.480, 0.480, 0.510, 0.510, 0.540, 0.540, 0.570, 0.570,
		0.600, 0.600, 0.634, 0.634, 0.668, 0.668, 0.703, 0.703, 0.739, 0.739,
		0.775, 0.775, 0.819, 0.819, 0.863, 0.863, 0.908, 0.908, 0.954, 0.954,
		1.000
	]
	return  Math.ceil(defense * playerDefense[targetLevel] / playerDefense[playerLevel]);
}

function getPlayerArmor(targetLevel) {
	var toughness = getSetting("toughness");
	var defense = getSetting("armor") - toughness;
	var playerLevel = getSetting("level");
	var armor = getPlayerAttribute(toughness, playerLevel, targetLevel) + getDefenseScale(defense, targetLevel, playerLevel);
	return  Math.ceil((1 + getSetting("armorMultiplier") / 100) * armor);
}

function getPercentage(damage, dungeonLevel) {
	return toPercent(damage, getPlayerHealth(dungeonLevel));
}

function getDamage(damage, weapon, potion, dungeonLevel) {
    // Not accurate because of weird armor scaling.
	var armor = Math.floor(getPlayerArmor(dungeonLevel));
	// Damage gets floored.
	return {
        min: Math.floor(damage * weapon.min / armor * (1 - potion)),
		avg: Math.floor(damage * weapon.avg / armor * (1 - potion)),
        max: Math.floor(damage * weapon.max / armor * (1 - potion))
    };
}

function getDamageWithoutWeapon(damage, potion, dungeonLevel) {
	// Damage gets floored.
	return Math.floor(damage / getPlayerArmor(dungeonLevel) * (1 - potion));
}

function getAgonyPerSecond(scale) {
	var resist = getSetting("resist");
	if (scale > 49)
        var base = 0.84;
    else if (scale > 39)
        var base = 0.66;
    else if (scale > 29)
        var base = 0.48;
    else if (scale > 19)
        var base = 0.3;
    else if (scale > 9)
        var base = 0.12;
    else
        return 0;
    if (resist < 0)
        resist = 0;
    scale -= resist * 0.012;
    if (scale < 0.01)
        scale = 0.01;
    return scale;
}

function getAgonyDamage(duration, scale) {
	// Agony probably gets floored.
    return Math.floor(getPlayerHealth(80) * getAgonyPerSecond(scale) * duration);
}

function getEffectDamage(effect, level, attribute, count) {
	var tick = 0.0;
	if (effect == "bleeding")
		tick = 2 + 0.25 * level + 0.06 * attribute;
	else if (effect == "poison")
		tick = 3.5 + 0.375 * level + 0.06 * attribute;
	else if (effect == "burning")
		tick = 7.5 + 1.55 * level + 0.155 * attribute;
	else if (effect == "torment")
		tick = 1.5 + 0.18 * level + 0.045 * attribute;
	else if (effect == "confusion1")
		tick = 2 + 0.1 * level + 0.035 * attribute;
	else if (effect == "confusion2")
		tick = 3.5 + 0.575 * level + 0.0625 * attribute;
	else if (effect == "regeneration")
		tick = 5 + 1.5625 * level + 0.125 * attribute;
	else if (effect == "regeneration")
		tick = 8 + 0.03 * level * levl + 0.075 * attribute;
	else if (effect == "might")
		tick = 5 + 0.3125 * level;
	return count * Math.floor(tick + 0.001);
}

//// ENEMY ATTRIBUTE CALCULATIONS ////

function getAttribute(level, multiplier) {
	var attributes =  [
      5,   10,   17,   22,   27,   35,   45,   50,   55,   60,
      68,   76,   84,   92,   94,   95,  103,  108,  112,  116,
      123,  129,  140,  147,  153,  160,  166,  171,  186,  192,
      208,  219,  230,  238,  253,  259,  274,  279,  284,  290,
      304,  317,  339,  353,  366,  380,  401,  416,  440,  454,
      471,  488,  514,  532,  561,  579,  598,  617,  643,  662,
      696,  718,  741,  765,  795,  818,  866,  891,  916,  941,
      976, 1004, 1059, 1089, 1119, 1149, 1183, 1214, 1274, 1307,
      1374, 1413, 1453, 1493, 1534, 1575, 1616, 1658, 1700, 1743,
      1786, 1829, 1873, 1917, 1961, 2006, 2052, 2098, 2144, 2190,
      2237
	];
	return Math.round(attributes[level] * multiplier - 0.01);
}

function getAttribute2(level, multiplier) {
	return level * multiplier;
}

function getArmor(level, multiplier) {
	var baseDefense =  [
      123,  128,  134,  138,  143,  148,  153,  158,  162,  167,
      175,  183,  185,  187,  190,  192,  202,  206,  210,  214,
      220,  224,  239,  245,  250,  256,  261,  267,  285,  291,
      311,  320,  328,  337,  356,  365,  385,  394,  402,  411,
      432,  443,  465,  476,  486,  497,  517,  527,  550,  561,
      575,  588,  610,  624,  649,  662,  676,  690,  711,  725,
      752,  769,  784,  799,  822,  837,  878,  893,  909,  924,
      949,  968, 1011, 1030, 1049, 1067, 1090, 1109, 1155, 1174,
      1223, 1247, 1271, 1295, 1319, 1343, 1367, 1391, 1415, 1439,
      1463, 1487, 1511, 1535, 1559, 1583, 1607, 1631, 1655, 1679,
      1703
	];
	return baseDefense[level] + getAttribute(level, multiplier);
}

function getHealth(level, multiplier1, multiplier2) {
	var baseHealth =  [
	  0,    18,   36,   54,   72,   90,   108,  126,  144,  162,
	  180,  198,  216,  234,  238,  243,  263,  275,  287,  299,
	  333,  367,  416,  451,  484,  517,  550,  581,  642,  675,
	  736,  769,  803,  835,  895,  929,  993,  1027, 1060, 1093,
	  1195, 1269, 1378, 1453, 1525, 1598, 1703, 1777, 1891, 1965,
	  2038, 2111, 2218, 2292, 2409, 2484, 2558, 2632, 2736, 2810,
	  2973, 3089, 3205, 3319, 3467, 3582, 3803, 3921, 4038, 4154,
	  4297, 4414, 4648, 4765, 4883, 5001, 5139, 5257, 5501, 5620,
	  5922, 6102, 6282, 6462, 6642, 6822, 7002, 7182, 7362, 7542,
	  7722, 7902, 8082, 8262, 8442, 8622, 8802, 8982, 9162, 9342,
	  9522
	];
	return Math.round(multiplier2 * (baseHealth[level] + 10 * getAttribute(level, multiplier1)) - 0.01);
}

function getCriticalChance(level, multiplier, targetLevel) {
	var criticalDefense =  [
      1.0,  1.1,  1.2,  1.3,  1.4,  1.5,  1.6,  1.7,  1.8,  1.9,
      2.0,  2.1,  2.2,  2.3,  2.4,  2.5,  2.6,  2.7,  2.8,  2.9,
      3.0,  3.2,  3.4,  3.6,  3.8,  4.0,  4.2,  4.4,  4.6,  4.8,
      5.0,  5.2,  5.4,  5.6,  5.8,  6.0,  6.2,  6.4,  6.6,  6.8,
      7.0,  7.3,  7.6,  7.9,  8.2,  8.5,  8.8,  9.1,  9.4,  9.7,
      10.0, 10.3, 10.6, 10.9, 11.2, 11.5, 11.8, 12.1, 12.4, 12.7,
      13.0, 13.4, 13.8, 14.2, 14.6, 15.0, 15.4, 15.8, 16.2, 16.6,
      17.0, 17.4, 17.8, 18.2, 18.6, 19.0, 19.4, 19.8, 20.2, 20.6,
      21.0, 21.5, 22.0, 22.5, 23.0, 23.5, 24.0, 24.5, 25.0, 25.5,
      26.0, 26.5, 27.0, 27.5, 28.0, 28.5, 29.0, 29.5, 30.0, 30.5,
      31.0
	];
	return Math.min(100, Math.max(0, Math.round(10 * (4 + getAttribute(level, multiplier - 1) / criticalDefense[targetLevel]))/10));
}

function getCriticalDamage(level, multiplier) {
	// Note: This is missing "critical damage per ferocity". So far no enemy does critical hits.
	return Math.round(10 * (150 + getAttribute(level, multiplier)))/10;
}


function getWeaponStrength(level, internalLevel, rarity, type, scale) {
	level = Number(level);
	// Weapon level gets calculated quite weird way. / 2015-10-01 / Wethospu
	var weaponLevel = Math.min(Math.max(1, Math.min(rarity, 5)) - 1 + internalLevel, 100);
	if (scale == 2)
     weaponLevel += level;
	weaponLevel = Math.min(weaponLevel, 100);
	var monsterWeaponLut = [
        162, 179, 197, 214, 231, 249, 267, 286, 303, 322,
        344, 367, 389, 394, 402, 412, 439, 454, 469, 483,
        500, 517, 556, 575, 593, 612, 622, 632, 672, 684,
        728, 744, 761, 778, 820, 839, 885, 905, 924, 943,
        991, 1016, 1067, 1093, 1119, 1145, 1193, 1220, 1275, 1304,
        1337, 1372, 1427, 1461, 1525, 1562, 1599, 1637, 1692, 1731,
        1802, 1848, 1891, 1936, 1999, 2045, 2153, 2201, 2249, 2298,
        2368, 2424, 2545, 2604, 2662, 2723, 2792, 2854, 2985, 3047,
        3191, 3269, 3348, 3427, 3508, 3589, 3671, 3754, 3838, 3922,
        4007, 4093, 4180, 4267, 4356, 4445, 4535, 4625, 4717, 4809,
        4902
    ];
	var avg = monsterWeaponLut[weaponLevel] * [0.5, 0.65, 0.8, 0.85, 0.9, 1, 1.05, 1.05][rarity] *
       [1, 1.1, 1.05, 1, 1, 1, 1.1, 1, 1, 1.1, 1.15, 1, 1.1, 0.9, 0.9, 0.9, 0.9, 1, 1, 1, 1, 1, 1, 1][type];
	   
	var weaponPowerSpread = [
        0.05, 0.06, 0.08, 0.05, 0.1, 0.03, 0.05, 0.06, 0.08, 0.1, 0.1, 0.06,
        0.06, 0.03, 0.08, 0.05, 0.06, 0.05, 0.05, 0.05, 0.05, 0.05, 0, 0
    ];
	return {
        min: Math.floor(avg - avg * weaponPowerSpread[type]),
		avg: avg,
        max: Math.floor(avg + avg * weaponPowerSpread[type])
    };
}

//// HEALTH SCALING /////

function fractalScaleHealth(health, scale, type) {
	if (!type)
		return health;
	// Formula should be 0.75% + 1.5% per level.
	// These is some weird rounding happening. This table seems to work so far.
	var championHealthTable = [
		0.76, 0.77, 0.78, 0.80, 0.82, 0.83, 0.84, 0.86, 0.88, 0.89,
		0.90, 0.92, 0.94, 0.95, 0.96, 0.98, 1.00, 1.01, 1.02, 1.04,
		1.06, 1.07, 1.08, 1.10, 1.12, 1.13, 1.14, 1.16, 1.18, 1.19,
		1.20, 1.22, 1.24, 1.25, 1.26, 1.28, 1.30, 1.31, 1.32, 1.34,
		1.36, 1.37, 1.38, 1.40, 1.42, 1.43, 1.44, 1.46, 1.48, 1.49,
		1.50, 1.52, 1.54, 1.55, 1.56, 1.58, 1.60, 1.61, 1.62, 1.64,
		1.66, 1.67, 1.68, 1.70, 1.72, 1.73, 1.74, 1.76, 1.78, 1.79,
		1.80, 1.82, 1.84, 1.85, 1.86, 1.88, 1.90, 1.91, 1.92, 1.94,
		1.96, 1.97, 1.98, 2.00, 2.02, 2.03, 2.04, 2.06, 2.08, 2.09,
		2.10, 2.12, 2.14, 2.15, 2.16, 2.18, 2.20, 2.21, 2.22, 2.24,
		2.26
    ];
	// Formula should be 0.80% + 1.5% per level.
	// These is some weird rounding happening. This table seems to work so far.
	var legendaryHealthTable = [
		0.80, 0.82, 0.83, 0.84, 0.86, 0.88, 0.89, 0.90, 0.92, 0.94,
		0.95, 0.96, 0.98, 1.00, 1.01, 1.02, 1.04, 1.06, 1.07, 1.08,
		1.10, 1.12, 1.13, 1.14, 1.16, 1.18, 1.19, 1.20, 1.22, 1.24,
		1.25, 1.26, 1.28, 1.30, 1.31, 1.32, 1.34, 1.36, 1.37, 1.38,
		1.40, 1.42, 1.43, 1.44, 1.46, 1.48, 1.49, 1.50, 1.52, 1.54,
		1.55, 1.56, 1.58, 1.60, 1.61, 1.62, 1.64, 1.66, 1.67, 1.68,
		1.70, 1.72, 1.73, 1.74, 1.76, 1.78, 1.79, 1.80, 1.82, 1.84,
		1.85, 1.86, 1.88, 1.90, 1.91, 1.92, 1.94, 1.96, 1.97, 1.98,
		2.00, 2.02, 2.03, 2.04, 2.06, 2.08, 2.09, 2.10, 2.12, 2.14,
		2.15, 2.16, 2.18, 2.20, 2.21, 2.22, 2.24, 2.26, 2.27, 2.28,
		2.30
    ];
	if (type == 1 || type == 5)
        return health;
    if (type == 2)
        return Math.ceil(health * (0.70 + scale * 0.03));
    if (type == 3)
        return Math.ceil(health * championHealthTable[scale]);
    if (type == 4)
        return Math.ceil(health * legendaryHealthTable[scale]);
	// Custom scaling (used in some special cases).
	var split = type.split(":");
	if (split.length > 1)
		return Math.ceil(health * (Number(split[0]) + scale * Number(split[1])));
    return health;
}

//// DAMAGE SCALING /////

function fractalScaleDamage(damage, scale, type) {
	if (!type)
		return damage;
	if (type == 1 || type == 5)
        return damage;
    if (type == 2 || type == 3 || type == 4)
        return Math.ceil(damage * (1 + 0.03 * scale));
	// Hardcode for everything else (expansion may change stuff so I don't want to put too much effort).
    return Math.ceil(damage * (1 + 0.0125 * scale));
}

//// LEVEL SCALING /////
function fractalScaleLevel(level, scale, type, rank) {
	if (type == null || type < 2 || rank == "champion" || rank == "legendary")
		return [ level ];
    if (scale == 1)
		return [ 80 ];
    if (scale < 20)
        return [ 81, 80 ];
    if (scale == 20)
        return [ 81 ];
    if (scale < 40)
        return [ 82, 81 ];
    if (scale == 40)
        return [ 82 ];
    if (scale < 60)
        return [ 83, 82 ];
    if (scale == 60)
        return [ 83 ];
    if (scale < 80)
        return [ 84, 83 ];;
    if (scale == 80)
        return [ 84 ];
    if (scale < 100)
        return [ 85, 84 ];
    if (scale == 100)
        return [ 85 ];
}