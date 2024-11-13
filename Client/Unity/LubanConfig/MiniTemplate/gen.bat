set WORKSPACE=..\..
set LUBAN_DLL=%WORKSPACE%\LubanConfig\MiniTemplate\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -d json ^
    -c cs-simple-json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%WORKSPACE%\Assets\GameConfig\Scripts\Configs\Luban\Gen\Code ^
    -x outputDataDir=%WORKSPACE%\Assets\AssetBundles\Configs\ResCfgs\Luban\Gen\Data

pause