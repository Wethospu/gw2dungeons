<?php
// Separate metadata and content.
// First line has "GUIDE|" and additional info.
// Second line has title.
// Third line has names for the navigation bar.
// Fourth line has links for the navigation bar.
$split = explode("\n", $content, 5);
$additional = explode("|", $split[0]);
if (sizeof($additional) > 1)
	$additional = $additional[1];
else
	$additional = "0";
$title = array_merge($title, explode('|', $split[1]));
$content = '
<script type="text/javascript">
	"use strict";
    $(document).ready(function () {
		generateSubNavigation('.json_encode(explode('|', $split[2])).', '.json_encode(explode('|', $split[3])).');
		loadSettings(true, true);
		loadCommentBox();
		loadPage();
		var scale = '.$additional.'
		if (scale > 0)
			saveSetting("currentScale", scale);
    });
</script>
'.$split[4];
