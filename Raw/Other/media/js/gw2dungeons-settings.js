;
"use strict";
// default settings
var placeholders = {
    level: 80,
    armor: 2137,
	toughness: 1000,
    health: 19212,
    agonyResist: 0,
	agonyResistAqua: 0,
    armorMultiplier: 0,
    healthMultiplier: 0
}

// Find correct starting values from statcalculator.
function updatePlaceholderValues() {
    placeholders.health = getPlayerBaseHealth(getSetting("level"), getSetting("profession"));
	placeholders.toughness = getPlayerAttribute(STAT_MAX, 80, getSetting("level"));
    placeholders.armor = getPlayerBaseDefense(getSetting("level"), getSetting("profession")) + placeholders.toughness;
    $("#armorSetting").attr("placeholder", placeholders.armor);
    $("#healthSetting").attr("placeholder", placeholders.health);
	$("#toughnessSetting").attr("placeholder", placeholders.toughness);
}

var settings = {
    // Values with a placeholder. //
    armor: null,                  //
	toughness: null,              //
    health: null,                 //
    level: null,                  //
    agonyResist: null,            //
	agonyResistAqua: null,        //
    armorMultiplier: null,        //
    healthMultiplier: null,       //
    ////////////////////////////////
	fractal: 10,
	profession: "warrior",
    potionStrength: "10",
    potionUsage: "none",
	// Search settings.
	searchAmountName: 10000,
	searchAmountInfo: 100,
	searchAmountAll: 10,
	searchView: 'Info',
	// General settings.
	properCooldowns: false,
    settingsVisited: false,
	homePage: "fractals",
    comments: true,
	useSidebar: false,
    adSetting: "big",
	defaultFilters: "18EvyA4",
	fontSize: 100,
	mainShowIcons: true,
    // Tab settings.
    tactics: [],
	tips: [],
	tabAmount: 10,
	// Enemy overlay settings.
	overFontSize: 100,
	overShowIcons: true,
    overDamageView: "number",
	overHealingView: "number",
	overDamageRange: "range",
	overShowAllInfo: true,
	overShowAnimations: true,
	overShowTactics: true,
	overShowImages: true,
    overSimplifyHealth: true,
    overSimplifyArmor: true,
	overShowHealth: true,
	overShowArmor: true,
	overShowWeaponStrength: true,
	overShowPower: true,
	overShowOffense: false,
	overShowPrecision: true,
	overShowFerocity: true,
	overShowCondition: true,
	overShowHealing: true,
	overShowRace: true,
    overShowSize: true,
	overShowRank: true,
	overShowGender: true,
	overShowLevel: true,
	overShowPathSelection : true,
	overShowTargetLevel: true,
	overShowFractalLevel: true,
	overShowScalingMode: true,
	overShowCooldowns: true,
	overShowRanges: true,
	// Enemy sidebar settings.
	sideFontSize: 80,
	sideShowIcons: true,
    sideDamageView: "number",
	sideHealingView: "number",
	sideDamageRange: "maximum",
	sideShowAllInfo: false,
	sideShowAnimations: false,
	sideShowTactics: false,
	sideShowImages: false,
    sideSimplifyHealth: true,
    sideSimplifyArmor: true,
	sideShowHealth: true,
	sideShowArmor: true,
	sideShowWeaponStrength: false,
	sideShowPower: false,
	sideShowOffense: false,
	sideShowPrecision: false,
	sideShowFerocity: false,
	sideShowCondition: false,
	sideShowHealing: false,
	sideShowRace: true,
    sideShowSize: true,
	sideShowRank: true,
	sideShowGender: false,
	sideShowLevel: true,
	sideShowPathSelection : false,
	sideShowTargetLevel: false,
	sideShowFractalLevel: false,
	sideShowScalingMode: false,
	sideShowCooldowns: true,
	sideShowRanges: true,
	// Daily tracker memory.
	donePaths: [],
	dailyPaths: [],
	date: null
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
    if (loadEnemies)
		applyEnemySettings("main");
	else if (loadEncounters)
		applyEncounterSettings("#main-container");

    if (settings.settingsVisited) {
        $('#nav-setting').css("font-weight", "normal");
        $('#nav-setting').css("color", "");
    }
    else {
        $('#nav-setting').css("font-weight", "bold");
        $('#nav-setting').css("color", "red");
    }
}

function loadIcons(setting) {
	if (getSetting(setting + "ShowIcons"))
	{
		$('.icon').each(function () {
			$(this).css('background-image', 'url(media/img/' + $(this).attr('data-src') + '.png)');
		});
	}
	else
	{
		$('.icon').each(function () {
			$(this).removeClass('icon');
		});
	}
}

// Performance: Takes up to 400 ms.
function chooseCorrectTab(location) {
    $(location + ' .tactics').each(function () {
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
	$(location + ' .tips').each(function () {
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

function applyEncounterSettings(location) {
    chooseCorrectTab(location);
	$('#main-container').css('font-size', '' + getSetting('fontSize') + '%')
	$('#data-overlay').css('font-size', '' + getSetting('overFontSize') + '%')
	$('#detail-container').css('font-size', '' + getSetting('sideFontSize') + '%')
	
}

function applyEnemySettings(setting) {
	if (setting == "main") {
		var location = "#main-container";
		setting = "over";
	}
	else if (setting == "over")
		var location = "#data-overlay";	
	else
		var location = "#detail-container";
	applyEncounterSettings(location);
	loadIcons(setting);
    $(location + " .enemy").each(function () {
        handleEnemy(this, setting);
    });
}

function handleEnemy(enemy, setting) {
	var damageView = getSetting(setting + "DamageView");
	var damageRange = getSetting(setting + "DamageRange")
	var healingView = getSetting(setting + "HealingView");
	var scalingType = $(enemy).attr("data-scaling");
	var rank = $(enemy).attr("data-rank");
	var currentPath = $(enemy).attr("data-current-path");
	// Without current path, default to the first one available. / 2015-10-09 / Wethospu
	if (!currentPath)
		currentPath = $(enemy).attr("data-path").split("|")[0];
	
	// Check potion settings and whether they apply to this enemy. / 2015-10-10 / Wethospu
	var potionUsage = getSetting("potionUsage");
    var potionStrength = (Number)(getSetting("potionStrength")) / 100;
	var enemyPotion = getPotionStrength(currentPath, $(enemy).attr("data-race"));
    if (enemyPotion == "" || potionUsage == "" || enemyPotion == "none" || potionUsage == "none" || (potionUsage == "main" && enemyPotion == "side"))
        potionStrength = 0;
	
	var gameMode = pathToGameMode(currentPath);
	// Set attributes and visibility.
	var playerLevel = $(enemy).attr("data-target-level");
	if (playerLevel == null || playerLevel == '') {
		playerLevel = getPlayerLevel(currentPath, getSetting("level"));
		$(enemy).attr("data-target-level", playerLevel);
	}
	if (getSetting(setting + "ShowTargetLevel") && gameMode == "dungeon") {
		$(enemy).find(".target-level-unit").show();
		$(enemy).find(".target-level").html(playerLevel);
	}	
	else
		$(enemy).find(".target-level-unit").hide();
	
	var fractalLevel = $(enemy).attr("data-fractal-level");
	if (fractalLevel == null) {
		fractalLevel = getSetting("fractal");
		$(enemy).attr("data-fractal-level", fractalLevel);
	}
	if (fractalLevel != getSetting("fractal"))
		saveSetting("fractal", fractalLevel);
	if (getSetting(setting + "ShowFractalLevel") && gameMode == "fractal") {
		$(enemy).find(".fractal-level-unit").show();
		$(enemy).find(".fractal-level").html(fractalLevel);
	}	
	else
		$(enemy).find(".fractal-level-unit").hide();
	
	if (getSetting(setting + "ShowScalingMode") && gameMode == "fractal")
		$(enemy).find(".scaling-unit").show();
	else
		$(enemy).find(".scaling-unit").hide();
	
	var level = $(enemy).attr("data-level");
	if (level == null || level == '') {
		level = getPathLevel(currentPath);
		// Without set level, allow fractal scale to affect it. / 2015-09-30 / Wethospu
		level = fractalScaleLevel(level, fractalLevel, scalingType, rank)[0];
		$(enemy).attr("data-level", level);
	}
		
	if (getSetting(setting + "ShowLevel")) {
		$(enemy).find(".level-unit").show();
		$(enemy).find(".level").html(level);
	}	
	else
		$(enemy).find(".level-unit").hide();
	
	
	if (getSetting(setting + "ShowGender"))
		$(enemy).find(".gender-unit").show();
	else
		$(enemy).find(".gender-unit").hide();
	
	if (getSetting(setting + "ShowRace"))
		$(enemy).find(".race-unit").show();
	else
		$(enemy).find(".race-unit").hide();
	
	if (getSetting(setting + "ShowRank"))
		$(enemy).find(".rank-unit").show();
	else
		$(enemy).find(".rank-unit").hide();
	
	if (getSetting(setting + "ShowSize"))
		$(enemy).find(".size-unit").show();
	else
		$(enemy).find(".size-unit").hide();
	
	if (getSetting(setting + "ShowRanges"))
		$(enemy).find(".range-unit").show();
	else
		$(enemy).find(".range-unit").hide();
	
	if (getSetting(setting + "ShowPathSelection"))
		$(enemy).find(".current-path-unit").show();
	else
		$(enemy).find(".current-path-unit").hide();
	
	if (getSetting(setting + "ShowAllInfo"))
		$(enemy).find(".secondary-info").show();
	else
		$(enemy).find(".secondary-info").hide();
	
	if (getSetting(setting + "ShowTactics"))
		$(enemy).find(".tactics").show();
	else
		$(enemy).find(".tactics").hide();
	
	if (getSetting(setting + "ShowTactics"))
		$(enemy).find(".tips").show();
	else
		$(enemy).find(".tips").hide();
	
	if (getSetting(setting + "ShowImages"))
		$(enemy).find(".enemy-media").show();
	else
		$(enemy).find(".enemy-media").hide();
	
	if (getSetting(setting + "ShowAnimations"))
		$(enemy).find(".animation-unit").show();
	else
		$(enemy).find(".animation-unit").hide();
	
	if (getSetting(setting + "ShowCooldowns")) {
		$(enemy).find(".cooldown-unit").show();
		$(enemy).find(".cooldown").each(function () {
			var cooldown = $(this).attr("data-amount");
			var internal = $(this).attr("data-internal");
			if (cooldown != null && (internal == null || !getSetting("properCooldowns")))
				$(this).html(cooldown);
			else
				$(this).html(internal);
		});
	}	
	else
		$(enemy).find(".cooldown-unit").hide();
	
	var power = getAttribute(level, $(enemy).attr("data-power"));
	if (getSetting(setting + "ShowPower") && !isNaN(power)) {
		$(enemy).find(".power-unit").show();
		$(enemy).find(".power").html(power);
	}	
	else
		$(enemy).find(".power-unit").hide();
	
	var criticalChance = getCriticalChance(level, $(enemy).attr("data-precision"), playerLevel);
	if (rank == "elite" || rank == "champion" || rank == "legendary")
		criticalChance = 0;
	if (getSetting(setting + "ShowPrecision") && !isNaN(criticalChance)) {
		$(enemy).find(".precision-unit").show();
		$(enemy).find(".precision").html(criticalChance);
	}	
	else
		$(enemy).find(".precision-unit").hide();
	
	var armor = getArmor(level, $(enemy).attr("data-toughness"));
	if (getSetting(setting + "ShowArmor") && !isNaN(armor)) {
		$(enemy).find(".armor-unit").show();
		$(enemy).find(".armor").html(armor);
	}	
	else
		$(enemy).find(".armor-unit").hide();
	
	var health = getHealth(level, $(enemy).attr("data-vitality"), $(enemy).attr("data-health"));
	health = fractalScaleHealth(health, fractalLevel, scalingType);
	if (getSetting(setting + "ShowHealth") && !isNaN(health)) {
		$(enemy).find(".health-unit").show();
		$(enemy).find(".health").html(health);
	}
	else
		$(enemy).find(".health-unit").hide();
	
	var criticalDamage = getCriticalDamage(level, $(enemy).attr("data-ferocity"));
	if (getSetting(setting + "ShowFerocity") && !isNaN(criticalDamage)) {
		$(enemy).find(".ferocity-unit").show();
		$(enemy).find(".ferocity").html(criticalDamage);
	}
	else
		$(enemy).find(".ferocity-unit").hide();
	
	var conditionDamage = getAttribute2(level, $(enemy).attr("data-condition"));
	if (getSetting(setting + "ShowCondition") && !isNaN(conditionDamage)) {
		$(enemy).find(".condition-unit").show();
		$(enemy).find(".condition").html(conditionDamage);
	}
	else
		$(enemy).find(".condition-unit").hide();
	
	var healingPower = getAttribute2(level, $(enemy).attr("data-healing"));
	if (getSetting(setting + "ShowHealing") && !isNaN(healingPower)) {
		$(enemy).find(".healing-unit").show();
		$(enemy).find(".healing-power").html(healingPower);
	}
	else
		$(enemy).find(".healing-unit").hide();

	var weaponStrengthMain = getWeaponStrength(level, $(enemy).attr("data-weapon-main-level"), $(enemy).attr("data-weapon-main-rarity"), $(enemy).attr("data-weapon-main-type"), $(enemy).attr("data-weapon-main-scale"));
	if ($(enemy).attr("data-weapon-off-level") != null)
		var weaponStrengthOff = getWeaponStrength(level, $(enemy).attr("data-weapon-off-level"), $(enemy).attr("data-weapon-off-rarity"), $(enemy).attr("data-weapon-off-type"), $(enemy).attr("data-weapon-off-scale"));
	if ($(enemy).attr("data-weapon-water-level") != null)
		var weaponStrengthWater = getWeaponStrength(level, $(enemy).attr("data-weapon-water-level"), $(enemy).attr("data-weapon-water-rarity"), $(enemy).attr("data-weapon-water-type"), $(enemy).attr("data-weapon-water-scale"));
	if (getSetting(setting + "ShowWeaponStrength") && !isNaN(weaponStrengthMain.min) && !isNaN(weaponStrengthMain.max)) {
		$(enemy).find(".weapon-unit").show();
		$(enemy).find(".weapon").html(weaponStrengthMain.min + "-" + weaponStrengthMain.max);
	}
	else
		$(enemy).find(".weapon-unit").hide();
	
	if (getSetting(setting + "ShowOffense") && !isNaN(power) && !isNaN(weaponStrengthMain.avg)) {
		var offense = Math.round(100 * power * weaponStrengthMain.avg / getArmor(level, 1) / getPlayerHealth(80, "elementalist"));
		$(enemy).find(".offense-unit").show();
		$(enemy).find(".offense").html(offense);
	}
	else
		$(enemy).find(".offense-unit").hide();
	
    // Simplify health if needed.
    if (getSetting(setting + "ShowHealth") && getSetting(setting + "SimplifyHealth")) {
        $(enemy).find(".health").each(function () {
            var baseHealth = (Number)($(this).html());
            $(this).html(simplifyHealth(baseHealth));
        });
    }
    // Simplify armor if needed.
    if (getSetting(setting + "ShowArmor") && getSetting(setting + "SimplifyArmor")) {
        $(enemy).find(".armor").each(function () {
            var baseArmor = (Number)($(this).html());
            $(this).html(simplifyArmor(baseArmor, playerLevel));
        });
    }

    $(enemy).find(".damage-value").each(function () {
		var damage = $(this).attr("data-amount");
		// Check is damage hard coded (over 100).
		if (damage < 100) {
			damage = fractalScaleDamage(damage * power, fractalLevel, scalingType)
			var weaponSlot = $(this).attr("data-weapon");
			if (weaponSlot == "water")
				var damages = getDamage(damage, weaponStrengthWater, potionStrength, playerLevel);
			else if (weaponSlot == "off")
				var damages = getDamage(damage, weaponStrengthOff, potionStrength, playerLevel);
			else
				var damages = getDamage(damage, weaponStrengthMain, potionStrength, playerLevel);
			insertDamageRange(this, damages, damageView, damageRange, playerLevel);
		}
		else {
			damage = getDamageWithoutWeapon(fractalScaleDamage(damage, fractalLevel, scalingType), potionStrength, playerLevel);
			insertDamage(this, damage, damageView, playerLevel);
		}
    });
    $(enemy).find(".percent-value").each(function () {
        var damage = getPercentage((Number)($(this).attr("data-amount")), playerLevel);
        insertDamage(this, damage, damageView, playerLevel);
    });
    $(enemy).find(".effect-value").each(function () {
        var damage = (Number)($(this).attr("data-amount"));
		var effect = $(this).attr("data-effect");
		var attribute = conditionDamage;
		if (effect == "regeneration")
			attribute = healingPower;
		if (effect == "retaliation")
			attribute = power;
		damage = getEffectDamage(effect, level, attribute, damage);
		if (effect == "might")
			$(this).html(toPercent(damage, power) + "%");
		else if (effect == "healingPower")
			insertHealing(this, damage, healingView, health);
		else
			insertDamage(this, damage, damageView, playerLevel);
    });
	var resist = getSetting("agonyResistance");
	if (currentPath == "aqua")
		resist = getSetting("agonyResistanceAqua");
    $(enemy).find(".agony-value").each(function () {
        var second = (Number)($(this).attr("data-amount"));
        insertDamage(this, getAgonyDamage(second, fractalLevel, resist), damageView, playerLevel);
    });
    $(enemy).find(".fixed-value").each(function () {
        var damage = (Number)($(this).attr("data-amount"));
        insertDamage(this, damage, damageView, playerLevel);
    });
	$(enemy).find(".healing-value").each(function () {
        var healing = (Number)($(this).attr("data-amount"));
		insertHealing(this, healing, healingView, health);
    });
	$(enemy).find(".healing-percent-value").each(function () {
        var healing = Math.floor($(this).attr("data-amount") * health / 100);
		insertHealing(this, healing, healingView, health);
    });
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
    var normalArmor = getArmor(dungeonLevel, 1);
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
function insertDamage(place, amount, viewSetting, dungeonLevel) {
    if (viewSetting == "percent")
        var toAdd = "<span title=\"" + amount + "\">" + getPercentage(amount, dungeonLevel) + "%</span>";
    else if (viewSetting == "number")
        var toAdd = "<span title=\"" + getPercentage(amount, dungeonLevel) + "%\">" + amount + "</span>";
    else
        var toAdd = amount + " (" + getPercentage(amount, dungeonLevel) + "%)";
	$(place).html(toAdd);
}

function insertDamageRange(place, amounts, viewSetting, rangeSetting, dungeonLevel) {
	if (rangeSetting == "maximum") {
		var damageStr = amounts.max;
		var percentStr = getPercentage(amounts.max, dungeonLevel);
	}
	else if (rangeSetting == "minimum") {
		var damageStr = amounts.min;
		var percentStr = getPercentage(amounts.min, dungeonLevel);
	}
	else if (rangeSetting == "average") {
		var damageStr = amounts.avg;
		var percentStr = getPercentage(amounts.avg, dungeonLevel);
	}
	else {
		var damageStr = "" + amounts.min + "-" + amounts.max;
		var percentStr = "" + getPercentage(amounts.min, dungeonLevel) + "-" + getPercentage(amounts.max, dungeonLevel);
	}
    if (viewSetting == "percent")
        var toAdd = "<span title=\"" + damageStr + "\">" + percentStr + "%</span>";
    else if (viewSetting == "number")
        var toAdd = "<span title=\"" + percentStr + "%\">" + damageStr + "</span>";
    else
        var toAdd = damageStr + " (" + percentStr + "%)";
	$(place).html(toAdd);
}

// Adds either percent, normal or both healing.
function insertHealing(place, amount, viewSetting, maxHealth) {
    if (viewSetting == "percent")
        var toAdd = "<span title=\"" + amount + "\">" + toPercent(amount, maxHealth) + "%</span>";
    else if (viewSetting == "number")
        var toAdd = "<span title=\"" + toPercent(amount, maxHealth) + "%\">" + amount + "</span>";
    else
        var toAdd = amount + " (" + toPercent(amount, maxHealth) + "%)";
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

function pathToGameMode(path) {
	if (path.substring(0, 2) == "ac" || path.substring(0, 2) == "cm" || path.substring(0, 2) == "ta"
	|| path.substring(0, 2) == "se" || path.substring(0, 3) == "cof" || path.substring(0, 4) == "hotw"
	|| path.substring(0, 3) == "coe" || path.substring(0, 4) == "arah") {
		return "dungeon";
	}
	return "fractal";
}

// Returns potion usage for every dungeon. Very inefficient! / 2015-10-10 / Wethospu
function getPotionStrength(path, race) {
	if (path.substring(0, 2) == "ac")
		return "none";
	if (path.substring(0, 2) == "cm")
		return race == "bandit" ? "main" : "none";
	if (path == "tas" || path == "taf" || path == "tau")
		return race == "nightmare_court" ? "main" : "none";
	if (path == "taae") {
		if (race == "aetherblade" || race == "nightmare_court")
			return "main";
		return "none";
	}
	if (path == "ses" || path == "ses1")
		return race == "inquest" ? "main" : "none";
	if (path == "se2" || path == "ses3") {
		if (race == "dredge")
			return "main";
		if (race == "inquest" || race == "destroyer")
			return "side";
		return "none";
	}
	if (path.substring(0, 3) == "cof")
		return race == "flame_legion" ? "main" : "none";
	if (path.substring(0, 4) == "hotw") {
		if (race == "icebrood")
			return "main";
		if (race == "sons_of_svanir")
			return "side";
		return "none";
	}
	if (path == "coes") {
		if (race == "inquest")
			return "main";
		if (race == "bandit")
			return "side";
		return "none";
	}
	if (path.substring(0, 3) == "coe") {
		if (race == "undead")
			return "main";
		if (race == "inquest" || race == "destroyer" || race == "icebrood")
			return "side";
		return "none";
	}
	if (path.substring(0, 4) == "arah")
		return race == "undead" ? "main" : "none";
	if (path == "aether")
		return race == "aetherblade" ? "main" : "none";
	if (path == "aqua")
		return race == "krait" ? "side" : "none";
	if (path == "cliff")
		return race == "bandit" ? "main" : "none";
	if (path == "furn")
		return race == "molten" ? "main" : "none";
	if (path == "mai") {
		if (race == "aetherblade")
			return "main";
		if (race == "inquest")
			return "side";
		return "none";
	}
	if (path == "snow") {
		if (race == "sons_of_svanir")
			return "main";
		if (race == "elemental")
			return "side";
		return "none";
	}
	if (path == "solid")
		return "none";
	if (path == "swamp")
		return "none";
	if (path == "thauma")
		return "none";
	if (path == "uncat") {
		if (race == "inquest")
			return "main";
		if (race == "flame_legion")
			return "side";
		return "none";
	}
	if (path == "under") {
		if (race == "dredge")
			return "main";
		if (race == "elemental")
			return "side";
		return "none";
	}
	if (path == "urban")
		return "none";
	if (path == "volc") {
		if (race == "grawl")
			return "main";
		if (race == "demon" || race == "elemental")
			return "side";
		return "none";
	}
	return "none";
}

function getWeaponName(id) {
	var weapons = ['Sword', 'Hammer', 'Longbow', 'Shortbow', 'Axe', 'Dagger', 'Greatsword', 'Mace',
				   'Pistol', 'Polearm', 'Rifle', 'Scepter', 'Staff', 'Focus', 'Torch', 'Warhorn',
				   'Shield', 'SmallBundle', 'LargeBundle', 'Spear', 'HarpoonGun', 'Trident', 'ToyWeapon',
				   'ToyVisual', 'MaxItemWeaponType'
	];
	return weapons[id];
}










