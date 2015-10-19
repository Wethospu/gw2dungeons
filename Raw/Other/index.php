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
$title = array('GW2 Dungeon Guide');

// classes added to html
$class = array('no-js');

// make sure people can't access stuff outside pages/ and default to home
$page = str_replace(array('.', '..', '/'), '', $_SERVER['QUERY_STRING']);
if (empty($page)) {
    $page = 'home';
}
$to_load = $page;
if (strtolower($page) == 'guilds' || strtolower($page) == 'players')
	$to_load = 'Records';
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
    if (substr($content, 0, 6) == 'GUIDE:') {
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
    <meta name="description" content="A complete guide to Guild Wars 2 dungeons.">
    <meta name="keywords" content="guild wars,gw2,dungeon,fractals,guide,walkthrough,ac,cm,ta,se,cof,hotw,coe,arah,fotm">
    <meta name="author" content="Jere Kuusela">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <link href='http://fonts.googleapis.com/css?family=PT+Sans:400,700,400italic,700italic' rel='stylesheet' type='text/css'>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css">
	<link rel="stylesheet" href="//code.jquery.com/ui/1.11.2/themes/smoothness/jquery-ui.css">
ID_CSS
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js"></script>
	<script src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.11.4/jquery-ui.min.js"></script>
	<script src="//s3.amazonaws.com/cc.silktide.com/cookieconsent.latest.min.js"></script>
ID_JS
    <!--[if lt IE 9]>
        <script src="/media/js/html5shiv.js"></script>
    <![endif]-->
	<!-- Begin Cookie Consent plugin by Silktide - http://silktide.com/cookieconsent -->
	<script type="text/javascript">
		window.cookieconsent_options = {"message":"This website uses cookies to store your preferences and to personalize advertising. If you don't like that please exit the site immediately.","dismiss":"Ok","learnMore":"More info","link":null,"theme":"dark-top"};
	</script>
	
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
            <a class="navbar-brand" href="./">GW2Dungeons.net</a>
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
			<form class="navbar-form navbar-right" action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">
				<div class="form-group">
					<input type="hidden" name="cmd" value="_s-xclick">
					<input type="hidden" name="hosted_button_id" value="Y6ESU27U7V3J4">
					<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" name="submit" alt="PayPal - The safer, easier way to pay online!">
				</div>
				<p class="navbar-text navbar-right" style="font-size:smaller;margin-bottom:0;margin-top:0;">Site by Wethospu<br>Hosted by Dulfy.net</p>
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
	<script>
		(function(b,o,i,l,e,r){b.GoogleAnalyticsObject=l;b[l]||(b[l]=
            function(){(b[l].q=b[l].q||[]).push(arguments)});b[l].l=+new Date;
			e=o.createElement(i);r=o.getElementsByTagName(i)[0];
			e.src='//www.google-analytics.com/analytics.js';
			r.parentNode.insertBefore(e,r)}(window,document,'script','ga'));
		ga('create','UA-59645523-1');ga('send','pageview');
	</script>
</body>
</html>
