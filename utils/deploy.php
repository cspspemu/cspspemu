<?php

require_once(__DIR__ . '/common.php');

chdir(Common::getRootPath());

$msbuild = Common::getMsBuildPath();
$git = Common::getGitPath();

if (!Common::getGitVersion()) {
	die("Can't find GIT!\n");
}

$git_revision_count = count(explode("\n", `git rev-list --all`));
$commitDate = strtotime(trim(`git log -1 --no-decorate --pretty=format:%ai`));

printf("BUILDING RELEASE (%d, %s)...\n", $git_revision_count, date('d-m-Y H:i:s', $commitDate));
`"{$msbuild}" /p:Configuration=Release`;
`merge_release.bat`;
copy("cspspemu_release.exe", "deploy/cspspemu/cspspemu.exe");

echo "REMOVING OLD FILES...\n";
`DEL /Q deploy\\cspspemu.7z 2> NUL`;
`DEL /Q deploy\\cspspemu\\src.zip 2> NUL`;
`RD /S /Q deploy\\cspspemu\\ms 2> NUL`;
`RD /S /Q deploy\\cspspemu\\src 2> NUL`;

echo "ARCHIVING GIT SOURCE...\n";
`git archive master --format zip -0 --output deploy\\cspspemu\\src.zip`;

echo "CREATING 7Z...\n";
chdir('deploy');

$base_file = 'cspspemu';

$base_file .= '-' . date('Y-m-d', $commitDate);

$base_file .= ' + r' . $git_revision_count;

$base_file .= ' + windows';
if ($git_revision_count >= 264) $base_file .= ' + linux';
$base_file .= ' + src';

`..\\utils\\7z\\7z a -t7z "{$base_file}.7z" cspspemu -aoa -mx9`;

