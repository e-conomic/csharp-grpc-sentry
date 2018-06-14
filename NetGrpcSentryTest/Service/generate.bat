setlocal

set PROTO_TOOLS=%userprofile%\.nuget\packages\Google.Protobuf.Tools\3.5.1\tools\windows_x64\protoc.exe
set PLUGIN_TOOLS=%userprofile%\.nuget\packages\Grpc.Tools\1.8.3\tools\windows_x64\grpc_csharp_plugin.exe
set PROTO_FOLDER=.
set PROJECT_FOLDER=.
set PROTO_FILE_NAME=protofile

 %PROTO_TOOLS% -I%PROTO_FOLDER% --csharp_out %PROJECT_FOLDER%  %PROTO_FOLDER%\%PROTO_FILE_NAME%.proto --grpc_out %PROJECT_FOLDER% --plugin=protoc-gen-grpc=%PLUGIN_TOOLS%

+endlocal 