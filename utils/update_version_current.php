<?php
function file_put_contents_if_newer($file, $content) {
	if (@file_get_contents($file) != $content) file_put_contents($file, $content);
}

date_default_timezone_set('europe/madrid');
$rootDir = realpath(dirname(__DIR__));

$gitrev = @explode("\n", `git rev-parse HEAD {$rootDir}`); $gitrev = trim($gitrev[0]);
file_put_contents_if_newer("{$rootDir}/git_revision.txt", $gitrev);

if (@$argv[1] == 'Release') {
	$content = date('Ymd');
} else {
	$content = date('Y-m-d') . '/' . substr($gitrev, 0, 10) . '/Git';
}
file_put_contents_if_newer("{$rootDir}/version_current.txt", $content);
