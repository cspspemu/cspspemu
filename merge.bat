@ECHO OFF
IF "%1"=="Debug" GOTO END
PUSHD %~dp0
	REM SET BASE_FOLDER=CSPspEmu.Sandbox\bin\Debug
	SET BASE_FOLDER=%~dp0\CSPspEmu.Sandbox\bin\Release
	SET FILES=
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Sandbox.exe"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSharpUtils.Drawing.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Audio.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Audio.Impl.Openal.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Audio.Impl.WaveOut.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Cpu.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Media.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Gpu.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Gpu.Impl.Opengl.dll"
	REM SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Core.Tests.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Gui.Winforms.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Hle.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Hle.Modules.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Runner.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\SafeILGenerator.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\CSPspEmu.Resources.dll"
	SET FILES=%FILES% "%BASE_FOLDER%\OpenTK.dll"

	SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
	"%~dp0\utils\ilmerge\ILMerge.exe" %TARGET% /out:cspspemu.exe %FILES%
	COPY %BASE_FOLDER%\OpenTK.dll .
POPD
:END