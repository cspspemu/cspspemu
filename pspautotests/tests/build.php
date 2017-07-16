<?php
// Requires PHP >= 5.3

require_once(__DIR__ . '/build_utils.php');

$pspSdk = new PspSdk();
$PSPAUTOTESTS = dirname(__DIR__);

//print_r($pspSdk);

$params = array_slice($argv, 1);

$filter = '*';
$path = __DIR__;

if (count($params) > 0) {
	$param = $params[0];
	
	if (dirname($param) == '.') {
		$filter = "*{$param}*";
	} else {
		if (is_dir($param)) {
			$path = $param;
		} else {
			$path = dirname($param);
		}
	}
}

foreach (recursive_directory_iterator($path) as $file) {
	if (!endsWith($file, '.expected')) continue;
	
	//if (!preg_match('@kirk@', $file)) continue;
	//if (!preg_match('@vfpu@', $file)) continue;
	//if (!preg_match('@fonttest@', $file)) continue;
	if (!fnmatch($filter, $file)) continue;
	
	$filebase = substr($file, 0, strrpos($file, '.'));
	$dirbase = dirname($filebase);
	$cfile = "{$filebase}.c";
	$outfile = "{$filebase}.elf";
	$eboot_pbp = "{$dirbase}/EBOOT.PBP";
	$param_sfo = "{$dirbase}/PARAM.SFO";
	$compilefile = "{$filebase}.compile";
	
	$libraries = array(
		'pspumd', 'psppower', 'pspdebug', 'pspgum_vfpu', 'pspgu',
		'pspge', 'pspdisplay', 'pspsdk', 'm', 'c', 'pspnet',
		'pspnet_inet', 'pspuser', 'psprtc', 'pspctrl'
	);
	
	$sources = array(
		"{$PSPAUTOTESTS}/common/common.c",
		$cfile,
	);
	
	if (is_file($compilefile)) {
		$info = parse_ini_string(file_get_contents($compilefile));
		if (isset($info['EXTRA_C_FILES'])) {
			$sources = array_merge($sources, array_map(function($name) use($dirbase) {
				return "{$dirbase}/{$name}";
			}, explode(' ', $info['EXTRA_C_FILES'])));
		}
		if (isset($info['EXTRA_LIBS'])) {
			$libraries = array_merge($libraries, explode(' ', $info['EXTRA_LIBS']));
		}
		if (isset($info['EXTRA_LIBS_PRE'])) {
			$libraries = array_merge(explode(' ', $info['EXTRA_LIBS_PRE']), $libraries);
		}
	}
	
	$gcc_args = array(
		'include_dirs' => array(".", "{$pspSdk->PSPSDK}/psp/sdk/include", "{$PSPAUTOTESTS}/common"),
		'library_dirs' => array(".", "{$pspSdk->PSPSDK}/psp/sdk/lib", "{$PSPAUTOTESTS}/common"),
		'defines'       => array(
			"_PSP_FW_VERSION" => $pspSdk->PSP_FW_VERSION
		),
		'flags' => array('-Wall', '-g', '-O0'),
		'libraries' => $libraries,
		'sources' => $sources,
		'output' => $outfile,
	);
	
	$src_time = @max(array_map('filemtime', $sources));
	$out_time = @filemtime($outfile);

	echo "{$cfile}...";
	if ($out_time < $src_time) {
		$output = $pspSdk->gcc($gcc_args);

		if ($output != '') {
			@unlink($outfile);
			echo "\n\n";
			echo "{$pspSdk->gccCmd}\n";
			echo "{$pspSdk->fixupCmd}\n";
			echo "\n\n{$output}\n";
			echo "...Error\n";
		} else {
			if (is_file($outfile)) {
				echo `{$pspSdk->MKSFO} TESTMODULE "{$param_sfo}"`;
				echo `{$pspSdk->PACK_PBP} "{$eboot_pbp}" "{$param_sfo}" NUL NUL NUL NUL NUL "{$outfile}" NUL > NUL`;
				
				@unlink($param_sfo);
				
				touch($outfile, $src_time);
				touch($eboot_pbp, $src_time);
			}

			echo "Ok\n";
		}
	} else {
		echo "Uptodate\n";
	}
}
