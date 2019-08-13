<?php
// Character used to glue the different parts of the title.
$title_glue = ' - ';

// redirect search to search page as anchor
if(isset($_POST['search'])) {
    $search = urlencode($_POST['search']);
    header(sprintf('Location: ./%s#%s', 'Search', $search));
    exit();
}
// Title is automated except for guides, see guide.php.
// This is the last part of the title
$title = array('gw2dungeons');

// classes added to html
$class = array('no-js');

// make sure people can't access stuff outside pages/ and default to home
$page = substr(str_replace(array('.', '..', '/'), '', $_SERVER['QUERY_STRING']), 4);
if (empty($page)) {
    $page = 'home';
}
$to_load = $page;
if (strtolower($page) == 'guilds' || strtolower($page) == 'players')
	$to_load = 'Records';
// Redirect for old fractal pages (prevents old links from breaking).
else if (strtolower($page) == 'urban')
	$to_load = 'F1';
else if (strtolower($page) == 'swamp')
	$to_load = 'F2';
else if (strtolower($page) == 'aqua')
	$to_load = 'F3';
else if (strtolower($page) == 'uncat')
	$to_load = 'F4';
else if (strtolower($page) == 'snow')
	$to_load = 'F5';
else if (strtolower($page) == 'volc')
	$to_load = 'F6';
else if (strtolower($page) == 'cliff')
	$to_load = 'F7';
else if (strtolower($page) == 'under')
	$to_load = 'F8';
else if (strtolower($page) == 'furn')
	$to_load = 'F9';
else if (strtolower($page) == 'molten')
	$to_load = 'F10';
else if (strtolower($page) == 'aether')
	$to_load = 'F14';
else if (strtolower($page) == 'thauma')
	$to_load = 'F15';
else if (strtolower($page) == 'solid')
	$to_load = 'F20';
else if (strtolower($page) == 'mai')
	$to_load = 'F25';
// attempt to read the page file, ignore failure as it is handled below
$content = @file_get_contents(sprintf('./pages/%s.htm', strtolower($to_load)));

$end = '';

// Display a error if needed. Otherwise check if the first 6 character of the
// page are "GUIDE:" and if so treat it as one. See guide.php.
if($content == false) {
    $class[] = 'error';
    $title[] = 'Error';
    $content = file_get_contents(sprintf('./pages/%s.htm', 'error'));
} else {
    if (substr($content, 0, 5) == 'GUIDE') {
        $class[] = 'guide';
        include('guide.php');
	} else {
        $title[] = ucfirst($page);
        $class[] = $to_load;
    }
}

$title = implode($title_glue, array_reverse($title));
$class = implode(' ', $class);
$class = strtolower($class);
?><!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->

<html class="<?php echo $class; ?>">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <title><?php echo $title; ?></title>
    <meta name="description" content="A guide to Guild Wars 2 dungeons and fractals.">
    <meta name="keywords" content="guild wars,gw2,dungeon,fractals,guide,walkthrough,ac,cm,ta,se,cof,hotw,coe,arah,fotm">
    <meta name="author" content="Jere Kuusela">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <link href='https://fonts.googleapis.com/css?family=PT+Sans:400,700,400italic,700italic' rel='stylesheet' type='text/css'>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css">
	<link rel="stylesheet" href="https://code.jquery.com/ui/1.11.2/themes/smoothness/jquery-ui.css">
ID_CSS
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js"></script>
	<script src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.11.4/jquery-ui.min.js"></script>
ID_JS
    <!--[if lt IE 9]>
        <script src="/media/html5shiv.js"></script>
    <![endif]-->
	</head>
<body>
    <!--[if lt IE 7]>
    <p class="browsehappy">You are using an <strong>outdated</strong> browser. Please <a href="http://browsehappy.com/">upgrade your browser</a> to improve your experience.</p>
    <![endif]-->
	<nav class="navbar navbar-inverse my-bar">
    <div class="container">
        <div class="navbar-header">
            <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false" aria-controls="navbar">
                <span class="sr-only">Toggle navigation</span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
            </button>
            <a class="navbar-brand" href="./">gw2dungeons.net</a>
        </div>
        <div id="navbar" class="navbar-collapse collapse">
            <ul class="nav navbar-nav">
				<li><a href="./General">General</a></li>
				<li><a href="./Records">Records</a></li>
                <li><a id="nav-setting" href="./Settings">Settings</a></li>
                <li><a href="./About">About</a></li>		
			</ul>
			<form class="navbar-form navbar-left" method="post" action="./">
                <div class="form-group">
					<!-- ID needed to set search text once search page has loaded. -->
                    <input id="topbar-search" type="text" class="form-control" placeholder="Search..." name="search">
                </div>
                <button class="btn btn-default" type="submit"><span class="glyphicon glyphicon-search"></span></button>
            </form>
        </div>
		<!-- Subnavigation generated dynamically when needed. -->
		<div id="sub-navigation"class="navbar-collapse collapse">
		</div>
    </div>
	</nav>
	<div class="container-fluid" id="screen">
		<div class="row">
			<div class="col-md-12" id="main-container">
				<?php echo $content; ?>
			</div>
			<div id="detail-container">
			</div>
        </div>
		<div class="row">
			<div class="col-md-12" id="comment-section">
			</div>
		</div>
	</div>
	<div id="left-ad" class="side-ad"></div>
	<div id="right-ad" class="side-ad"></div>
	<div id="data-overlay" class="modal" tabindex="-1">
		<div class="modal-dialog">
			<div class="modal-content">
			</div>
		</div>
	</div>
</body>
</html>
