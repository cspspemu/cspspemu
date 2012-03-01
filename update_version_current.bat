@ECHO OFF
"%~dp0\pspautotests\utils\win32\php.exe" -r"date_default_timezone_set('europe/madrid');file_put_contents(__DIR__ . '/version_current.txt', date('Ymd') . '%1');"