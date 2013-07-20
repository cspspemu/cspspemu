@ECHO OFF
ECHO UPDATE_VERSION_CURRENT
CD "%~dp0"
"%~dp0\utils\php\php.exe" "%~dp0\utils\update_version_current.php" %* > "%~dp0\update_version_current.log"
ECHO UPDATE_VERSION_CURRENT_END