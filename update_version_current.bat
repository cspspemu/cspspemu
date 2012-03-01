@ECHO OFF
ECHO UPDATE_VERSION_CURRENT
"%~dp0\pspautotests\utils\win32\php.exe" "%~dp0\utils\update_version_current.php" %*
ECHO UPDATE_VERSION_CURRENT_END