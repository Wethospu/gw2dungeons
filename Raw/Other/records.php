<?php
	$return = array();
	$success = true;
	$message = '';
	try {
        $conn = new PDO('mysql:host=localhost;dbname=RecordDB', 'Watcher', 'FFDgTJ_(omf[');
        // set the PDO error mode to exception
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        if ($_POST['type'] == 'records') {
			if ($_POST['amount'] == 0)
				$return['records'] = readRecordsPathByPath($conn);
			else
				$return['records'] = readRecordsByDate($conn);
			if ($_POST['group'] > 0)
				$return['players'] = readPlayersByGroup($conn, $_POST['group']);
			else if ($_POST['player'] > 0) {
				$return['characters'] = readCharactersByPlayer($conn, $_POST['player']);
				$return['groups'] = readGroupsByPlayer($conn, $_POST['player']);
			}
		}
		else if ($_POST['type'] == 'readData') {
			$return['instanceTypes'] = readInstanceTypes($conn);
			$return['instances'] = readInstances($conn);
			$return['paths'] = readPaths($conn);
			$return['professions'] = readProfessions($conn);
			$return['categories'] = readCategories($conn);
			if ($_POST['page'] == 'Guilds')
				$return['groups'] = readGroups($conn);
			else if ($_POST['page'] == 'Players')
				$return['players'] = readPlayers($conn);
		}
		else if ($_POST['type'] == 'readProfessions') {
			$return['professions'] = readProfessions($conn);
		}
		else {
			$message = "Site error. Action not recognized.";
			$success = false;
		}

        $conn = null;

    }
    catch(PDOException $e)
    {
        $success = false;
		$message = 'Error ('.$e->getCode().'): '.$e->getMessage();
    }
	$return['success'] = $success;
	$return['message'] = $message;
	echo json_encode($return);
	exit();
	
	function readRecordsPathByPath(PDO $conn) {
		$paths = explode('|', $_POST['paths']);
		$records = array();
		// Read top record for each given path.
		$string = 'SELECT ID, time, date, validity, topiclink, category FROM Records WHERE path=? ';
		// Only validity should filter.
		if ($_POST['validity'] == 1)
			$string = $string.'AND validity=1 ';
		if ($_POST['date1'] != '')
			$string = $string.'AND date>=\''.$_POST['date1'].'\' ';
		if ($_POST['date2'] != '')
			$string = $string.'AND date<=\''.$_POST['date2'].'\' ';
		$string = $string.'ORDER BY time ASC;';
		$sql = $conn->prepare($string);
		// Read characters and videos.
		$sql2 = $conn->prepare('SELECT characterID, link FROM Videos WHERE record=?;');
		$sql3 = $conn->prepare('SELECT name, profession, player FROM Characters WHERE ID=? LIMIT 1;');
		// Read groups.
		$sql5 = $conn->prepare('SELECT groupID FROM RecordGroups WHERE record=?;');
		$sql6 = $conn->prepare('SELECT name, tag, website FROM Groups WHERE ID=? LIMIT 1;');
		// Find game build.
		$sql7 = $conn->prepare('SELECT build FROM GameBuilds WHERE date < ? ORDER BY Build DESC LIMIT 1;');
		// Replace category id with category text.
		$sql8 = $conn->prepare('SELECT name FROM Categories WHERE ID=?;');
		foreach($paths as $row) {
			$sql->execute(array($row));
			// Use while(true) with limits.
			for ($tries = 0; $tries < 1000; $tries++) {
				// Read top records one by one until matching one is found.
				$record = $sql->fetch();
				// Check that next record exists.
				if (!$record)
					break;
				// Check that category is correct.
				// Accept trio and quadro for full party.
				if ($_POST['category'] == 1 && $record['category'] != 1 && $record['category'] != 6 && $record['category'] != 5)
					continue;
				if ($_POST['category'] == 3 && $record['category'] != 3)
					continue;
				// Accept solo for duo.
				if ($_POST['category'] == 4 && $record['category'] != 3 && $record['category'] != 4)
					continue;
				if ($_POST['category'] == 5 && $record['category'] != 5)
					continue;
				// Accept trio for quadro.
				if ($_POST['category'] == 6 && $record['category'] != 5 && $record['category'] != 6)
					continue;
				// Get characters and videos.
				$sql2->execute(array($record['ID']));
				$record['characters'] = $sql2->fetchAll();
				// Check that players match the wanted player.
				$playersMatch = $_POST['player'] == 0;
				// Transform character id to character data.
				for ($i = 0; $i < count($record['characters']); $i++) {
					$sql3->execute(array($record['characters'][$i]['characterID']));
					$character = $sql3->fetch();
					$record['characters'][$i]['profession'] = $character['profession'];
					$record['characters'][$i]['name'] = $character['name'];
					$record['characters'][$i]['player'] = $character['player'];
					if ($_POST['player'] == $character['player'])
						$playersMatch = true;
				}
				if (!$playersMatch)
					continue;
				// Check that professions match the wanted profession.
				$professionsMatch = $_POST['profession'] == 0;
				for ($i = 0; $i < count($record['characters']); $i++) {
					if ($_POST['profession'] == $record['characters'][$i]['profession'])
						$professionsMatch = true;
				}
				if (!$professionsMatch)
					continue;
				$sql5->execute(array($record['ID']));
				$record['groups'] = $sql5->fetchAll();
				// Check that groups match the wanted group.
				$groupsMatch = $_POST['group'] == 0;
				// Transform group id to group data.
				for ($i = 0; $i < count($record['groups']); $i++) {
					if ($_POST['group'] == $record['groups'][$i]['groupID'])
						$groupsMatch = true;
					$sql6->execute(array($record['groups'][$i]['groupID']));
					$record['groups'][$i] = $sql6->fetch();			
				}
				if (!$groupsMatch)
					continue;
				$sql7->execute(array($record['date']));
				$record['build'] = $sql7->fetch()['build'];
				$sql8->execute(array($record['category']));
				$record['category'] = $sql8->fetch()['name'];

				$records[$row] = $record;
				break;
			}

		}
		return $records;
	}

	function readRecordsByDate(PDO $conn) {
		$records = array();
		$counter = $_POST['amount'];
		if ($counter < 1)
			return $records;
		// Read X last records with given filters.
		$string = 'SELECT ID, time, date, path, validity, topiclink, category FROM Records WHERE TRUE ';
		// Only validity should filter.
		if ($_POST['validity'] == 1)
			$string = $string.'AND validity=1 ';
		if ($_POST['date1'] != '')
			$string = $string.'AND date>=\''.$_POST['date1'].'\' ';
		if ($_POST['date2'] != '')
			$string = $string.'AND date<=\''.$_POST['date2'].'\' ';
		if ($_POST['path'] > 0 )
			$string = $string.'AND path=\''.$_POST['path'].'\' ';
		$string = $string.'ORDER BY date DESC;';
		$sql = $conn->prepare($string);
		// Check that instance is correct.
		$sql4 = $conn->prepare('SELECT instance, (SELECT type FROM Instances WHERE ID=instance) AS type FROM Paths WHERE ID=? LIMIT 1;');
		// Read characters and videos.
		$sql2 = $conn->prepare('SELECT characterID, link FROM Videos WHERE record=?;');
		$sql3 = $conn->prepare('SELECT name, profession, player FROM Characters WHERE ID=? LIMIT 1;');
		// Read groups.
		$sql5 = $conn->prepare('SELECT groupID FROM RecordGroups WHERE record=?;');
		$sql6 = $conn->prepare('SELECT name, tag, website FROM Groups WHERE ID=? LIMIT 1;');
		// Find game build.
		$sql7 = $conn->prepare('SELECT build FROM GameBuilds WHERE date < ? ORDER BY Build DESC LIMIT 1;');
		// Replace category id with category text.
		$sql8 = $conn->prepare('SELECT name FROM Categories WHERE ID=?;');
		$sql->execute();
		// Use while(true) with limits.
		for ($tries = 0; $tries < 1000; $tries++) {
			// Read top records one by one until matching one is found.
			$record = $sql->fetch();
			// Check that next record exists.
			if (!$record)
				break;
			// Check that category is correct.
			// Accept trio and quadro for full party.
			if ($_POST['category'] == 1 && $record['category'] != 1 && $record['category'] != 6 && $record['category'] != 5)
				continue;
			if ($_POST['category'] == 3 && $record['category'] != 3)
				continue;
			// Accept solo for duo.
			if ($_POST['category'] == 4 && $record['category'] != 3 && $record['category'] != 4)
				continue;
			if ($_POST['category'] == 5 && $record['category'] != 5)
				continue;
			// Accept trio for quadro.
			if ($_POST['category'] == 6 && $record['category'] != 5 && $record['category'] != 6)
				continue;
			
			// Check that path is correct.
			if ($_POST['instance'] > 0 || $_POST['instance_type'] > 0) {
				$sql4->execute(array($record['path']));
				$path = $sql4->fetch();
				if ($_POST['instance'] > 0 && ($_POST['instance'] != $path['instance']))
					continue;
				if ($_POST['instance_type'] > 0 && ($_POST['instance_type'] != $path['type']))
					continue;
			}
			
			// Get characters and videos.
			$sql2->execute(array($record['ID']));
			$record['characters'] = $sql2->fetchAll();
			// Check that players match the wanted player.
			$playersMatch = $_POST['player'] == 0;
			// Transform character id to character data.
			for ($i = 0; $i < count($record['characters']); $i++) {
				$sql3->execute(array($record['characters'][$i]['characterID']));
				$character = $sql3->fetch();
				$record['characters'][$i]['profession'] = $character['profession'];
				$record['characters'][$i]['name'] = $character['name'];
				if ($_POST['player'] == $character['player'])
					$playersMatch = true;
			}
			if (!$playersMatch)
				continue;
			// Check that professions match the wanted profession.
			$professionsMatch = $_POST['profession'] == 0;
			for ($i = 0; $i < count($record['characters']); $i++) {
				if ($_POST['profession'] == $record['characters'][$i]['profession'])
					$professionsMatch = true;
			}
			if (!$professionsMatch)
				continue;
			$sql5->execute(array($record['ID']));
			$record['groups'] = $sql5->fetchAll();
			// Check that groups match the wanted group.
			$groupsMatch = $_POST['group'] == 0;
			// Transform group id to group data.
			for ($i = 0; $i < count($record['groups']); $i++) {
				if ($_POST['group'] == $record['groups'][$i]['groupID'])
					$groupsMatch = true;
				$sql6->execute(array($record['groups'][$i]['groupID']));
				$record['groups'][$i] = $sql6->fetch();			
			}
			if (!$groupsMatch)
				continue;
			$sql7->execute(array($record['date']));
			$record['build'] = $sql7->fetch()['build'];
			$sql8->execute(array($record['category']));
			$record['category'] = $sql8->fetch()['name'];
			$records[] = $record;
			$counter--;
			if ($counter == 0)
				break;
		}
		return $records;
	}
	
	function readGroups(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name, tag, website FROM Groups ORDER BY name');
		$sql->execute();
		return $sql->fetchAll();
	}

	function readPlayers(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name FROM Players ORDER BY name');
		$sql->execute();
		$players = $sql->fetchAll();
		// Read groups for the players.
		for ($i = 0; $i < count($players); $i++)
			$players[$i]['groups'] = readGroupsByPlayer($conn, $players[$i]['ID']);
		return $players;
	}
	
	function readPlayersByGroup(PDO $conn, $group) {
		$sql = $conn->prepare('SELECT player, (SELECT name FROM Players WHERE ID = player) AS name FROM PlayerGroups WHERE groupID=?;');
		$sql->execute(array($group));
		$players = $sql->fetchAll();
		// Read characters for the players.
		$sql = $conn->prepare('SELECT name, profession FROM Characters WHERE player=?;');
		for ($i = 0; $i < count($players); $i++) {
			$sql->execute(array($players[$i]['player']));
			$players[$i]['characters'] = $sql->fetchAll();
		}
		return $players;
	}

	function readInstances(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name, image, type FROM Instances ORDER BY ordering');
		$sql->execute();
		return $sql->fetchAll();
	}

	function readInstanceTypes(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name FROM InstanceTypes ORDER BY ID');
		$sql->execute();
		return $sql->fetchAll();
	}

	function readPaths(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name, validity, instance, minPlayers FROM Paths ORDER BY ordering');
		$sql->execute();
		return $sql->fetchAll();
	}

	function readProfessions(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name, image FROM Professions ORDER BY name');
		$sql->execute();
		return $sql->fetchAll();
	}

	function readCategories(PDO $conn) {
		$sql = $conn->prepare('SELECT ID, name FROM Categories ORDER BY ID');
		$sql->execute();
		return $sql->fetchAll();
	}
	
	function readCharactersByPlayer(PDO $conn, $player) {
		// Get player's characters.
		$sql = $conn->prepare('SELECT name, profession FROM Characters WHERE player=?;');
		$sql->execute(array($player));
		return $sql->fetchAll();
	}

	function readGroupsByPlayer(PDO $conn, $player) {
		// Get group ids.
		$sql = $conn->prepare('SELECT groupID FROM PlayerGroups WHERE player=? LIMIT 5;');
		$sql->execute(array($player));
		$groups = $sql->fetchAll();
		// Get group name.
		$sql = $conn->prepare('SELECT name, tag FROM Groups WHERE ID=? LIMIT 1;');
		for($i = 0; $i < count($groups); $i++) {
			$sql->execute(array($groups[$i]['groupID']));
			$groups[$i] = $sql->fetch();
		}
		return $groups;
	}
	