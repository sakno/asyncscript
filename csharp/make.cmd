CALL sysvars.cmd
%MSBUILD% src\DynamicScript.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Clean,Build