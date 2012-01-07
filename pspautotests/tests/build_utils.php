<?php
// Requires PHP 5.3

class PspSdk {
	public $PSPSDK;
	public $GCC;
	public $FIXUP_IMPORTS;
	public $MKSFO;
	public $PACK_PBP;
	public $EXE_PREFIX;
	//public $PSP_FW_VERSION = 150;
	//public $PSP_FW_VERSION = 371;
	public $PSP_FW_VERSION = 500;
	
	public $gccCmd;
	public $fixupCmd;
	
	public function __construct() {
		if (PHP_OS == 'WINNT') {
			$this->EXE_PREFIX = '.exe';
		} else {
			$this->EXE_PREFIX = '';
		}

		$this->PSPSDK        = $this->detect_pspsdk_path();
		$this->GCC           = realpath("{$this->PSPSDK}/bin/psp-gcc{$this->EXE_PREFIX}");
		$this->FIXUP_IMPORTS = realpath("{$this->PSPSDK}/bin/psp-fixup-imports{$this->EXE_PREFIX}");
		$this->MKSFO         = realpath("{$this->PSPSDK}/bin/mksfo{$this->EXE_PREFIX}");
		$this->PACK_PBP      = realpath("{$this->PSPSDK}/bin/pack-pbp{$this->EXE_PREFIX}");
	}
	
	public function gcc($params) {
		$args = array();
		
		$elfoutput = $params['output'];
		
		foreach ($params['include_dirs'] as $include_dir) $args[] = "-I{$include_dir}";
		foreach ($params['library_dirs'] as $library_dir) $args[] = "-L{$library_dir}";
		foreach ($params['defines'] as $key => $value) $args[] = "-D{$key}={$value}";
		foreach ($params['flags'] as $flag) $args[] = $flag;
		foreach ($params['sources'] as $source) $args[] = $source;
		foreach ($params['libraries'] as $library) $args[] = "-l{$library}";
		$args[] = '-o';
		$args[] = $elfoutput;
		
		if (is_file($elfoutput)) @unlink($elfoutput);
		
		$gcc_cmd   = $this->GCC . ' ' . implode(' ', array_map('escapeshellarg', $args));
		$fixup_cmd = $this->FIXUP_IMPORTS . ' ' . escapeshellarg($elfoutput);
		//echo "$cmd\n";
		
		$this->gccCmd = $gcc_cmd;
		$this->fixupCmd = $fixup_cmd;
		
		$gcc_out = `{$gcc_cmd} 2>&1`;
		$fixup_out = '';
		
		if (is_file($elfoutput)) {
			$fixup_out = `$fixup_cmd 2>&1`;
		}

		return trim("{$gcc_out}\n{$fixup_out}");
	}

	static protected function detect_pspsdk_path() {
		$PSPSDK = getenv('PSPSDK');

		if ($PSPSDK == '') {
			foreach (array('/pspsdk') as $path) {
				if (is_dir($path)) {
					$PSPSDK = realpath($path);
					break;
				}
			}
		}
		
		if ($PSPSDK == '') {
			throw(new Exception("Can't find the psp sdk. Please set the enviroment variable PSPSDK."));
		}

		return $PSPSDK;
	}
}

function endsWith($str, $end) {
	return (substr($str, -strlen($end)) == $end);
}

function recursive_directory_iterator($dir) {
	return new RecursiveIteratorIterator(new RecursiveDirectoryIterator($dir), RecursiveIteratorIterator::CHILD_FIRST);
}