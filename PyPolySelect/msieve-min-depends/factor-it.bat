@echo off

rem "c:\program files\python34\python" %1
rem python factmsieve.py job-rsa-200/rsa200.n

setlocal EnableDelayedExpansion
set "arg1="
call set "arg1=%%1"

if defined arg1 goto :arg_exists


echo arg1 is missing
echo Error: No argument provided.
echo Usage: factor-it.bat <filename>
goto :done

:arg_exists


REM Set the input filename and directory name
set INPUT_FILE=%1
set DIRECTORY_NAME=%~n1

echo INPUT_FILE: %INPUT_FILE% 
echo DIRECTORY_NAME: %DIRECTORY_NAME% 

REM Check if the input file exists
if not exist %INPUT_FILE% (
    echo Error: Input file '%INPUT_FILE%' not found.
	goto :done
)

if not exist %DIRECTORY_NAME% (
	mkdir %DIRECTORY_NAME%
)

REM Create the directory


REM Copy the input file to the directory
copy %INPUT_FILE% %DIRECTORY_NAME%

REM Execute the command
python factmsieve-0.86.py %DIRECTORY_NAME%\%INPUT_FILE%

REM Optional: Pause at the end to see the output
rem pause

:done