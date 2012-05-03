<?php

date_default_timezone_set('Europe/Madrid');

chdir(dirname(__DIR__));

`copy /Y cspspemu.exe deploy\\cspspemu\\cspspemu.exe`;

echo "REMOVING OLD FILES...\n";
`DEL /Q deploy\\cspspemu.7z 2> NUL`;
`DEL /Q deploy\\cspspemu\\src.zip 2> NUL`;
`RD /S /Q deploy\\cspspemu\\ms 2> NUL`;
`RD /S /Q deploy\\cspspemu\\src 2> NUL`;

$git_revision_count = count(explode("\n", `git rev-list --all`));
$commitDate = strtotime(trim(`git log -1 --no-decorate --pretty=format:%ai`));

echo "ARCHIVING GIT SOURCE...\n";
`git archive master --format zip -0 --output deploy\\cspspemu\\src.zip`;

echo "CREATING 7Z...\n";
chdir('deploy');

$file = sprintf('cspspemu-%s-r%d.7z', date('Ymd', $commitDate), $git_revision_count);

`..\\utils\\7z\\7z a -t7z "{$file}" cspspemu -aoa -mx9`;

