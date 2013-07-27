<?php

function merge_iterator($a, $b) {
	foreach ($a as $v) yield $v;
	foreach ($b as $v) yield $v;
}

function globRecursive($path, $find) {
    $dh = opendir($path);
    while (($file = readdir($dh)) !== false) {
        if (substr($file, 0, 1) == '.') continue;
        $rfile = "{$path}/{$file}";
        if (is_dir($rfile)) {
            foreach (globRecursive($rfile, $find) as $ret) {
                yield $ret;
            }
        } else {
            if (fnmatch($find, $file)) yield $rfile;
        }
    }
    closedir($dh);
}

foreach (merge_iterator(globRecursive('src', '*.cc'), globRecursive('src', '*.cpp')) as $file) {
	$bcfile = basename(pathinfo($file, PATHINFO_FILENAME) . '.bc');
	$llfile = basename(pathinfo($file, PATHINFO_FILENAME) . '.ll');
	passthru("c:\\mingw64\\bin\\clang.exe -O3 -Iinclude -emit-llvm -c {$file} -o {$bcfile}");
	passthru("c:\\mingw64\\bin\\llvm-dis.exe {$bcfile} -o {$llfile}");
}
//c:\mingw64\bin\clang.exe -O3 -Iinclude -emit-llvm -c 