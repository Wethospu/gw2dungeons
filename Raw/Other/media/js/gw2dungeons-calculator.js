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
	else if (effect == "power")
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
	return Math.ceil(attributes[level] * multiplier - 0.001);
}

function getAttribute2(level, multiplier) {
	return level * multiplier;
}

function getArmor(level, multiplier) {
	var bsaeDefense =  [
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
	return bsaeDefense[level] + getAttribute(level, multiplier);
}

function getHealth(level, multiplier1, multiplier2) {
	var baseHealth =  [
      0,   20,   40,   60,   80,  100,  120,  140,  160,  180,
      200,  220,  240,  260,  265,  270,  292,  306,  319,  332,
      370,  408,  462,  501,  538,  575,  611,  646,  713,  750,
      818,  855,  892,  928,  995, 1032, 1103, 1141, 1178, 1215,
      1328, 1410, 1531, 1614, 1695, 1776, 1892, 1974, 2101, 2183,
      2265, 2346, 2464, 2547, 2677, 2760, 2842, 2924, 3040, 3122,
      3303, 3432, 3561, 3688, 3852, 3980, 4226, 4357, 4487, 4616,
      4774, 4904, 5164, 5295, 5426, 5557, 5710, 5841, 6112, 6245,
      6580, 6780, 6980, 7180, 7380, 7580, 7780, 7980, 8180, 8380,
      8580, 8780, 8980, 9180, 9380, 9580, 9780, 9980,10180,10380,
      10580
	];
	return Math.ceil(multiplier2 * Math.ceil(0.9 * baseHealth[level] - 0.001) + getAttribute(level, multiplier1) - 0.001);
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
	return Math.max(0, Math.round(10 * (getAttribute(level, multiplier) / criticalDefense[targetLevel] - 43.119047619))/10);
}

function getCriticalDamage(level, multiplier) {
	// Note: This is missing "critical damage per ferocity". So far no enemy does critical hits.
	return Math.round(10 * (150 + getAttribute(level, multiplier)))/10;
}

////// These values must match GW2Helper.cs ///////
//// HEALTH SCALING /////

function fractalScaleHealth(health, scale, type) {
	var normalHealthTable = [
      1.000000000, 1.042855700, 1.085711400, 1.128567100, 1.171422800, 1.214278500, 1.257134200, 1.299989900, 1.342845600, 1.385701300,
      1.428557000, 1.471412700, 1.514268400, 1.557124100, 1.599979800, 1.642835500, 1.685691200, 1.728546900, 1.771402600, 1.814258300,
      1.857114000, 1.899969700, 1.942825400, 1.985681100, 2.028536800, 2.071392500, 2.114248200, 2.157103900, 2.199959600, 2.242815300,
      2.285671000, 2.328526700, 2.371382400, 2.414238100, 2.457093800, 2.499949500, 2.542805200, 2.585660900, 2.628516600, 2.671372300,
      2.714228000, 2.757083700, 2.799939400, 2.842795100, 2.885650800, 2.928506500, 2.971362200, 3.014217900, 3.057073600, 3.099929300,
      3.142785000, 3.185640700, 3.228496400, 3.271352100, 3.314207800, 3.357063500, 3.399919200, 3.442774900, 3.485630600, 3.528486300,
      3.571342000, 3.614197700, 3.657053400, 3.699909100, 3.742764800, 3.785620500, 3.828476200, 3.871331900, 3.914187600, 3.957043300,
      3.999899000, 4.042754700, 4.085610400, 4.128466100, 4.171321800, 4.214177500, 4.257033200, 4.299888900, 4.342744600, 4.385600300,
      4.428456000, 4.471311700, 4.514167400, 4.557023100, 4.599878800, 4.642734500, 4.685590200, 4.728445900, 4.771301600, 4.814157300,
      4.857013000, 4.899868700, 4.942724400, 4.985580100, 5.028435800, 5.071291500, 5.114147200, 5.157002900, 5.199858600, 5.242714300,
      5.285570000
    ];
	var championHealthTable = [
      1.000000000, 1.013333333, 1.040000000, 1.066666667, 1.080000000, 1.093333333, 1.120000000, 1.146666667, 1.160000000, 1.173333333,
      1.200000000, 1.226666667, 1.240000000, 1.253333333, 1.280000000, 1.306666667, 1.320000000, 1.333333333, 1.360000000, 1.386666667,
      1.400000000, 1.413333333, 1.440000000, 1.466666667, 1.480000000, 1.493333333, 1.520000000, 1.546666667, 1.560000000, 1.573333333,
      1.600000000, 1.626666667, 1.640000000, 1.653333333, 1.680000000, 1.706666667, 1.720000000, 1.733333333, 1.760000000, 1.786666667,
      1.800000000, 1.813333333, 1.840000000, 1.866666667, 1.880000000, 1.893333333, 1.920000000, 1.946666667, 1.960000000, 1.973333333,
      2.000000000, 2.026666667, 2.040000000, 2.053333333, 2.080000000, 2.106666667, 2.120000000, 2.133333333, 2.160000000, 2.186666667,
      2.200000000, 2.213333333, 2.240000000, 2.266666667, 2.280000000, 2.293333333, 2.320000000, 2.346666667, 2.360000000, 2.373333333,
      2.400000000, 2.426666667, 2.440000000, 2.453333333, 2.480000000, 2.506666667, 2.520000000, 2.533333333, 2.560000000, 2.586666667,
      2.600000000, 2.613333333, 2.640000000, 2.666666667, 2.680000000, 2.693333333, 2.720000000, 2.746666667, 2.760000000, 2.773333333,
      2.800000000, 2.826666667, 2.840000000, 2.853333333, 2.880000000, 2.906666667, 2.920000000, 2.933333333, 2.960000000, 2.986666667,
      3.000000000
    ];
	var legendaryHealthTable = [
      1.000000000, 1.012500000, 1.037500000, 1.062500000, 1.075000000, 1.087500000, 1.112500000, 1.137500000, 1.150000000, 1.162500000,
      1.187500000, 1.212500000, 1.225000000, 1.237500000, 1.262500000, 1.287500000, 1.300000000, 1.312500000, 1.337500000, 1.362500000,
      1.375000000, 1.387500000, 1.412500000, 1.437500000, 1.450000000, 1.462500000, 1.487500000, 1.512500000, 1.525000000, 1.537500000,
      1.562500000, 1.587500000, 1.600000000, 1.612500000, 1.637500000, 1.662500000, 1.675000000, 1.687500000, 1.712500000, 1.737500000,
      1.750000000, 1.762500000, 1.787500000, 1.812500000, 1.825000000, 1.837500000, 1.862500000, 1.887500000, 1.900000000, 1.912500000,
      1.937500000, 1.962500000, 1.975000000, 1.987500000, 2.012500000, 2.037500000, 2.050000000, 2.062500000, 2.087500000, 2.112500000,
      2.125000000, 2.137500000, 2.162500000, 2.187500000, 2.200000000, 2.212500000, 2.237500000, 2.262500000, 2.275000000, 2.287500000,
      2.312500000, 2.337500000, 2.350000000, 2.362500000, 2.387500000, 2.412500000, 2.425000000, 2.437500000, 2.462500000, 2.487500000,
      2.500000000, 2.512500000, 2.537500000, 2.562500000, 2.575000000, 2.587500000, 2.612500000, 2.637500000, 2.650000000, 2.662500000,
      2.687500000, 2.712500000, 2.725000000, 2.737500000, 2.762500000, 2.787500000, 2.800000000, 2.812500000, 2.837500000, 2.862500000,
      2.875000000
    ];
    if (type == 2)
        return Math.ceil(health * normalHealthTable[scale]);
    if (type == 3)
        return Math.ceil(health * championHealthTable[scale]);
    if (type == 4)
        return Math.ceil(health * legendaryHealthTable[scale]);
    return health;
}

//// DAMAGE SCALING /////

function fractalScaleDamage(damage, scale, type) {
	var normalDamageTable = [
      1.000000000, 1.030000000, 1.060000000, 1.090000000, 1.120000000, 1.150000000, 1.180000000, 1.210000000, 1.240000000, 1.270000000,
      1.300000000, 1.330000000, 1.360000000, 1.390000000, 1.420000000, 1.450000000, 1.480000000, 1.510000000, 1.540000000, 1.570000000,
      1.600000000, 1.630000000, 1.660000000, 1.690000000, 1.720000000, 1.750000000, 1.780000000, 1.810000000, 1.840000000, 1.870000000,
      1.900000000, 1.930000000, 1.960000000, 1.990000000, 2.020000000, 2.050000000, 2.080000000, 2.110000000, 2.140000000, 2.170000000,
      2.200000000, 2.230000000, 2.260000000, 2.290000000, 2.320000000, 2.350000000, 2.380000000, 2.410000000, 2.440000000, 2.470000000,
      2.500000000, 2.530000000, 2.560000000, 2.590000000, 2.620000000, 2.650000000, 2.680000000, 2.710000000, 2.740000000, 2.770000000,
      2.800000000, 2.830000000, 2.860000000, 2.890000000, 2.920000000, 2.950000000, 2.980000000, 3.010000000, 3.040000000, 3.070000000,
      3.100000000, 3.130000000, 3.160000000, 3.190000000, 3.220000000, 3.250000000, 3.280000000, 3.310000000, 3.340000000, 3.370000000,
      3.400000000, 3.430000000, 3.460000000, 3.490000000, 3.520000000, 3.550000000, 3.580000000, 3.610000000, 3.640000000, 3.670000000,
      3.700000000, 3.730000000, 3.760000000, 3.790000000, 3.820000000, 3.850000000, 3.880000000, 3.910000000, 3.940000000, 3.970000000,
      4.000000000
    ];
	var bossDamageTable = [
      1.000000000, 1.032200000, 1.064400000, 1.096600000, 1.128800000, 1.161000000, 1.193200000, 1.225400000, 1.257600000, 1.289800000,
      1.322000000, 1.354200000, 1.386400000, 1.418600000, 1.450800000, 1.483000000, 1.515200000, 1.547400000, 1.579600000, 1.611800000,
      1.644000000, 1.676200000, 1.708400000, 1.740600000, 1.772800000, 1.805000000, 1.837200000, 1.869400000, 1.901600000, 1.933800000,
      1.966000000, 1.998200000, 2.030400000, 2.062600000, 2.094800000, 2.127000000, 2.159200000, 2.191400000, 2.223600000, 2.255800000,
      2.288000000, 2.320200000, 2.352400000, 2.384600000, 2.416800000, 2.449000000, 2.481200000, 2.513400000, 2.545600000, 2.577800000,
      2.610000000, 2.642200000, 2.674400000, 2.706600000, 2.738800000, 2.771000000, 2.803200000, 2.835400000, 2.867600000, 2.899800000,
      2.932000000, 2.964200000, 2.996400000, 3.028600000, 3.060800000, 3.093000000, 3.125200000, 3.157400000, 3.189600000, 3.221800000,
      3.254000000, 3.286200000, 3.318400000, 3.350600000, 3.382800000, 3.415000000, 3.447200000, 3.479400000, 3.511600000, 3.543800000,
      3.576000000, 3.608200000, 3.640400000, 3.672600000, 3.704800000, 3.737000000, 3.769200000, 3.801400000, 3.833600000, 3.865800000,
      3.898000000, 3.930200000, 3.962400000, 3.994600000, 4.026800000, 4.059000000, 4.091200000, 4.123400000, 4.155600000, 4.187800000,
      4.220000000
    ];
    if (type == 2)
        return Math.ceil(damage * normalDamageTable[scale]);
    if (type == 3)
        return Math.ceil(damage * bossDamageTable[scale]);
    if (type == 4)
        return Math.ceil(damage * bossDamageTable[scale]);
    return damage;
}

//// LEVEL SCALING /////
function fractalScaleLevel(level, scale, type) {
	if (type == null || type < 1)
		return [ level ];
    if (scale == 1 || (type != 2 && type != 5))
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