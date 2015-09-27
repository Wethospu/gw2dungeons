;
"use strict";
// default settings
var placeholders = {
    level: 80,
    armor: 2137,
    health: 18472,
    resist: 0,
    fractal: 10,
    defenseMultiplier: 0,
    healthMultiplier: 0,
}

// Find correct starting values from statcalculator.
function updatePlaceholderValues() {
    placeholders.health = getHealth(getSetting("level"), getSetting("profession"));
    placeholders.armor = getBaseArmor(getSetting("level"), getSetting("profession"));
    $("#armorSetting").attr("placeholder", placeholders.armor);
    $("#healthSetting").attr("placeholder", placeholders.health);
}

var settings = {
    // Values with a placeholder. //
    armor: null,                  //
    health: null,                 //
    level: null,                  //
    resist: null,                 //
    fractal: null,                //
    defenseMultiplier: null,      //
    healthMultiplier: null,       //
    ////////////////////////////////
	searchAmountName: 10000,
	searchAmountInfo: 100,
	searchAmountAll: 10,
	searchView: 'Info',
    profession: "warrior",
    potionStrength: "10",
    potionUsage: "none",
    tactics: [],
	tips: [],
    damage: "damage",
    simplifyHealth: true,
    simplifyArmor: true,
    icon: true,
    settingsVisited: false,
    comments: true,
    adSetting: "medium",
	defaultFilters: "18EvyA4",
	donePaths: [],
	dailyPaths: [],
	date: null,
	tabAmount: 10
}

function verifyDailyPaths() {
	var d = new Date();
	var date = d.getUTCFullYear() * 10000 + d.getUTCMonth() * 100 + d.getUTCDate();
	if (date != settings.date) {
		settings.dailyPaths.length = 0;
		saveSetting("dailyPaths", settings.dailyPaths);
	}
	saveSetting("date", date);
}

var validTactics = ['normal', 'melee', 'ranged', 'skip', 'coordinated', 'exploit', 'solo'];

var validTips = ['guardian', 'elementalist', 'engineer', 'mesmer', 'necromancer', 'ranger', 'revenant', 'thief', 'warrior', 'consumable', 'asura', 'charr', 'human', 'norn', 'sylvari'];

// Getter needed because of the placeholder-system.
// To make it work properly some settings can be null which means the placeholder value must be used instead.
function getSetting(setting) {
    if (settings[setting] == null)
        return placeholders[setting];
    return settings[setting];
}

// save setting to localstorage and update live
function saveSetting(setting, value) {
    $.localStorage(setting, value);
    settings[setting] = value;
}

// load settings from localstorage
function loadSettings(loadEncounters, loadEnemies) {
    for (var setting in settings) {
        var value = $.localStorage(setting);
        if (value != null)
            settings[setting] = value;
    }
    applySettings(loadEncounters, loadEnemies);
}

function applySettings(loadEncounters, loadEnemies) {
	if (loadEnemies)
		applyEnemySettings(true);
	else if (loadEncounters)
		applyEncounterSettings();

    if (settings.settingsVisited) {
        $('#topsetting').css("font-weight", "normal");
        $('#topsetting').css("color", "");
    }
    else {
        $('#topsetting').css("font-weight", "bold");
        $('#topsetting').css("color", "red");
    }
}

// Performance: Takes up to 400 ms.
function chooseCorrectTab() {
    $('.tactics').each(function () {
		// Get tactics.
		var tactics = $(this).find('a[href^=#s]').toArray();
		if (tactics.length == 0)
			return;
        for (var i = 0; i < settings['tactics'].length; i++) {
            var hash = "#" + this.id + settings['tactics'][i];
			for (var j = 0; j < tactics.length; j++) {
				if (tactics[j].hash == hash) {
					$(tactics[j]).on('shown', function (e) {
						// Prevent some kind of an update for a performance boost. / 2015-08-13 / Wethospu
						return false;
					});
					$(tactics[j]).tab('show');
					return;
				}
			}
        }
		// Fail-safe to open the first tactic if settings haven't been visited. / 2015-09-10 / Wethospu
        $(tactics[0]).on('shown', function (e) {return false;});
		$(tactics[0]).tab('show');
    });
	$('.tips').each(function () {
        // Get tips.
		var tips = $(this).find('a[href^=#t]').toArray();
		if (tips.length == 0)
			return;
        for (var i = 0; i < settings['tips'].length; i++) {
            var hash = "#" + this.id + settings['tips'][i];
			for (var j = 0; j < tips.length; j++) {
				if (tips[j].hash == hash) {
					$(tips[j]).on('shown', function (e) {
						// Prevent some kind of an update for a performance boost. / 2015-08-13 / Wethospu
						return false;
					});
					$(tips[j]).tab('show'); 
					return;
				}
			}
        }
		// Fail-safe to open the first tip if settings haven't been visited. / 2015-09-10 / Wethospu
        $(tips[0]).on('shown', function (e) {return false;});
		$(tips[0]).tab('show');
    });
}

function applyEncounterSettings() {
    chooseCorrectTab();
    // Whether to show icons.
    if (getSetting("icon"))
	{
		$('.icon').each(function () {
			$(this).css('background-image', 'url(media/img/' + $(this).attr('title') + '.png)');
		});
	}
	else
	{
		$('.icon').each(function () {
			$(this).removeClass('icon');
		});
	}
}

function applyEnemySettings(updateMain) {
	applyEncounterSettings();
    // Calculating values.
    updateValues(getSetting("armor"), getSetting("health"), getSetting("level"), getSetting("profession"), getSetting("resist"), getSetting("fractal"), getSetting("defenseMultiplier"), getSetting("healthMultiplier"));
    // Updates every enemy.
	var divId = "";
	if (updateMain)
		divId = "#main-container";
	else
		divId = "#data-overlay";
    $(divId + " .enemy").each(function () {
        handleEnemy(this);
    });
}

function handleEnemy(enemy) {
	var damageSetting = getSetting("damage");
    var potionUsage = getSetting("potionUsage");
    var potionStrength = (Number)(getSetting("potionStrength")) / 100;
	var enemyPotion = $(enemy).data("potion");
    if (enemyPotion == "" || potionUsage == "" || enemyPotion == "none" || potionUsage == "none" || (potionUsage == "main" && enemyPotion == "side"))
        potionStrength = 0;
    var dungeonLevel = getPathLevel($(enemy).data("path"));
	var playerLevel = getPlayerLevel($(enemy).data("path"), getSetting("level"));
	// Get attributes.
	var level = $(enemy).data("level");
	if (level == null)
		level = dungeonLevel;
	$(enemy).find(".level").html(level);
	var power = getAttribute(level, $(enemy).data("power"));
	$(enemy).find(".power").html(power);
	var criticalChance = getCriticalChance(level, $(enemy).data("precision"), playerLevel);
	var category = $(enemy).data("category");
	if (category == "elite" || category == "champion" || category == "legendary")
		criticalChance = 0;
	$(enemy).find(".precision").html(criticalChance);
	$(enemy).find(".armor").html(getArmor(level, $(enemy).data("toughness")));
	$(enemy).find(".health").html(getHealth(level, $(enemy).data("vitality"), $(enemy).data("health")));
	$(enemy).find(".ferocity").html(getCriticalDamage(level, $(enemy).data("ferocity")));
	var conditionDamage = getAttribute2(level, $(enemy).data("condition"));
	$(enemy).find(".condition").html(conditionDamage);
	var healingPower = getAttribute2(level, $(enemy).data("healing"));
	$(enemy).find(".healing-power").html(healingPower);
	
    // Check if scaling is needed.
    var scalingType = Number($(enemy).data("scaling"));
    if (scalingType != null && scalingType > 0) {
        scaleValues(enemy, scalingType);
    }
    // Simplify health if needed.
    if (getSetting("simplifyHealth")) {
        $(enemy).find(".health").each(function () {
            var baseHealth = (Number)($(this).html());
            $(this).html(simplifyHealth(baseHealth));
        });
    }
    // Simplify armor if needed.
    if (getSetting("simplifyArmor")) {
        $(enemy).find(".armor").each(function () {
            var baseArmor = (Number)($(this).html());
            $(this).html(simplifyArmor(baseArmor, dungeonLevel));
        });
    }

    $(enemy).find(".damageValue").each(function () {
        var damage = getDamage((Number)($(this).data('amount')), potionStrength, dungeonLevel);
        insertDamage(this, damageSetting, damage, dungeonLevel);
    });
    $(enemy).find(".percentValue").each(function () {
        var damage = calculatePercentDamage((Number)($(this).data('amount')), dungeonLevel);
        insertDamage(this, damageSetting, damage, dungeonLevel);
    });
    $(enemy).find(".effectValue").each(function () {
        var damage = (Number)($(this).data('amount'));
		var effect = $(this).data('effect');
		var attribute = conditionDamage;
		if (effect == "regeneration")
			attribute = healingPower;
		if (effect == "retaliation")
			attribute = power;
		damage = getEffectDamage(effect, level, attribute, damage);
		if (effect == "power")
		{
			damage = Math.round(1000 * damage / power) / 10;
			$(this).html(damage);
		}
		else
			insertDamage(this, damageSetting, damage, dungeonLevel);
    });
    $(enemy).find(".agonyValue").each(function () {
        var second = (Number)($(this).data('amount'));
        insertDamage(this, damageSetting, getAgonyDamage(second), dungeonLevel);
    });
    $(enemy).find(".constantValue").each(function () {
        var damage = (Number)($(this).data('amount'));
        insertDamage(this, damageSetting, damage, dungeonLevel);
    });
}

function scaleValues(data, scalingType) {
    // Read base values. These must not be altered or additional enemy instances get wrong base values!
    var baseHealth = 0;
    $(data).find(".health").each(function () {
        baseHealth = (Number)($(this).html());
    });
    var baseArmor = 0;
    $(data).find(".armor").each(function () {
        baseArmor = (Number)($(this).html());
    });
    var enemyBaseDamages = new Array();
    $(data).find(".damageValue").each(function () {
        enemyBaseDamages.push((Number)($(this).html()));
    });
    

    // Break down mode (1: no scaling, 2: normal/veteran scaling, 3: champion scaling: 4: legendary scaling, 5: level scaling).
    var fractalScale = getSetting("fractal");
    // Generate enemy levels.
    var enemyLevels = new Array()
    if (scalingType == 2 || scalingType == 5) {
        enemyLevels = fractalEnemyLevels(fractalScale).split("|");
    }
    else {
        // Hardcoded at 80 for fractals. Has to be changed if any enemies are over level 80 at scale 1.
        enemyLevels.push(80);
    }
    //// Do first scaling normally.
    var health = fractalHealthScaling(baseHealth, fractalScale, enemyLevels[0], scalingType);
    var armor = fractalLevelArmorCalculator(baseArmor, enemyLevels[0]);

    var enemyDamages = new Array();
    for (damageIndex = 0; damageIndex < enemyBaseDamages.length; damageIndex++) {
        enemyDamages.push(fractalDamageScaling(enemyBaseDamages[damageIndex], fractalScale, enemyLevels[0], scalingType));
    }
    $(data).find(".health").each(function () {
        $(this).html(health);
    });
    $(data).find(".level").each(function () {
        $(this).html(enemyLevels[0]);
    });
    $(data).find(".armor").each(function () {
        $(this).html(armor);
    });
    var index = 0;
    $(data).find(".damageValue").each(function () {
        if ($.isNumeric($(this).html())) {
            $(this).html(enemyDamages[index]);
            index++;
        }
    });

    //// Additional scalings generate new enemy instances.
    if (enemyLevels.length > 1) {
        var enemyData = data;
        // Copy original data as a reference.
        var copy = $(enemyData).clone();
        for (levelIndex = 1; levelIndex < enemyLevels.length; levelIndex++) {
            // Do scaling to the copy.
            health = fractalHealthScaling(baseHealth, fractalScale, enemyLevels[levelIndex], scalingType);
            armor = fractalLevelArmorCalculator(baseArmor, enemyLevels[levelIndex]);
            enemyDamages = new Array();
            for (damageIndex = 0; damageIndex < enemyBaseDamages.length; damageIndex++) {
                enemyDamages.push(fractalDamageScaling(enemyBaseDamages[damageIndex], fractalScale, enemyLevels[levelIndex], scalingType));
            }

            $(copy).find(".health").each(function () {
                $(this).html(health);
            });
            $(copy).find(".level").each(function () {
                $(this).html(enemyLevels[levelIndex]);
            });
            $(copy).find(".armor").each(function () {
                $(this).html(armor);
            });
            var index = 0;
            $(copy).find(".damageValue").each(function () {
                if ($.isNumeric($(this).html())) {
                    $(this).html(enemyDamages[index]);
                    index++;
                }
            });

            // Add as a new enemy.
            $(enemyData).append("</br>");
            $(enemyData).append(copy.html());
        }
    }
}

function simplifyHealth(baseHealth) {
    var health = baseHealth;
    var length = 3;
    // Move decimal point until there are only 3 numbers.
    // Keep track of number length to know whether to put k or m at end of it.
    while (health > 999) {
        health = health / 10;
        length = length + 1;
    }
    // Round up the number to remove decimals.
    health = Math.ceil(health);
    // Use number length to scale value to millions or thousands.
    if (length > 6)
        health = (Math.pow(10, length - 7) * health / 100) + "m";
    else if (length > 3)
        health = (Math.pow(10, length - 4) * health / 100) + "k";
    return "<span title=\"" + baseHealth + "\">" + health + "</span>";;
}

function simplifyArmor(armor, dungeonLevel) {
    // Level scaling probably works really bad.
    var normalArmor = getTooltipArmor(dungeonLevel);
    var ratio = armor / normalArmor;
    var type = "Medium";
    if (ratio > 2)
        type = "Massive";
    else if (ratio < 0.5)
        type = "Unarmored";
    else if (ratio > 1.25)
        type = "Very heavy";
    else if (ratio < 0.75)
        type = "Very light";
    else if (ratio > 1.05)
        type = "Heavy";
    else if (ratio < 0.95)
        type = "Light";
    type = "<span title=\"" + armor + "\">" + type + "</span>";
    return type;
}

// Adds either percent, normal or both damage.
function insertDamage(place, setting, damage, dungeonLevel) {
    if (setting == "percent")
        var toAdd = "<span title=\"" + damage + "\">" + getPercentage(damage, dungeonLevel) + "%</span>";
    else if (setting == "damage")
        var toAdd = "<span title=\"" + getPercentage(damage, dungeonLevel) + "%\">" + damage + "</span>";
    else
        var toAdd = damage + "(" + getPercentage(damage, dungeonLevel) + "%)";
	$(place).html(toAdd);
}


function getPathLevel(path) {
    if (path.substring(0, 7) == "setting")
        return 80;
    if (path.substring(0, 3) == "acs")
        return 30;
    if (path.substring(0, 2) == "ac")
        return 35;
    if (path.substring(0, 3) == "cms")
        return 40;
    if (path.substring(0, 2) == "cm")
        return 45;
    if (path.substring(0, 3) == "tas")
        return 50;
    if (path.substring(0, 3) == "taf" || path.substring(0, 3) == "tau")
        return 55;
    if (path.substring(0, 3) == "ses")
        return 60;
    if (path.substring(0, 2) == "se")
        return 65;
    if (path.substring(0, 4) == "cofs")
        return 70;
    if (path.substring(0, 3) == "cof")
        return 75;
    if (path.substring(0, 5) == "hotws")
        return 76;
    if (path.substring(0, 4) == "coes")
        return 78;
    return 80;
}

function getPlayerLevel(path, maxLevel) {
    var pathLevel = getPathLevel(path);
	if (path.substring(0, 3) == "acs" || path.substring(0, 3) == "cms" || path.substring(0, 3) == "tas"
	|| path.substring(0, 3) == "ses" || path.substring(0, 4) == "cofs" || path.substring(0, 5) == "hotws"
	|| path.substring(0, 4) == "coes") {
		// Players have up to 2 more levels in story modes.
		if (maxLevel == pathLevel + 1)
			return maxLevel;
		if (maxLevel > pathLevel);
			pathLevel += 2;
	}
	// Dungeons don't scale players up.
	if (path.substring(0, 2) == "ac" || path.substring(0, 2) == "cm" || path.substring(0, 2) == "ta"
	|| path.substring(0, 2) == "se" || path.substring(0, 3) == "cof" || path.substring(0, 4) == "hotw"
	|| path.substring(0, 3) == "coe" || path.substring(0, 4) == "arah") {
		if (maxLevel < pathLevel)
			pathLevel = maxLevel;
	}
	return pathLevel;	
}
