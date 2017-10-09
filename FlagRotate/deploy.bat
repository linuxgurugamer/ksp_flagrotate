
@echo off
set H=R:\KSP_1.3.1_dev
echo %H%

set d=%H%
if exist %d% goto one
mkdir %d%
:one
set d=%H%\Gamedata
if exist %d% goto two
mkdir %d%
:two
set d=%H%\Gamedata\FlagRotate
if exist %d% goto three
mkdir %d%
:three
set d=%H%\Gamedata\FlagRotate\Plugins
if exist %d% goto four
mkdir %d%
:four


copy bin\Debug\FlagRotate.dll ..\GameData\FlagRotate\Plugins


xcopy /y /s "..\GameData\FlagRotate" "%H%\GameData\FlagRotate"
