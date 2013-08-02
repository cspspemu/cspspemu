@ECHO OFF
ECHO MERGE
PUSHD %~dp0
	SET BASE_FOLDER=%~dp0\CSPspEmu\bin\Release
	SET FILES=
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.exe"

	"%~dp0\utils\php\php.exe" -r"file_put_contents('__files.txt', getenv('FILES') . ' ' . implode(' ', array_map(function($a) { return '\"' . basename($a) . '\"'; }, glob(getenv('BASE_FOLDER') . '\\*.dll'))));"
	SET /p FILES=<__files.txt
	DEL /Q __files.txt 2> NUL
	
	ECHO %FILES%
	ECHO ---

	SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	DEL cspspemu_release.exe 2> NUL
	"%~dp0\utils\ilmerge\ILMerge.exe" /lib:"%BASE_FOLDER%" %TARGET% /out:cspspemu_release.exe %FILES%
	COPY %BASE_FOLDER%\OpenTK.dll .
POPD
