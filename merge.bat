@ECHO OFF
ECHO MERGE
IF "%1"=="Debug" GOTO SKIP_DEBUG
IF "%1"=="Trace" GOTO SKIP_DEBUG
PUSHD %~dp0
	REM SET BASE_FOLDER=CSPspEmu\bin\Debug
	SET BASE_FOLDER=%~dp0\CSPspEmu\bin\Release
	SET FILES=
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.exe"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.Drawing.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Utils.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Crypto.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Audio.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Cpu.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Media.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Gpu.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Resources.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Gui.Winforms.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Gui.Winforms.Controls.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Hle.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Hle.Modules.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Runner.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\SafeILGenerator.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\OpenTK.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\GLES.dll"

	SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	"%~dp0\utils\ilmerge\ILMerge.exe" %TARGET% /out:cspspemu.exe %FILES%
	COPY %BASE_FOLDER%\OpenTK.dll .
POPD
GOTO END
:SKIP_DEBUG
echo SKIP_DEBUG
:END