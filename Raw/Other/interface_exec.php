<?php
	// By default, use Watcher info so details don't have to be revealed in client side js files.
    $username = 'Watcher';
	if (!empty($_POST['username']))
		$username = $_POST['username'];
	$password = 'FFDgTJ_(omf[';
	if (!empty($_POST['password']))
		$password = $_POST['password'];
	$database = 'RecordDB';
	if (isset($_POST['database']))
		$database = $_POST['database'];

	$type = $_POST['type'];
$servername = "localhost";
$message = '';
$groups = array();
$players = array();
$characters = array();
$professions = array();
$gameBuilds = array();
$records = array();
$instances = array();
$paths = array();
$rulesets = array();
$categories = array();
$instanceTypes = array();
$success = true;

    try {
        $conn = new PDO('mysql:host='.$servername.';dbname='.$database, $username, $password);
        // set the PDO error mode to exception
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        if ($type == "login") {
            $message = "Login successful";
        }
		else if ($type == "readData") {
			$groups = readGroups($conn);
            $players = readPlayers($conn);
			$characters = readCharacters($conn);
			$records = readRecords($conn);
			$professions = readProfessions($conn);
			$gameBuilds = readGameBuilds($conn);
			$instances = readInstances($conn);
			$paths = readPaths($conn);
			$categories = readCategories($conn);
            $instanceTypes = readInstanceTypes($conn);
            $message = "Login successful";
		}
        else if ($type == "readRulesets") {
            $rulesets = readRulesets($conn);
            $message = "Rules read.";
        }
        else if ($type == "addGroup") {
            $message = addGroup($conn);
            $groups = readGroups($conn);
        }
        else if ($type == "editGroup") {
            $message = editGroup($conn);
            $groups = readGroups($conn);
            $players = readPlayers($conn);
			$records = readRecords($conn);
        }
        else if ($type == "deleteGroup") {
            $message = deleteGroup($conn);
            $groups = readGroups($conn);
        }
        else if ($type == "addPlayer") {
            $message = addPlayer($conn);
            $players = readPlayers($conn);
        }
        else if ($type == "editPlayer") {
            $message = editPlayer($conn);
            $players = readPlayers($conn);
			$characters = readCharacters($conn);
        }
        else if ($type == "deletePlayer") {
            $message = deletePlayer($conn);
            $players = readPlayers($conn);
        }
		else if ($type == "addCharacter") {
            $message = addCharacter($conn);
            $characters = readCharacters($conn);
        }
        else if ($type == "editCharacter") {
            $message = editCharacter($conn);
            $characters = readCharacters($conn);
			$records = readRecords($conn);
        }
        else if ($type == "deleteCharacter") {
            $message = deleteCharacter($conn);
            $characters = readCharacters($conn);
        }
		else if ($type == "addRecord") {
            $message = addRecord($conn);
            $records = readRecords($conn);
        }
        else if ($type == "editRecord") {
            $message = editRecord($conn);
            $records = readRecords($conn);
        }
        else if ($type == "editCharactersRecord") {
            $message = editCharactersRecord($conn);
            $records = readRecords($conn);
        }
        else if ($type == "deleteRecord") {
            $message = deleteRecord($conn);
            $records = readRecords($conn);
        }
		else if ($type == "addGameBuild") {
            $message = addGameBuild($conn);
            $gameBuilds = readGameBuilds($conn);
        }
        else if ($type == "editGameBuild") {
            $message = editGameBuild($conn);
            $gameBuilds = readGameBuilds($conn);
        }
        else if ($type == "deleteGameBuild") {
            $message = deleteGameBuild($conn);
            $gameBuilds = readGameBuilds($conn);
        }
		else {
			$message = 'Site error. Action '.$type.' not recognized.';
			$success = false;
		}
			
        $conn = null;

    }
    catch(PDOException $e)
    {
        $success = false;
		if ($e->getCode() == 1044)
            $message = 'Incorrect password.';
        else if ($e->getCode() == 1045)
            $message = 'Failed to login.';
        else if ($e->getCode() == 2002)
            $message = 'Unable to reach the database.';
		else if ($e->getCode() == 23000) {
			if ($type == "deleteGroup")
				$message = 'Can\'t delete a group with players.';
			else if ($type == "deletePlayer")
				$message = 'Can\'t delete a player with characters.';
			else if ($type == "deleteCharacter")
				$message = 'Can\'t delete a character with records.';
			else
				$message = 'Error ('.$e->getCode().'): '.$e->getMessage();
		}
        else if ($e->getCode() == 42000)
            $message = 'Permission denied.';
        else
            $message = 'Error ('.$e->getCode().'): '.$e->getMessage();
    }

echo json_encode(array("message" => $message, "success" => $success, "groups" => $groups, "rulesets" => $rulesets,
					"players" => $players, "characters" => $characters, "professions" => $professions, "categories" => $categories,
					"records" => $records, "gameBuilds" => $gameBuilds, "instances" => $instances, "paths" => $paths,
					"instanceTypes" => $instanceTypes));
	exit();

function readGroups(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, tag, website FROM Groups ORDER BY name');
    $sql->execute();
    return $sql->fetchAll();
}

function readPlayers(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, account FROM Players ORDER BY name');
    $sql->execute();
    $players = $sql->fetchAll();
    // Get group ids.
    $sql = $conn->prepare('SELECT groupID FROM PlayerGroups WHERE player=? LIMIT 5;');
    // Get group name.
    $sql2 = $conn->prepare('SELECT name, tag FROM Groups WHERE ID=? LIMIT 1;');
	// Characters.
	$sql3 = $conn->prepare('SELECT name FROM Characters WHERE player=?;');
    for($i = 0; $i < count($players); $i++) {
        $sql->execute(array($players[$i]['ID']));
        $groups = $sql->fetchAll();
        for($j = 0; $j < count($groups); $j++) {
            $sql2->execute(array($groups[$j]['groupID']));
            $group = $sql2->fetch();
            $groups[$j] = $group['name'].' '.$group['tag'];

        }
        $players[$i]['groups'] = $groups;
		$sql3->execute(array($players[$i]['ID']));
		$characters = $sql3->fetchAll();
        for($j = 0; $j < count($characters); $j++) {
            $characters[$j] = $characters[$j]['name'];

        }
		$players[$i]['characters'] = $characters;
    }
    return $players;
}

function readCharacters(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, player, profession FROM Characters ORDER BY name');
    $sql->execute();
    $characters = $sql->fetchAll();
    // Get player and profession name.
    $sql = $conn->prepare('SELECT name, account FROM Players WHERE ID=? LIMIT 1;');
    $sql2 = $conn->prepare('SELECT name FROM Professions WHERE ID=? LIMIT 1;');
    for($i = 0; $i < count($characters); $i++) {
        $characters[$i]['playerID'] = $characters[$i]['player'];
        $sql->execute(array($characters[$i]['player']));
		$player = $sql->fetch();
        $characters[$i]['player'] = $player['name'];
		$characters[$i]['account'] = $player['account'];
        $sql2->execute(array($characters[$i]['profession']));
        $characters[$i]['profession'] = $sql2->fetch()['name'];
    }
    return $characters;
}

function readRecords(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, time, date, path, validity, category, topiclink FROM Records ORDER BY path, time');
    $sql->execute();
    $records = $sql->fetchAll();
	// Get dungeon name and path.
    $sql = $conn->prepare('SELECT instance, name FROM Paths WHERE ID=? LIMIT 1;');
    $sql2 = $conn->prepare('SELECT name FROM Instances WHERE ID=? LIMIT 1;');
	for($i = 0; $i < count($records); $i++) {
        $sql->execute(array($records[$i]['path']));
        $instance = $sql->fetch();
        $instanceID = $instance['instance'];
        $records[$i]['path'] = $instance['name'];
        $sql2->execute(array($instanceID));
        $records[$i]['instance'] = $sql2->fetch()['name'];
	}
	// Get character names.
    $sql = $conn->prepare('SELECT link, characterID FROM Videos WHERE record=?;');
    $sql2 = $conn->prepare('SELECT name FROM Characters WHERE ID=? LIMIT 1;');
    // Get groups.
    $sql3 = $conn->prepare('SELECT groupID FROM RecordGroups WHERE record=?;');
    $sql4 = $conn->prepare('SELECT name, tag, website FROM Groups WHERE ID=? LIMIT 1;');
    for($i = 0; $i < count($records); $i++) {
        $sql->execute(array($records[$i]['ID']));
        $j = 1;
        foreach ($sql->fetchAll() as $row) {
            $records[$i]['character'.$j] = $row['characterID'];
            $records[$i]['video'.$j] = $row['link'];
            $j++;
        }
        // Transform character IDs to names.
        for($k = 1; $k < $j; $k++) {
            $sql2->execute(array($records[$i]['character'.$k]));
            $records[$i]['character'.$k] = $sql2->fetch()['name'];
        }
        // Initialize remaining players with empty stuff.
        // TODO: Store characters as an array to allow any amount of them.
        for(; $k < 6; $k++) {
            $records[$i]['character'.$k] = '';
            $records[$i]['video'.$k] = '';
        }
        $sql3->execute(array($records[$i]['ID']));
        $records[$i]['groups'] = $sql3->fetchAll();
        // Transform group id to group data.
        for ($k = 0; $k < count($records[$i]['groups']); $k++) {
            $sql4->execute(array($records[$i]['groups'][$k]['groupID']));
            $records[$i]['groups'][$k] = $sql4->fetch();
        }
    }
    return $records;
}

function readProfessions(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, image FROM Professions ORDER BY name');
    $sql->execute();
    return $sql->fetchAll();
}

function readGameBuilds(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, date, build FROM GameBuilds ORDER BY build DESC');
    $sql->execute();
    return $sql->fetchAll();
}

function readRulesets(PDO $conn) {
    $sql = $conn->prepare('SELECT date, ruleset FROM Rules ORDER BY ruleset DESC');
    $sql->execute();
    return $sql->fetchAll();
}

function readInstances(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, image, type FROM Instances ORDER BY ordering');
    $sql->execute();
    return $sql->fetchAll();
}

function readInstanceTypes(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name FROM InstanceTypes ORDER BY ordering');
    $sql->execute();
    return $sql->fetchAll();
}

function readPaths(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name, validity, instance FROM Paths ORDER BY ordering');
    $sql->execute();
    return $sql->fetchAll();
}

function readCategories(PDO $conn) {
    $sql = $conn->prepare('SELECT ID, name FROM Categories ORDER BY ID');
    $sql->execute();
    return $sql->fetchAll();
}

function addGroup(PDO $conn) {
    $sql = $conn->prepare('INSERT INTO Groups (name, tag, website) VALUES (?, ?, ?);');
    $sql->execute(array($_POST['name'], $_POST['tag'], $_POST['website']));
    return 'Added group '.$_POST['name'].'.';
}

function editGroup(PDO $conn) {
    // Prevent stuff like empty id or *.
    if (!is_numeric($_POST['id']))
        return 'ID is not a number.';
    // Check which values should be changed.
    $data = array('name', 'tag', 'website');
    $temp_array = array();
    $var_array = array();
    foreach ($data as $row) {
        if (strlen($_POST[$row]) > 0) {
            $temp_array[] = $row.'=?';
            $var_array[] = $_POST[$row];
        }
    }
    $var_array[] = $_POST['id'];
    if (count($temp_array) == 0)
        return 'All values are empty. Nothing to edit.';
    $sql = $conn->prepare('UPDATE Groups SET'.' '.implode(', ', $temp_array).' WHERE ID=?;');
    if (!$sql->execute($var_array))
        return "Error??";
    return 'Edited '.count($temp_array).' values for group '.$_POST['id'].'.';
}

function deleteGroup(PDO $conn) {
	if (!is_numeric($_POST['id']))
        return 'group ID is not a number.';
    $sql = $conn->prepare('DELETE FROM Groups WHERE ID=?;');
    $sql->execute(array($_POST['id']));
    return 'Deleted group '.$_POST['id'].'.';
}

function addPlayer(PDO $conn) {
    $sql = $conn->prepare('INSERT INTO Players (name, account) VALUES (?, ?);');
    $sql->execute(array($_POST['name'], $_POST['account']));
    // Add groups.
    // Get record id.
    $player = $conn->lastInsertId();
    if ($player == 0 || !is_numeric($player))
        return 'Failed to get proper record ID. Please delete the added record.';
    $groups = explode('|', $_POST['groups']);
    $sql = $conn->prepare('INSERT INTO PlayerGroups (groupID, player) VALUES (?, ?);');
    for($i = 0; $i < count($groups); $i++) {
		if ($groups[$i] == 0 || $groups[$i] == '')
            continue;
        if (!is_numeric($groups[$i]))
            return 'Group ID is not a number. Please delete the added player.';
        $sql->execute(array($groups[$i], $player));
    }
    return 'Added player '.$_POST['name'].' to guilds '.$_POST['groups'].'.';
}

function editPlayer(PDO $conn) {
    // Prevent stuff like empty id or *.
    if (!is_numeric( $_POST['id']))
        return 'Player ID is not a number.';
    // Check which values should be changed.
    $data = array('name', 'account');
    $temp_array = array();
    $var_array = array();
    foreach ($data as $row) {
        if (strlen($_POST[$row]) > 0) {
            $temp_array[] = $row.'=?';
			$var_array[] = $_POST[$row];
        }
    }
    // Don't update nothing.
    if (count($temp_array) > 0) {
        $var_array[] = $_POST['id'];
        $sql = $conn->prepare('UPDATE Players SET' . ' ' . implode(', ', $temp_array) . ' WHERE ID=?;');
        $sql->execute($var_array);
    }
    //// Get group info.
    $groups = explode('|', $_POST['groups']);
    //// Update group entries.
    // Get entries.
    $sql = $conn->prepare('SELECT ID FROM PlayerGroups WHERE player=?;');
    $sql->execute(array($_POST['id']));
    $entries = $sql->fetchAll();
    // Just update everything. No guarantee that things are in order.
    $i = 0;
    $j = 0;
    // Update current entries.
    $sql = $conn->prepare('UPDATE PlayerGroups SET groupID=? WHERE ID=?;');
    while($i < count($groups) && $j < count($entries)) {
        if ($groups[$i] == 0 || $groups[$i] == '') {
            $i++;
            continue;
        }
        $sql->execute(array($groups[$i], $entries[$j]['ID']));
        $i++;
        $j++;
    }
    // Add missing entries.
    $sql = $conn->prepare('INSERT INTO PlayerGroups (player, groupID) VALUES (?, ?);');
    for (;$i < count($groups); $i++) {
        if ($groups[$i] == 0)
            continue;
        $sql->execute(array($_POST['id'], $groups[$i]));
    }
    // Delete extra entries.
    $sql = $conn->prepare('DELETE FROM PlayerGroups WHERE ID=?;');
    for (;$j < count($entries); $j++) {
        $sql->execute(array($entries[$j]['ID']));
    }

    return 'Edited '.count($temp_array).' values for player '.$_POST['id'].'.';
}

function deletePlayer(PDO $conn) {
	if (!is_numeric($_POST['id']))
        return 'Player ID is not a number.';
    $sql = $conn->prepare('DELETE FROM Players WHERE ID=?;');
    $sql->execute(array($_POST['id']));
    return 'Deleted player '.$_POST['id'].'.';
}

function addCharacter(PDO $conn) {
    if (!is_numeric($_POST['player']))
        return 'Player ID is not a number.';
    if (!is_numeric($_POST['profession']))
        return 'Profession ID is not a number.';
    $sql = $conn->prepare('INSERT INTO Characters (name, player, profession) VALUES (?, ?, ?);');
    $sql->execute(array($_POST['name'], $_POST['player'], $_POST['profession']));
    return 'Added character '.$_POST['name'].'.';
}

function editCharacter(PDO $conn) {
    // Prevent stuff like empty id or *.
    if (!is_numeric( $_POST['id']))
        return 'Character ID is not a number.';
    if (!is_numeric( $_POST['player']))
        return 'Player ID is not a number.';
    if (!is_numeric( $_POST['profession']))
        return 'Profession ID is not a number.';
    // Check which values should be changed.
    $data = array('name', 'player', 'profession');
    $temp_array = array();
    $var_array = array();
    foreach ($data as $row) {
        if (strlen($_POST[$row]) > 0) {
            $temp_array[] = $row.'=?';
            $var_array[] = $_POST[$row];
        }
    }
    $var_array[] = $_POST['id'];
    if (count($temp_array) == 0)
        return 'All values are empty. Nothing to edit.';
    $sql = $conn->prepare('UPDATE Characters SET'.' '.implode(', ', $temp_array).' WHERE ID=?;');
    if (!$sql->execute($var_array))
        return "Error??";
    return 'Edited '.count($temp_array).' values for character '.$_POST['id'].'.';
}

function deleteCharacter(PDO $conn) {
	if (!is_numeric($_POST['id']))
        return 'Character ID is not a number.';
    $sql = $conn->prepare('DELETE FROM Characters WHERE ID=?;');
    $sql->execute(array($_POST['id']));
    return 'Deleted character '.$_POST['id'].'.';
}

function addRecord(PDO $conn) {
    if (!is_numeric($_POST['path']))
        return 'Path ID is not a number.';
    if (!is_numeric($_POST['validity']))
        return 'Validity is not a number.';
    if (!is_numeric($_POST['category']))
        return 'Category is not a number.';
    $videos = explode('|', $_POST['videos']);
    $characters = explode('|', $_POST['characters']);
    $groups = explode('|', $_POST['groups']);
    if (count($videos) != count($characters))
        return 'Character and video amount doesn\'t match';

    $sql = $conn->prepare('INSERT INTO Records (time, date, path, validity, topiclink, category) VALUES (?, ?, ?, ?, ?, ?);');
    $sql->execute(array($_POST['time'], $_POST['date'], $_POST['path'], $_POST['validity'], $_POST['topiclink'], $_POST['category']));
    // Get record id.
    $recordID = $conn->lastInsertId();
	if ($recordID == 0 || !is_numeric($recordID))
		return 'Failed to get proper record ID. Please delete the added record.';

	// Add videos and characters.
    $sql = $conn->prepare('INSERT INTO Videos (link, record, characterID) VALUES (?, ?, ?);');
	for($i = 0; $i < count($characters); $i++) {
		if ($characters[$i] == 0 || $characters[$i] == '')
            continue;
		if (!is_numeric($characters[$i]))
			return 'Character ID is not a number. Please delete the added record.';
        $sql->execute(array($videos[$i], $recordID, $characters[$i]));
	}
	
	// Add groups.
    $sql = $conn->prepare('SELECT tag FROM Groups WHERE ID=? LIMIT 1;');
    $sql2 = $conn->prepare('INSERT INTO RecordGroups (groupID, record) VALUES (?, ?);');
	for($i = 0; $i < count($groups); $i++) {
		if ($groups[$i] == 0 || $groups[$i] == '')
            continue;
		if (!is_numeric($groups[$i]))
			return 'Group ID is not a number. Please delete the added record.';
        // Check that group isn't a special unknown group.
        $sql->execute(array($groups[$i]));
        $tag = $sql->fetch()['tag'];
        if ($tag == '[]')
            continue;
		// Add group.
        $sql2->execute(array($groups[$i], $recordID));
	}
    return 'Added record.';
}

function editRecord(PDO $conn) {
    // Note: This doesn't update characters and videos for a record.
    // Prevent stuff like empty id or *.
    if (!is_numeric( $_POST['id']))
        return 'Record ID is not a number.';
    // Check which values should be changed.
    $data = array('time', 'date', 'path', 'validity', 'topiclink', 'category');
    $temp_array = array();
    $var_array = array();
    foreach ($data as $row) {
        if (strlen($_POST[$row]) > 0) {
            $temp_array[] = $row.'=?';
            $var_array[] = $_POST[$row];
        }
    }
    $var_array[] = $_POST['id'];
    if (count($temp_array) == 0)
        return 'All values are empty. Nothing to edit.';
    $sql = $conn->prepare('UPDATE Records SET'.' '.implode(', ', $temp_array).' WHERE ID=?;');
    if (!$sql->execute($var_array))
        return "Error??";
    return 'Edited '.count($temp_array).' values for record '.$_POST['id'].'.';
}

function editCharactersRecord(PDO $conn) {
    // Prevent stuff like empty id or *.
    if (!is_numeric($_POST['id']))
        return 'Record ID is not a number.';
    // Characters and videos are assumed to match.
    $videos = explode('|', $_POST['videos']);
    $characters = explode('|', $_POST['characters']);
    if (count($videos) != count($characters))
        return 'Character and video amount doesn\'t match';
    //// Update character entries.
    // Get entries.
    $sql = $conn->prepare('SELECT ID FROM Videos WHERE record=?;');
    $sql->execute(array($_POST['id']));
    $entries = $sql->fetchAll();
    // Just update everything. No guarantee that things are in order.
    $i = 0;
    $j = 0;
    // Update current entries.
    $sql = $conn->prepare('UPDATE Videos SET link=?, characterID=? WHERE ID=?;');
    while($i < count($characters) && $j < count($entries)) {
        if ($characters[$i] == 0 || $characters[$i] == '') {
            $i++;
            continue;
        }
        $sql->execute(array($videos[$i], $characters[$i], $entries[$j]['ID']));
        $i++;
        $j++;
    }
    // Add missing entries.
    $sql = $conn->prepare('INSERT INTO Videos (link, record, characterID) VALUES (?, ?, ?);');
    for (;$i < count($characters); $i++) {
        if ($characters[$i] == 0 || $characters[$i] == '')
            continue;
        $sql->execute(array($videos[$i], $_POST['id'], $characters[$i]));
    }
    // Delete extra entries.
    $sql = $conn->prepare('DELETE FROM Videos WHERE ID=?;');
    for (;$j < count($entries); $j++) {
        $sql->execute(array($entries[$j]['ID']));
    }

    //// Get group info.
    $groups = explode('|', $_POST['groups']);
    //// Update group entries.
    // Get entries.
    $sql = $conn->prepare('SELECT ID FROM RecordGroups WHERE record=?;');
    $sql->execute(array($_POST['id']));
    $entries = $sql->fetchAll();
    // Just update everything. No guarantee that things are in order.
    $i = 0;
    $j = 0;
    // Update current entries.
    $sql = $conn->prepare('UPDATE RecordGroups SET record=?, groupID=? WHERE ID=?;');
    while($i < count($groups) && $j < count($entries)) {
        if ($groups[$i] == 0 || $groups[$i] == '') {
            $i++;
            continue;
        }
        $sql->execute(array($_POST['id'], $groups[$i], $entries[$j]['ID']));
        $i++;
        $j++;
    }
    // Add missing entries.
    $sql = $conn->prepare('INSERT INTO RecordGroups (record, groupID) VALUES (?, ?);');
    for (;$i < count($groups); $i++) {
        if ($groups[$i] == 0 || $groups[$i] == '')
            continue;
        $sql->execute(array($_POST['id'], $groups[$i]));
    }
    // Delete extra entries.
    $sql = $conn->prepare('DELETE FROM RecordGroups WHERE ID=?;');
    for (;$j < count($entries); $j++) {
        $sql->execute(array($entries[$j]['ID']));
    }
    return 'Edited characters and videos for record '.$_POST['id'].'.';
}

function deleteRecord(PDO $conn) {
    if (!is_numeric( $_POST['id']))
        return 'Record ID is not a number.';
    $sql = $conn->prepare('DELETE FROM Records WHERE ID=?;');
    $sql->execute(array($_POST['id']));
    return 'Deleted record '.$_POST['id'].'.';
}

function addGameBuild(PDO $conn) {
    if (!is_numeric($_POST['build']))
        return 'Game build is not a number.';
    $sql = $conn->prepare('INSERT INTO GameBuilds (date, build) VALUES (?, ?);');
    $sql->execute(array($_POST['date'], $_POST['build']));
    return 'Added game build '.$_POST['build'].'.';
}

function editGameBuild(PDO $conn) {
    // Prevent stuff like empty id or *.
    if (!is_numeric( $_POST['id']))
        return 'Game build ID is not a number.';
    if (strlen($_POST['build']) > 0 && !is_numeric($_POST['build']))
        return 'Game build is not a number.';
    // Check which values should be changed.
    $data = array('date', 'build');
    $temp_array = array();
    $var_array = array();
    foreach ($data as $row) {
        if (strlen($_POST[$row]) > 0) {
            $temp_array[] = $row.'=?';
            $var_array[] = $_POST[$row];
        }
    }
    $var_array[] = $_POST['id'];
    if (count($temp_array) == 0)
        return 'All values are empty. Nothing to edit.';
    $sql = $conn->prepare('UPDATE GameBuilds SET'.' '.implode(', ', $temp_array).' WHERE ID=?;');
    if (!$sql->execute($var_array))
        return "Error??";
    return 'Edited '.count($temp_array).' values for game build '.$_POST['id'].'.';
}

function deleteGameBuild(PDO $conn) {
    if (!is_numeric( $_POST['id']))
        return 'Game build ID is not a number.';
    $sql = $conn->prepare('DELETE FROM GameBuilds WHERE ID=?;');
    $sql->execute(array($_POST['id']));
    return 'Deleted game build '.$_POST['id'].'.';
}
