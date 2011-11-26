@ECHO OFF
SET FILES=
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Sandbox.exe
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSharpUtils.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSharpUtils.Drawing.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.Audio.Imple.Openal.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.Cpu.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.Gpu.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.Gpu.Impl.Opengl.dll
REM SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Core.Tests.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Gui.Winforms.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Hle.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Hle.Modules.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\CSPspEmu.Runner.dll
SET FILES=%FILES% CSPspEmu.Sandbox\bin\Debug\OpenTK.dll

SET TARGET=/targetplatform:v4,"%ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
"C:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" %TARGET% /out:cspspemu.exe %FILES%