
@echo off
rem �p�X��`
set compiler=%SYSTEMROOT%\Microsoft.NET\Framework\v2.0.50727\csc.exe
set srcpath=%1
set srcname=%~n1
set srcext=%~x1
set binext=.exe
if "%srcname%"=="" call :cErr "�\�[�X�t�@�C�����h���b�v"

rem �������s
cd /d %~dp1
echo.�R���p�C�� [%srcname%]
echo. %compiler% /nologo /out:"%srcname%%binext%" %srcpath%
%compiler% /nologo /out:"%srcname%%binext%" %srcpath%
echo.�I���R�[�h [%errorlevel%]
goto cEnd

rem �G���[�\��
:cErr 
echo.%~1
goto cEnd

:cEnd
pause
