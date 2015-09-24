;
"use strict";

function dateToSQL(date) {
    var arr = date.split('/');
    if (arr.length != 3)
        return "";
    return arr[0] + "-" + arr[1] + "-" + arr[2];
}

function SQLToDate(sql) {
    var arr = sql.split('-');
    if (arr.length != 3)
        return 0;
    return arr[0] + "\/" + arr[1] + "\/" + arr[2];
}

function isDateLessThan(date1, date2) {
    // Transfer to SQL if needed.
    if (date1.indexOf('-') == -1)
        date1 = dateToSQL(date1);
    if (date2.indexOf('-') == -1)
        date2 = dateToSQL(date2);
    date1 = date1.split('-');
    date2 = date2.split('-');
    if (date1[0] < date2[0])
        return 1;
    if (date1[0] == date2[0] && date1[1] < date2[1])
        return 1;
    if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] < date2[2])
        return 1;
    if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
        return 0;
    return -1;
}

function timeRemaing(date) {
	// Transfer to normal if needed.
    if (date.indexOf('-') != -1)
        date = SQLToDate(date);
	date = date.split('\/');
	date = new Date(Date.UTC(date[0], date[1] - 1, date[2]));
	var current = new Date();
	var millis = date.getTime() - current.getTime();
	var seconds = Math.floor(millis / 1000);
    millis = millis - seconds * 1000;
    var minutes = Math.floor(seconds / 60);
    seconds = seconds - 60 * minutes;
    var hours = Math.floor(minutes / 60);
    minutes = minutes - 60 * hours;
	var days = Math.floor(hours / 24);
    hours = hours - 24 * days;
	var weeks = Math.floor(days / 7);
    days = days - 7 * weeks;
	var timeString = "";
	if (weeks > 0)
        return 'in ' + weeks + '&nbsp;w and ' + days +'&nbsp;d';
	if (days > 0)
        return 'in ' + days + '&nbsp;d and ' + hours +'&nbsp;h';
	if (hours > 0)
        return 'in ' + hours + '&nbsp;h and ' + minutes +'&nbsp;m';
    if (minutes > 0)
        return 'in ' + minutes + '&nbsp;m';
	return 'now!';
}


function buildTimeStr(time, topiclink) {
    var seconds = Math.floor(time / 1000);
    var millis = time - seconds * 1000;
    var minutes = Math.floor(seconds / 60);
    seconds = seconds - 60 * minutes;
    var hours = Math.floor(minutes / 60);
    minutes = minutes - 60 * hours;
    var timeString = "";
	if (topiclink && topiclink.length > 0) {
		if (topiclink.indexOf("http") == -1)
			topiclink = 'https://forum-en.guildwars2.com/forum/game/dungeons/' + topiclink + '/first';
		timeString = '<a class="overlay-link" href="' + topiclink + '">';
	}	
    if (hours > 0)
        timeString = timeString + hours +"&nbsp;h";
    if (minutes > 0)
        timeString = timeString + minutes +"&nbsp;m&nbsp;";
    if (seconds > 0)
        timeString = timeString + seconds +"&nbsp;s";
    if (millis > 0)
        timeString = timeString + millis +"&nbsp;ms";
	if (topiclink && topiclink.length > 0)
		timeString += '</a>';
    return timeString;
}

function buildGroupStr(groups) {
    var strs = [];
    $.each(groups, function() {
        var str = "";
        if (this.website && this.website.length > 0)
            str += "<a href=\"" + this.website + "\">";
        str += this.name + " " + this.tag;
        if (this.website && this.website.length > 0)
            str += "</a>";
		// Ignore multiple same guilds.
		if (strs.indexOf(str) == -1)
			strs.push(str);
    });
    return strs.join(", ");
}

function buildCharacterStr(characters, professions) {
    var strs = [];
    $.each(characters, function() {
        var str = "";
		if (this.link && this.link.length > 0) {
            // Check is link a youtube short link.
            if (this.link.indexOf(".") == -1)
                this.link = "http://youtu.be/" + this.link;
            str += '<a href="' + this.link + '" class="overlay-link">';
        }
        str += this.name;  
        // Profession icon.
        // Find the correct profession.
        var professionID = this.profession;
        $.each(professions, function() {
            if (this.ID == professionID)
               str += '&nbsp;<img class="profession-icon" src="media/img/' + this.image + '">';
        });
		if (this.link && this.link.length > 0)
            str += '</a>';
        strs.push(str);
    });
    return strs.join(", ");
}

function buildPathStr(path, paths, instances) {
    var str = '';
    // Find the correct path.
    $.each(paths, function() {
        if (this.ID != path)
            return;
        // Find the correct instance.
        var instance = this.instance;
        $.each(instances, function() {
            if (this.ID != instance)
                return;
            str = this.name.replace(/ /g, '&nbsp;') + '&nbsp;';
        });
        str += this.name.replace(/ /g, '&nbsp;');
    });
    return str;
}

function getHash() {
	var hash = document.location.hash.split("#");
	if (hash.length < 2)
		return "";
	return hash[1];
}

function setHash(filter) {
	var hash = document.location.hash.split("#");
	if (hash.length < 2)
		hash.push(filter);
	else
		hash[1] = filter;
	document.location.hash = hash.join("#");
}

function getSecondaryFilter() {
	var hash = document.location.hash.split("#");
	if (hash.length < 3)
		return "";
	return hash[2];
}

function setSecondaryFilter(filter) {
	var hash = document.location.hash.split("#");
	if (hash.length < 2)
		hash.push("");
	if (hash.length < 3)
		hash.push(filter);
	else
		hash[2] = filter;
	document.location.hash = hash.join("#");
}

	// Filter encoding.
	// Version 1: first value
	// Type: 4
	// Instance: 9
	// Path: 18
	// Profession: 9
	// Category: 6
	// Validity: 2
	// After date: 12 + 32 + 10
	// Before date: 12 + 32 + 10
	// Players : 5
	// columns: 2 + 2 + 2 + 2 + 2
	function storeFilters() {
		var values = [];
		var maxValues = [];
		values.push(document.getElementById("record-instance-type").selectedIndex);
		maxValues.push(4);
		values.push(document.getElementById("record-instance").selectedIndex);
		maxValues.push(9);
		values.push(document.getElementById("record-path").selectedIndex);
		maxValues.push(18);
		values.push(document.getElementById("record-profession").selectedIndex);
		maxValues.push(9);
		values.push(document.getElementById("record-category").selectedIndex);
		maxValues.push(6);
		values.push(document.getElementById("record-validity").selectedIndex);
		maxValues.push(2);
		// Date can be any string so check its validity.
		var date = $("#record-date-pre").val().split('/');
		if (date.length == 3 && date[1] > 0 && date[1] < 13)
			values.push(date[1] - 1)
		else
			values.push(0);
		maxValues.push(12);
		// If day is zero then the value is unset.
		if (date.length == 3 && date[2] > 0 && date[2] < 32)
			values.push(Number(date[2]))
		else
			values.push(0);
		maxValues.push(32);
		if (date.length == 3 && date[0] > 2011 && date[0] < 2022)
			values.push(date[0] - 2012)
		else
			values.push(0);
		maxValues.push(10);
		var date = $("#record-date-after").val().split('/');
		if (date.length == 3 && date[1] > 0 && date[1] < 13)
			values.push(date[1] - 1)
		else
			values.push(0);
		maxValues.push(12);
		// If day is zero then the value is unset.
		if (date.length == 3 && date[2] > 0 && date[2] < 32)
			values.push(Number(date[2]))
		else
			values.push(0);
		maxValues.push(32);
		if (date.length == 3 && date[0] > 2011 && date[0] < 2022)
			values.push(date[0] - 2012)
		else
			values.push(0);
		maxValues.push(10);
		values.push(document.getElementById("record-layout").selectedIndex);
		maxValues.push(4);
		values.push($("#column-time").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-date").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-group").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-characters").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-build").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-validity").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		values.push($("#column-category").hasClass('active') ? 1 : 0);
		maxValues.push(2);
		// Add version number.
		setHash("1" + storeValuesToString(values, maxValues));
	}

	function storeValuesToString(values, maxValues) {
		if (values.length != maxValues.length)
			return "";
		// Uses only numbers (48 - 57) and letters (65 - 90, 97 - 122).
		// These convert to: 0 - 9, 10 - 35, 36 - 61.
		var base = 62;
		// Store values in array for base62 calculations.
		var characters = [];
		characters.push(0);
		// Start encoding.
		for (var i = 0; i < values.length; i++) {
			// Multiply with maximum value to create space for the value.
			// Handle multiplication separately to prevent numeric overflow.
			for (var j = 0; j < characters.length; j++) {
				characters[j] *= maxValues[i];
			}
			// Add the value.
			characters[0] += values[i];
			// Handle value overflows to prevent numeric overflow.
			for (var j = 0; j < characters.length; j++) {
				while (characters[j] >= base) {
					characters[j] -= base;
					if (j + 1 == characters.length)
						characters.push(0);
					characters[j + 1]++;
				}
			}
		}
		// Convert characters to string.
		var string = "";
		for (var i = 0; i < characters.length; i++) {
			var value = characters[i];
			// Convert to real char values.
			if (value < 10)
				value += 48;
			else if (value < 36)
				value += 55;
			else
				value += 61;
			string = String.fromCharCode(value) + string;
		}
		return string;
	}

	function readFilters(string) {
		// Get version to read string properly.
		var version = string.substr(0, 1).charCodeAt(0);
		// Convert to proper values.
		if (version > 90)
			version -= 61;
		else if (version > 57)
			version -= 55;
		else
			version -= 48;
		var string = string.substr(1);
		// Set max values.
		var maxValues = [];
		maxValues.push(4);
		maxValues.push(9);
		maxValues.push(18);
		maxValues.push(9);
		if (version > 0)
			maxValues.push(6);
		else
			maxValues.push(3);
		maxValues.push(2);
		maxValues.push(12);
		maxValues.push(32);
		maxValues.push(10);
		maxValues.push(12);
		maxValues.push(32);
		maxValues.push(10);
		maxValues.push(4);
		maxValues.push(2);
		maxValues.push(2);
		maxValues.push(2);
		maxValues.push(2);
		maxValues.push(2);
		maxValues.push(2);
		maxValues.push(2);
		// Read values.
		var values = readValuesFromString(string, maxValues);
		if (values.length != maxValues.length) {
			console.log("Error while loading filters.");
			return;
		}
		// Set values.
		document.getElementById("record-instance-type").selectedIndex = values[0];
		updateInstanceSelect();
		document.getElementById("record-instance").selectedIndex = values[1];
		updatePathSelect();
		document.getElementById("record-path").selectedIndex = values[2];
		document.getElementById("record-profession").selectedIndex = values[3];
		document.getElementById("record-category").selectedIndex = values[4];
		document.getElementById("record-validity").selectedIndex = values[5];
		// Build dates. If day is zero then the value is empty.
		if (values[7] == 0)
			$("#record-date-pre").val("");
		else {
			$("#record-date-pre").val((values[8] + 2012) + "/" + (values[6] + 1) + "/" + values[7]);
		}
		if (values[10] == 0)
			$("#record-date-after").val("");
		else {
			$("#record-date-after").val((values[11] + 2012) + "/" + (values[9] + 1) + "/" + values[10]);
		}
		document.getElementById("record-layout").selectedIndex = values[12];
		if (values[13] == 1)
			$("#column-time").addClass('active');
		if (values[14] == 1)
			$("#column-date").addClass('active');
		if (values[15] == 1)
			$("#column-group").addClass('active');
		if (values[16] == 1)
			$("#column-characters").addClass('active');
		if (values[17] == 1)
			$("#column-build").addClass('active');
		if (values[18] == 1)
			$("#column-validity").addClass('active');
		if (values[19] == 1)
			$("#column-category").addClass('active');
	}

	
	function readValuesFromString(string, maxValues) {
		// Uses only numbers (48 - 57) and letters (65 - 90, 97 - 122).
		// These convert to: 0 - 9, 10 - 35, 36 - 61.
		var base = 62;
		// Convert string to base62.
		var characters = [];
		for (var i = 0; i < string.length; i++) {
			var value = string.charCodeAt(i);
			// Convert to proper values.
			if (value > 90)
				value -= 61;
			else if (value > 57)
				value -= 55;
			else
				value -= 48;
			characters.unshift(value);
		}
		var values = [];
		// Start decoding. Last values are at top of the "stack" so start from the end.
		for (var i = maxValues.length - 1; i >= 0; i--) {
			// Divide with maximum value to extract one value.
			// Start with end to pass down modulus.
			var modulus = 0;
			for (var j = characters.length - 1; j >= 0; j--) {
				characters[j] += modulus * base;
				// Store result to calculate modulus.
				var newValue = Math.floor(characters[j] / maxValues[i]);
				modulus = characters[j] - newValue * maxValues[i];
				characters[j] = newValue;
			}
			// Remaining modulus contains the value.
			// Insert to back to keep values in order.
			values.unshift(modulus);
			// Underflow handled automatically.
		}
		if (values.length != maxValues.length)
			return [];
		return values;
	}
	