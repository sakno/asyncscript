IF %PROCESSOR_ARCHITECTURE% EQU x86 (SET SDK="%PROGRAMFILES%\Microsoft SDKs\Windows\v7.0A\bin") ELSE (SET SDK="%PROGRAMFILES(x86)%\Microsoft SDKs\Windows\v7.0A\bin")


%SDK%\sn -p DynamicScript.Core\public.snk PublicKey

%SDK%\sn -tp PublicKey
