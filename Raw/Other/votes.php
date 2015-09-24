<?php
	// By default, use Watcher info so details don't have to be revealed in client side js files.
    $username = 'Watcher';
	if (!empty($_POST['username']))
		$username = $_POST['username'];
	$password = 'FFDgTJ_(omf[';
	if (!empty($_POST['password']))
		$password = $_POST['password'];
	$database = 'VoteDB';
	if (!empty($_POST['database']))
		$database = $_POST['database'];

	$polls = array();
	$success = true;
	$message = '';
	try {
        $conn = new PDO('mysql:host='.$servername.';dbname='.$database, $username, $password);
        // set the PDO error mode to exception
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
		if ($_POST['type'] == "login") {
            $message = "Login successful";
        }
        else if ($_POST['type'] == 'getCurrentPolls') {
			$polls = readCurrentPolls($conn, $username);
		}
		else if ($_POST['type'] == 'getOldPolls') {
			$polls = readOldPolls($conn, $username);
		}
		else if ($_POST['type'] == 'addVote') {
			$message = addVote($conn, $username);	
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
		if ($e->getCode() == 1044)
            $message = 'Incorrect password.';
        else if ($e->getCode() == 1045)
            $message = 'Failed to login.';
        else if ($e->getCode() == 2002)
            $message = 'Unable to reach the database.';
		else
			$message = 'Error ('.$e->getCode().'): '.$e->getMessage();
    }
	echo json_encode(array("message" => $message, "success" => $success, "polls" => $polls));
	exit();
	
	function readCurrentPolls(PDO $conn, $username) {
		date_default_timezone_set('Europe/London');
		$currentDate = date('Y-m-d');
		$records = array();
		// Read all open votes.
		$sql = $conn->prepare('SELECT ID, label, text, description, type, expire, topic, showvotes FROM Questions WHERE expire>? ORDER BY expire ASC;');
		$sql->execute(array($currentDate));
		// Read current votes.
		$sql2 = $conn->prepare('SELECT ID, vote, voter, comment FROM Votes WHERE questionID=?;');
		// Use while(true) with limits.
		$questions = array();
		for ($tries = 0; $tries < 1000; $tries++) {
			$question = $sql->fetch();
			if (!$question)
				break;
			foreach ($question as &$row)
				$row = utf8_encode($row);
			$sql2->execute(array($question['ID']));
			$question['votes'] = $sql2->fetchAll();
			foreach ($question['votes'] as &$row) {
				$row['comment'] = utf8_encode($row['comment']);
				// Check whether the vote should be hidden.
				// Note: Own vote has to be returned so that the site remembers your own votes!
				// Note: The admin should get the results.
				if ($question['showvotes'] == 0 && $username != 'Approver' && $username != $row['voter'])
					$row = null;
				else if ($username != $row['voter'] && $username != 'Approver')
					$row['voter'] = "";
			}
			$questions[$tries] = $question;
		}
		return $questions;
	}

	function readOldPolls(PDO $conn, $username) {
		date_default_timezone_set('Europe/London');
		$currentDate = date('Y-m-d');
		$records = array();
		// Read all open votes.
		$sql = $conn->prepare('SELECT ID, label, text, description, type, expire, topic FROM Questions WHERE expire<=? ORDER BY expire DESC;');
		$sql->execute(array($currentDate));
		// Read current votes.
		$sql2 = $conn->prepare('SELECT ID, vote, voter, comment FROM Votes WHERE questionID=?;');
		// Use while(true) with limits.
		$questions = array();
		for ($tries = 0; $tries < 1000; $tries++) {
			// Read top records one by one until matching one is found.
			$question = $sql->fetch();
			if (!$question)
				break;
			foreach ($question as &$row)
				$row = utf8_encode($row);
			$sql2->execute(array($question['ID']));
			$question['votes'] = $sql2->fetchAll();
			foreach ($question['votes'] as &$row) {
				$row['comment'] = utf8_encode($row['comment']);
				// Don't send the voter info except for the admin).
				if ($username != 'Approver')
					$row['voter'] = "";	
			}
			$questions[$tries] = $question;
		}
		return $questions;
	}
	
	function addVote(PDO $conn, $username) {
		if (!is_numeric($_POST['poll']))
			return 'Poll ID '.$_POST['poll'].' is not a number.';
		// Check whether the poll exists.
		$sql = $conn->prepare('SELECT ID FROM Questions WHERE ID=?;');
		$sql->execute(array($_POST['poll']));
		if (!$sql->fetch())
			return 'Poll ID '.$_POST['poll'].' doesn\'t exist.';
		// Check whether a vote already exists.
		$sql = $conn->prepare('SELECT ID FROM Votes WHERE questionID=? AND voter=?;');
		$sql->execute(array($_POST['poll'], $username));
		$previous_vote = $sql->fetch();
		if ($previous_vote)
		{
			//// Update the previous vote.
			$sql2 = $conn->prepare('UPDATE Votes SET vote=?,comment=? WHERE ID=?;');
			$sql2->execute(array($_POST['vote'], $_POST['comment'], $previous_vote['ID']));
			//// Delete any additional votes (bugs).
			$sql2 = $conn->prepare('DELETE FROM Votes WHERE ID=?;');
			for ($tries = 0; $tries < 1000; $tries++) {
				$previous_vote = $sql->fetch();
				if (!$previous_vote)
					break;
				$sql2->execute(array($previous_vote['ID']));
			}
			return 'Vote edited.';
		}
		else
		{
			//// Add a new vote.
			$sql = $conn->prepare('INSERT INTO Votes (questionID,vote,comment,voter) VALUES (?,?,?,?);');
			$sql->execute(array($_POST['poll'], $_POST['vote'], $_POST['comment'], $username));
			return 'Vote added.';
		}
	}
	