



mkdir build.windows
del /s/q build.windows\*.* 
if %errorlevel% neq 0 exit /b %errorlevel%


pushd build.windows
cmake -G "Visual Studio 14 Win64" ..
if %errorlevel% neq 0 popd & exit /b %errorlevel%

rem /verbosity:<level> Display this amount of information in the event log. The available verbosity levels are: q[uiet], m[inimal],n[ormal], d[etailed], and diag[nostic]. (Short form: /v)                  Example:                     /verbosity:quiet

rem choose one of the two following lines
rem msbuild limevideosdknative.sln /p:Configuration=Release /p:Platform=x64
msbuild limevideosdknative.sln  /p:Configuration=Release /p:Platform=x64 /m:1 /filelogger /v:n
if %errorlevel% neq 0 popd & exit /b %errorlevel%

popd
