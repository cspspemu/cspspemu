<?php

date_default_timezone_set('Europe/Madrid');

class Common {
	static public function getMsBuildPath() {
		$data = `REG QUERY HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions\\4.0 /v MSBuildToolsPath`;
		preg_match('@REG_SZ\s+(.*)$@msi', $data, $matches);
		return trim($matches[1]) . "\\MSBuild.exe";
	}

	static public function getGitPath() {
		return 'git';
	}

	static public function getGitVersion() {
		$git = static::getGitPath();
		if (preg_match('@\\d+\\.\\d+\\.\\d+@', `{$git} --version 2>&1`, $matches)) {
			return $matches[0];
		} else {
			return '';
		}
	}

	static public function getUtilsPath() {
		return realpath(__DIR__);
	}

	static public function getRootPath() {
		return realpath(dirname(static::getUtilsPath()));
	}
}

function file_put_contents_if_newer($file, $content) {
	if (@file_get_contents($file) != $content) file_put_contents($file, $content);
}
