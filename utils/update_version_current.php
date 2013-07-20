<?php

require_once(__DIR__ . '/common.php');

$rootDir = Common::getRootPath();
$git = Common::getGitPath();

$gitrev = @explode("\n", `{$git} rev-parse HEAD {$rootDir}`); $gitrev = trim($gitrev[0]);
$rev_count = count(explode("\n", trim(`{$git} rev-list --all`))) + 0;

if (@$argv[1] == 'Release') {
	$content = date('Ymd');
} else {
	$content = date('Ymd') . '/' . substr($gitrev, 0, 10) . '/Git';
}

file_put_contents_if_newer("{$rootDir}/git_revision.txt", $gitrev);
file_put_contents_if_newer("{$rootDir}/version_current.txt", $content);
file_put_contents_if_newer("{$rootDir}/version_current_numeric.txt", $rev_count);
