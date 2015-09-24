<?php
// Separate metadata and content.
// First line has title.
// Second line has names for the navigation bar.
// Third line has links for the navigation bar.
$split = explode("\n", $content, 4);
$split[0] = trim(substr($split[0], 6));
$title = array_merge($title, explode('|', $split[0]));
$content = '
<script type="text/javascript">
	"use strict";
    $(document).ready(function () {
		loadSettings(true, true);
		loadCommentBox();
		loadPage();
		generateSubNavigation('.json_encode(explode('|', $split[1])).', '.json_encode(explode('|', $split[2])).');
    });
</script>
'.$split[3];
