@ECHO OFF

SET XTX="W:\Forza\Tools\XTX\XTX.exe"

FOR /R %%G IN (*.xds) DO (
	PUSHD "%%~dpG"
	%XTX% "%%G"
	POPD
)
