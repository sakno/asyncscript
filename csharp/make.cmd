CALL sysvars.cmd
%MSBUILD% src\interpreter\Interpreter.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=%1 /t:Clean,Build