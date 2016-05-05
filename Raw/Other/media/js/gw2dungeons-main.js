;
"use strict";

var cntrlIsPressed = false;

function resizeSidebar() {
	var position = $(".encounter-right").offset();
	if (!position) {
		// Fix for the search page. / 2015-10-12 / Wethospu
		position = $("#main-container").offset();
		if (position)
			position.left += $("#main-container").width();
		else
			return;
	}
	$("#detail-container").css("top", "" + position.top + "px");
	$("#detail-container").css("left", "" + (position.left + 10) + "px");
	$("#detail-container").css("right", "5px");
	$("#detail-container").css("max-height", "" + ($(window).height() - position.top - 5) + "px");
}

function loadCommentBox() {
	$("#comment-section").html("");
	if (getSetting("comments"))
		$("#comment-section").html('<div id="HCB_comment_box"></div><link rel="stylesheet" type="text/css" href="//www.htmlcommentbox.com/static/skins/shady/skin.css" /><script type="text/javascript" id="hcb"> /*<!--*/ if(!window.hcb_user){hcb_user={};} (function(){var s=document.createElement("script"), l=(hcb_user.PAGE || ""+window.location), h="//www.htmlcommentbox.com";s.setAttribute("type","text/javascript");s.setAttribute("src", h+"/jread?page="+encodeURIComponent(l).replace("+","%2B")+"&mod=%241%24wq1rdBcg%24KgqanRakyO80BQlb3L2as1"+"&opts=22&num=50");if (typeof s!="undefined") document.getElementsByTagName("head")[0].appendChild(s);})(); hcb_user.submit="";/*-->*/ </script>');
}

function loadAd(side) {
	// Load ads based on ad setting and margin size. / 2015-07-31 / Wethospu
	var ads = getSetting("adSetting");
	var size = $('#' + side + '-ad').width();
	if (ads == "big" && size >= 300) {
		$('#' + side + '-ad').html('<script async src="//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>\
							<!-- gw2dungeons-main-big -->\
							<ins class="adsbygoogle"\
							style="display:inline-block;width:300px;height:600px;float:' + side + '"\
							data-ad-client="ca-pub-6596049380548180"\
							data-ad-slot="4729718557"></ins>\
							<script>\
							(adsbygoogle = window.adsbygoogle || []).push({});\
							</script>');
	}
	else if ((ads == "small" || ads == "big") && size >= 160) {
		$('#' + side + '-ad').html('<script async src="//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>\
							<!-- gw2dungeons-main -->\
							<ins class="adsbygoogle"\
							style="display:inline-block;width:160px;height:600px;float:' + side + '"\
							data-ad-client="ca-pub-6596049380548180"\
							data-ad-slot="6582248550"></ins>\
							<script>\
							(adsbygoogle = window.adsbygoogle || []).push({});\
							</script>');
	}
	else
		$('#' + side + '-ad').html("");
	// In encounters varying path bar length puts ad in wrong position. / 2015-10-12 / Wethospu
	var position = $(".my-bar").offset();
	if (position)
		$('#' + side + '-ad').css("top", "" + (position.top + $(".my-bar").outerHeight(false)) + "px");
}

function loadThumbs() {
	// Note: This forces page generation which may stall the page for a while. / 2015-08-13 / Wethospu
	var width = $(window).width();
	$('.thumb-image').each(function(){
		// Load thumb based on screen size. / 2015-07-31 / Wethospu
		var imageName = $(this).attr("data-name");
		if (width > 1500) {
			$(this).attr("src", "PATH_THUMB_BIG" + imageName);
			$(this).css("display", "initial");
		}
		else if (width > 1200) {
			$(this).attr("src", "PATH_THUMB_SMALL" + imageName);
			$(this).css("display", "initial");
		}
		else
			$(this).css("display", "none");
	});
}


function initScreenResize() {
	// Run the functions with the current window width. / 2015-07-31 / Wethospu
	loadThumbs();
	loadAd("left");
	loadAd("right");
	resizeSidebar();
	// Window width isn't reliable because javascript handles it differently than css. / 2015-07-31 / Wethospu
	// So just check when css properties change and do stuff then! / 2015-07-31 / Wethospu
	var thumbWidth = $(".encounter-left").css("min-width");
	var leftAdWidth = $("#left-ad").css("width");
	var rightAdWidth = $("#right-ad").css("width");
	
	$( window ).resize(function() {
		if (thumbWidth != $(".encounter-left").css("min-width")) {
			loadThumbs();
			thumbWidth = $(".encounter-left").css("min-width");
		}
		if (leftAdWidth != $("#left-ad").css("width")) {
			loadAd("left");
			leftAdWidth = $("#left-ad").css("width");
		}
		if (rightAdWidth != $("#right-ad").css("width")) {
			loadAd("right");
			rightAdWidth = $("#right-ad").css("width");
		}
		// Scale overlay if on. / 2015-07-31 / Wethospu
		if ($("#data-overlay").hasClass("in")) {
			scaleElement("#overlay-pane");
			height = $('#overlay-pane')[0].getBoundingClientRect().height;
			$(".modal-dialog").css('height', height + 42 + 'px');
			$(".modal-content").css('height', height + 42 + 'px');
		}
		resizeSidebar();
	});
}

function loadRecordRun() {
	$('.record-run').each(function(){
		var recordObj = this;
		var path = $(recordObj).html();
		$(recordObj).html("");
		if (path == null || path.length == 0)
			return;
		var request = $.ajax({
			url: "records.php",
			type: "post",
			data: "type=readProfessions"
		});
		
		var professions;
		request.done(function (data){
			data = JSON.parse(data);
			if (data.success)
			{
				professions = data.professions;
				request = $.ajax({
					url: "records.php",
					type: "post",
					data: "type=records&validity=1&category=1&paths=" + path
				});
				request.done(function (data){
				data = JSON.parse(data);
				if (data.success)
				{
					if (data.records[path] == null)
						$(recordObj).html("None");
					else
						$(recordObj).html(buildTimeStr(data.records[path].time) + ' by ' + buildGroupStr(data.records[path].groups) + ' (' + buildCharacterStr(data.records[path].characters, professions) + ')');
					handleOverlayLinks();
				}	
				else
					console.log(data.message);	
				});
				request.fail(function (data){
					data = JSON.parse(data);
					console.log(data.message);
				});
			}
			else
				console.log(data.message);
		});
		request.fail(function (data){
			data = JSON.parse(data);
			console.log(data.message);
		});
	});
}

function loadPage() {
	// Remove no-js since js obviously works. / 2015-07-31 / Wethospu
    $('html').removeClass('no-js');
	// Activate enemy links. / 2015-07-31 / Wethospu
	$('#detail-container, #main-container').on("click", "span.enemy-button", openEnemyOverlay);
	$('#data-overlay').on("click", "span.enemy-button", openEnemyOverlayForce);
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.level-minus", function() {
		levelMinus(this, 'level', 0);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.level-plus", function() {
		levelPlus(this, 'level', 100);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.target-level-minus", function() {
		levelMinus(this, 'target-level', 0);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.target-level-plus", function() {
		levelPlus(this, 'target-level', 80);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.fractal-level-minus", function() {
		// Reset enemy level to let it scale normally. / 2015-09-30 / Wethospu
		$($(this).parents('.enemy')[0]).attr("data-level", '');
		var newScale = levelMinus(this, 'fractal-level', 0);
		saveSetting("currentScale", newScale);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.fractal-level-plus", function() {
		$($(this).parents('.enemy')[0]).attr("data-level", '');
		var newScale = levelPlus(this, 'fractal-level', 100);
		saveSetting("currentScale", newScale);
	});
	$('#detail-container, #main-container, #data-overlay').on( "click", "span.path-button", function() {
		var enemy = $($(this).parents('.enemy')[0]);
		$(enemy).attr("data-level", '');
		$(enemy).attr("data-target-level", '');
		$(enemy).attr("data-current-path", $(this).html().toLowerCase());
		handleEnemy(enemy, $(enemy).parent().attr('id') == "detail-container" ? "side" : "over");
	});
	
	$('#data-overlay').on("click", "span.overlay-close", function() {
		$('#data-overlay').modal('hide');
	});
	
	
	$(document).keydown(function(event){
		if (event.which == "17")
			cntrlIsPressed = true;
	});

	$(document).keyup(function(){
		if (event.which == "17")
			cntrlIsPressed = false;
	});
	$('#data-overlay').on('hide.bs.modal', function () {
		$('.modal-content').html('');
	});
	gfyCollection.init();
	
	handleOverlayLinks();
	loadRecordRun();
	initScreenResize();
}

function levelMinus(element, target, minLevel) {
	var enemy = $(element).parents('.enemy')[0];
	var level = Number($(enemy).attr("data-" + target));
	if (cntrlIsPressed)
		level -= 10;
	else
		level -= 1;
	if (level < minLevel)
		level = minLevel;
	$(enemy).attr("data-" + target, level);
	handleEnemy(enemy, $(enemy).parent().attr('id') == "detail-container" ? "side" : "over");
	return level;
}

function levelPlus(element, target, maxLevel) {
	var enemy = $(element).parents('.enemy')[0];
	var level = Number($(enemy).attr("data-" + target));
	if (cntrlIsPressed)
		level += 10;
	else
		level += 1;
	if (level > maxLevel)
		level = maxLevel;
	$(enemy).attr("data-" + target, level);
	handleEnemy(enemy, $(enemy).parent().attr('id') == "detail-container" ? "side" : "over");
	return level;
}


var overlayHtml = '<span class="glyphicon glyphicon-remove overlay-close clickable"></span><ul id="overlay-nav" class="nav nav-tabs"></ul><div id="overlay-pane" class="tab-content"></div>';

function handleOverlayLinks() {
	// Load links on overlay unless ctrl is clicked. / 2015-07-31 / Wethospu
	$(".overlay-link").on('click',function(event){
		// For some reason 2-3 random links "fire up" on page load. / 2015-07-31 / Wethospu
		if (!event.originalEvent)
			return true;
		if (cntrlIsPressed)
			return true;
		// Check is the url already loaded. / 2015-09-16 / Wethospu
		var url = simplifyUrl(this.href);
		overlayRemoveOld(url);
		// Create a new tab. / 2015-07-31 / Wethospu
		var content = '<div class="embed-responsive embed-responsive-16by9" style="';
		var width = $(this).attr("data-width");
		if (width > 0)
			content += ' width:' + width + 'px;';
		var height = $(this).attr("data-height");
		if (height > 0)
			content += ' height:' + height + 'px;';
		content += '">';
		if (this.href.indexOf("gfycat") > -1) {
			// Special gfycat embed solution (normal embed causes extra margins).
			// NOTE: Solution adds 'position: relative;'. This was manually removed from gfycat.min to make this work.
			content += '<div class="gfyitem" data-id="' + url + '"></div>';
		}
		else if (this.href.indexOf(".jpg") > -1 || this.href.indexOf(".png") > -1) {
			// Direct image handling to prevet SAMEORIGIN errors.
			content += '<img src="' + handleUrl(this.href) + '"';
			if (width > 0)
				content += ' width="' + width + '"';
			if (height > 0)
				content += ' height="' + height + '"';
			content += '></img>';
		}
		else if (this.href.indexOf("youtu") > -1)
		{
			// Youtube suggest iframes for their videos (https://developers.google.com/youtube/player_parameters).
			content += '<iframe class="embed-responsive-item" src="' + handleUrl(this.href) + '"';
			if (width > 0)
				content += ' width="' + width + '"';
			if (height > 0)
				content += ' height="' + height + '"';
			content += ' frameborder="0" margin="0" scrolling="no" style="-webkit-backface-visibility: hidden;-webkit-transform: scale(1);"></iframe>';
		}
		else
		{
			// Other links (like gw2 wiki pages) have to be opened normally because of SAMEORIGIN policy.
			return true;
		}
		content += '</div>';
		if (!$("#data-overlay").hasClass("in")) {
			$('#data-overlay').modal();
			$('#data-overlay').css('padding-right', '');
			$(".modal-content").html(overlayHtml);
		}	
		$("#overlay-nav").append('<li><a href="#tab-' + url + '" data-toggle="tab" data-description="' + url + '" data-width="' + width + '" data-height="' + height + '">' + shortenUrl(this.href) + '</a></li>');
		$("#overlay-pane").append('<div class="tab-pane" id="tab-' + url + '">' + content + '</div>');
		
		setOverlaySize(width, height);
		// Activate the new tab. / 2015-09-16 / Wethospu
		$($('#overlay-nav a[data-toggle="tab"]')).each(function () {
			if (url == $(this).attr("data-description"))
				$(this).tab('show');		
		});
		overlayHtml = $(".modal-content").html();
		$(".modal-backdrop").height($(document).height());
		$('#overlay-nav a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
			setOverlaySize($(e.target).attr("data-width"), $(e.target).attr("data-height"));
		});
		// Generat gfycat objects. / 2015-10-19 / Wethospu
		if (this.href.indexOf("gfycat") > -1)
			gfyCollection.scan();
		// Prevent default behavior. / 2015-07-31 / Wethospu
		return false;
	});
	// Remove class to prevent further binding. / 2015-07-31 / Wethospu
	$(".overlay-link").removeClass("overlay-link");
}

function setOverlaySize(width, height)
{
	width = Number(width);
	height = Number(height);
	if (height > 0)
	{
		$("#overlay-pane").width(width + 'px');
		$("#overlay-pane").height(height + 'px');
	}
	else
	{
		$("#overlay-pane").width('');
		$("#overlay-pane").height('');
	}
	$(".modal-dialog").css('max-width', width + 2 + 'px');
	$(".modal-content").css('max-width', width + 2 + 'px');
	scaleElement("#overlay-pane");
	if (height > 0)
	{
		height = $('#overlay-pane')[0].getBoundingClientRect().height;
		$(".modal-dialog").height(height + 42 + 'px');
		$(".modal-content").height(height + 42 + 'px');
	}
	else
	{
		$(".modal-dialog").height('');
		$(".modal-content").height('');
	}
}

function scaleElement(element) {
	var width = $(element).width();
	var parentWidth = $(element).parent().width()
	var scale = parentWidth/width;
	var origin = 0;
	if (scale >= 1) {
		scale = 1;
		origin = 0;
	}	
	$(element).css("-webkit-transform", "matrix(" + scale + ", 0, 0, " + scale + ", " + origin + ", 0)");
	$(element).css("-webkit-transform-origin", "0 0");
	$(element).css("-moz-transform", "matrix(" + scale + ", 0, 0, " + scale + ", " + origin + ", 0)");
	$(element).css("-moz-transform-origin", "0 0");
	$(element).css("-o-transform", "matrix(" + scale + ", 0, 0, " + scale + ", " + origin + ", 0)");
	$(element).css("-o-transform-origin", "0 0");		
}

function generateSubNavigation(names, links) {
	var content = '<ul class="nav navbar-nav">';
	for (var i = 0; i < names.length; i++)
		content += '<li><a href="./' + links[i] + '">' + names[i] + '</a></li>';
    content += '</ul>';
	$("#sub-navigation").html(content);
}

function dungeonToPaths(dungeon) {
	if (dungeon == "Ascalonian Catacombs" || dungeon == "Caudecus's Manor" || dungeon == "Sorrows Embrace" || dungeon == "Citadel of Flame"
         || dungeon == "Crucible of Eternity" || dungeon == "Honor of the Waves")
	     return ["Story", "Path 1", "Path 2", "Path 3"];
	else if (dungeon == "Twilight Arbor")
	    return ["Story", "Path Forward", "Path Assault", "Path Up"];
	else if (dungeon == "The Ruined City of Arah")
	    return ["Story", "Path 1", "Path 2", "Path 3", "Path 4"];
	else if (dungeon == "Fractals of the Mists")
	    return ["Instabilities", "Aetherblade", "Aquatic Ruins", "Cliffside", "Molten Furnace", "Captain Mai Trin", "Molten Boss", "Snowblind", "Solid Ocean", "Swampland", "Thaumanova Reactor", 
	    "Uncategorized", "Underground Facility", "Urban Battleground", "Volcanic"];
	return new Array();
}

function dungeonToPages(dungeon) {
    if (dungeon == "Ascalonian Catacombs")
        return ["ACS", "AC1", "AC2", "AC3"];
    else if (dungeon == "Caudecus's Manor")
        return ["CMS", "CM1", "CM2", "CM3"];
    else if (dungeon == "Twilight Arbor")
        return ["TAS", "TAF", "TAAE", "TAU"];
    else if (dungeon == "Sorrows Embrace")
        return ["SES", "SE1", "SE2", "SE3"];
    else if (dungeon == "Citadel of Flame")
        return ["COFS", "COF1", "COF2", "COF3"];
    else if (dungeon == "Honor of the Wavess")
        return ["HOTWS", "HOTW1", "HOTW2", "HOTW3"];
    else if (dungeon == "Crucible of Eternity")
        return ["COES", "COE1", "COE2", "COE3"];
    else if (dungeon == "The Ruined City of Arah")
        return ["ARAHS", "ARAH1", "ARAH2", "ARAH3", "ARAH4"];
    else if (dungeon == "Fractals of the Mists")
        return ["INSTA", "AETHER", "AQUA", "CLIFF", "FURN", "MAI", "MOLTEN", "SNOW", "SOLID", "SWAMP", "THAUMA",
	    "UNCAT", "UNDER", "URBAN", "VOLC"];
    return new Array();
}

// Finds a clicked enemy and loads details about it.
function openEnemyOverlay() {
    // Determine where to open the enemy. / 2015-10-12 / Wethospu
	var useOverlay = $(this).attr("data-force");
	if (useOverlay)
		useOverlay = useOverlay > 0 ? true : false;
	if (!useOverlay) {
		useOverlay = !getSetting("useSidebar");
		if (cntrlIsPressed)
			useOverlay = !useOverlay;
	}
	openEnemyOverlaySub(this, useOverlay);
}

function openEnemyOverlayForce() {
	openEnemyOverlaySub(this, true);
}

function openEnemyOverlaySub(enemy, useOverlay) {
    var enemyNumbers = String($(enemy).attr("data-index")).split(":");
	var enemyLevels = String($(enemy).attr("data-level")).split(":");
	var enemyScale = getSetting("currentScale");
	if (useOverlay) {
		if (!$("#data-overlay").hasClass("in")) {
			$('#data-overlay').modal();
			$('#data-overlay').css('padding-right', '');
			$(".modal-content").html(overlayHtml);
		}	
	}
	else {
		$('#detail-container').html('');
		if (!$('#detail-container').is(":visible")) {
			$('#detail-container').show();
			$('#right-ad').hide();
		}
	}	
	var path = $(enemy).attr("data-path");
	openEnemySub(0, enemyNumbers, enemyLevels, enemyScale, path, useOverlay);
}

// Finds one enemy (index).
// Enemy files are split to pieces to make loading faster.
function openEnemySub(index, enemyNumbers, enemyLevels, enemyScale, path, useOverlay) {
	if (index >= enemyNumbers.length) {
		applyEnemySettings(useOverlay ? "over" : "side");
		handleOverlayLinks();
		loadThumbs();
		if (useOverlay) {
			overlayHtml = $(".modal-content").html();
			$(".modal-backdrop").height($(document).height());
			$('#overlay-nav a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
				setOverlaySize($(e.target).attr("data-width"), $(e.target).attr("data-height"));
			});
		}
		return;
	}	
	var numberToFind = enemyNumbers[index];
	var fileNumber = Math.floor(numberToFind / 100);
	numberToFind -= fileNumber * 100;
	var enemyDiv = $("<div />");
	$(enemyDiv).load('enemies/enemies' + fileNumber + '.htm', function () {	
		var counter = -1;
		$(enemyDiv).find(".enemy").each(function () {
			counter++;
			if (numberToFind != counter)
				return;
			if (useOverlay) {
				overlayRemoveOld(counter);
				// Create a new tab. / 2015-07-31 / Wethospu
				var content = $(this)[0].outerHTML;
				var name =  $(content).find(".enemy-name").html();
				$("#overlay-nav").append('<li><a class="set-event" href="#tab-' + counter + '" data-toggle="tab" data-description="' + counter + '" data-width="1250" data-height="0">' + name + '</a></li>');
				$("#overlay-pane").append('<div class="tab-pane" id="tab-' + counter + '">' + content + '</div>');
				var enemy = $("#overlay-pane #tab-" + counter + " .enemy");
				// Set enemy level dynamically based on enemy link. / 2015-09-28 / Wethospu
				if (enemyLevels[index] > 0)
					$(enemy).attr("data-level", enemyLevels[index]);
				if (enemyScale && enemyScale > 0)
					$(enemy).attr("data-fractal-level", enemyScale);
				// Set the enemy path to have a correct base level. / 2015-10-08 / Wethospu
				$(enemy).attr("data-current-path", path);
				// Activate the new tab. / 2015-09-16 / Wethospu
				$('#overlay-nav a[data-toggle="tab"]').each(function () {
					if (counter == $(this).attr("data-description"))
						$(this).tab('show');		
				});
				setOverlaySize(1250, 0);
			}
			else {
				// Add an overlay link for more info. / 2015-10-12 / Wethospu
				var name = $(this).find(".enemy-name").html();
				$(this).find(".enemy-name").html('<span class="enemy-button" data-index="' + enemyNumbers[index] + '" data-force="1">' + name + '</span>');
				// Prevent a clash with overlay tactics. / 2015-10-12 / Wethospu
				$(this).find(".tactics a").each(function() {
					$(this).attr('href', $(this).attr('href') + 'a');
				});
				$(this).find(".tactics .tab-pane").each(function() {
					$(this).attr('id', $(this).attr('id') + 'a');
				});
				$(this).find(".tips a").each(function() {
					$(this).attr('href', $(this).attr('href') + 'a');
				});
				$(this).find(".tips .tab-pane").each(function() {
					$(this).attr('id', $(this).attr('id') + 'a');
				});
				$('#detail-container').append(this);
			}
			openEnemySub(index + 1, enemyNumbers, enemyLevels, enemyScale, path, useOverlay);
		});   
    }).error(function (jqXHR, textStatus, errorThrown) {
        console.log("error " + textStatus);
        console.log("incoming Text " + jqXHR.responseText);
    });
}

function overlayRemoveOld(id) {
	// Count tabs to remove extra. / 2015-09-21 / Wethospu
	var count = 0;
	// Remove any existing tabs so they get moved to the end. / 2015-09-21 / Wethospu
	$('#overlay-nav a[data-toggle="tab"]').each(function () {
		if (id == $(this).attr("data-description"))
		{
			// Remove the content. / 2015-09-16 / Wethospu
			$('#overlay-nav #tab-' + id).replaceWith("");
			$(this).replaceWith("");
		}
		else
			count++;
	});
	// -1 because a new one will get added. / 2015-09-21 / Wethospu
	var maxTabs = getSetting("tabAmount") - 1;
	if (count <= maxTabs)
		return;
	// Remove extra tabs from the beginning. / 2015-09-21 / Wethospu
	$('#overlay-pane a[data-toggle="tab"]').each(function () {
		if (count > maxTabs)
		{
			$('#overlay-pane #tab-' + $(this).attr("data-description")).replaceWith("");
			$(this).replaceWith("");
			count--;
		}
	});
}

function handleUrl(url) {
	// Get the video ID and remove any extra details from the url.
	if (url.indexOf("https://www.youtube.com/watch?v=") > -1)
		url = "http://www.youtube.com/embed/" + url.replace(/.+?v=([^&]+).*/, "$1");
	if (url.indexOf("youtu.be") > -1)
		url = "http://www.youtube.com/embed/" + url.replace(/.+\/([^&]+).*/, "$1");
	return url;
}

function simplifyUrl(url) {
	var urlSplit = url.split("/");
	var url = urlSplit[urlSplit.length - 1].split(".")[0].replace(/%22/g, '').replace(/\(|\)/g, '').replace(/!/g, '').replace(/:/g, '');
	return url;
}

function shortenUrl(url) {
	var urlSplit = url.split("/");
	var url = urlSplit[urlSplit.length - 1].split(".")[0].replace(/%22/g, '"');
	return url;
}

function addPlayButton(content) {
	// Find link url.
	var urlIndex = content.indexOf("href") + 5;
	if (content[urlIndex] == '"')
		urlIndex++;
	// Find end.
	var urlEnd = content.indexOf('"', urlIndex);
	var url = handleUrl(content.substr(urlIndex, urlEnd - urlIndex));
	return content + '<button class="icon-button" data-url="' + url + '" data-toggle="modal" data-target="#data-overlay"><span class="glyphicon glyphicon-play-circle"></span></button>';
}