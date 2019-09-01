
@echo off
rem パス定義
set compiler=%SYSTEMROOT%\Microsoft.NET\Framework\v2.0.50727\csc.exe
set srcpath=%1
set srcname=%~n1
set srcext=%~x1
set binext=.exe
if "%srcname%"=="" call :cErr "ソースファイルをドロップ"

rem 処理実行
cd /d %~dp1
echo.コンパイラ [%srcname%]
echo. %compiler% /nologo /out:"%srcname%%binext%" %srcpath%
%compiler% /nologo /out:"%srcname%%binext%" %srcpath%
echo.終了コード [%errorlevel%]
goto cEnd

rem エラー表示
:cErr 
echo.%~1
goto cEnd

:cEnd
pause
