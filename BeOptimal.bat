@echo off

REM Set Default Variables
set UploadDir=Z:\ProjectName\Game\PerfTests
set ConfigDir=Z:\ProjectName\Game\PerfTests\Config.ini
set BuildDir=Z:\ProjectName\Game\ProjectName\Saved\StagedBuilds
set ProjectDir=Z:\ProjectName\Game\ProjectName\ProjectName.uproject
for /f "delims=[] tokens=2" %%a in ('ping -4 -n 1 %ComputerName% ^| findstr [') do set NetworkIP=%%a 

REM Change directory to access RunUAT
cd /d "Engine\Build\BatchFiles"

REM Check if the number of arguments is at least 1
if "%~1"=="" (
    echo "Usage: BeOptimal.bat [RunTest|ListTests|Compare|GenerateConfig|Database] [..] [..]"    
    cd ..\..\..
    exit /b 1
)

set TestType=%1
shift

if /I "%TestType%"=="RunTest" ( 
    if "%~1"=="" (
        echo "Usage: BeOptimal.bat RunTest [TestName] [MapName]"
        echo "Optional Flag: -ignoreComparisonPass"
	    cd ..\..\..
        exit /b 1
    )

    if "%~2"=="" (
        echo "Usage: BeOptimal.bat RunTest [TestName] [MapName]"
        echo "Optional Flag: -ignoreComparisonPass"
	    cd ..\..\..
        exit /b 1
    )
    
    if "%~3"=="" (
    	REM Start UAT for RunTest
    	echo Running performance test %1 on map %2
    	RunUAT RunUnreal -project=%ProjectDir% -targetPlatform=Win64 -configuration=Development -configdir=%ConfigDir% -test=RunTest -build=%BuildDir% -uploaddir=%UploadDir% -runtest=%1 -testmap=%2 -server -IP=%NetworkIP% | findstr /R /C:"# - "
    
    ) else if "%~3"=="-ignoreComparisonPass" (
        REM Start UAT for RunTest
    	echo Running performance test %1 on map %2
    	RunUAT RunUnreal -project=%ProjectDir% -targetPlatform=Win64 -configuration=Development -configdir=%ConfigDir% -test=RunTest -build=%BuildDir% -uploaddir=%UploadDir% -runtest=%1 -testmap=%2 -server -ignoreComparisonPass -IP=%NetworkIP% | findstr /R /C:"# - "
    )


) else if /I "%TestType%"=="ListTests" (
    if "%~1"=="" (
        echo "Usage: BeOptimal.bat ListTests [MapName]"
	    cd ..\..\..
        exit /b 1
    )
    
    REM Start UAT for ListTests
    echo Listing tests for map %1
    RunUAT RunUnreal -project=%ProjectDir% -targetPlatform=Win64 -configuration=Development -test=ListTests -build=%BuildDir% -testmap=%1 | findstr /R /C:"# - "

) else if /I "%TestType%"=="Compare" (
    if "%~1"=="" (
        echo "Usage: BeOptimal.bat Compare [ClientFileNew] [ClientFileOld] [ServerFileNew] [ServerFileOld]"
	    cd ..\..\..
        exit /b 1
    )

    if "%~2"=="" (
        echo "Usage: BeOptimal.bat Compare [ClientFileNew] [ClientFileOld] [ServerFileNew] [ServerFileOld]"
	    cd ..\..\..
        exit /b 1
    )

    if "%~3"=="" (
        echo "Usage: BeOptimal.bat Compare [ClientFileNew] [ClientFileOld] [ServerFileNew] [ServerFileOld]"
	    cd ..\..\..
        exit /b 1
    )

    if "%~4"=="" (
        echo "Usage: BeOptimal.bat Compare [ClientFileNew] [ClientFileOld] [ServerFileNew] [ServerFileOld]"
	    cd ..\..\..
        exit /b 1
    )
    
    REM Start UAT for Compare
    echo Comparison initalizing...
    RunUAT RunUnreal -project=%ProjectDir% -targetPlatform=Win64 -configdir=%ConfigDir% -configuration=Development -test=Compare -build=%BuildDir% -newClientInputFile=%1 -oldClientInputFile=%2 -newServerInputFile=%3 -oldServerInputFile=%4 | findstr /R /C:"# - "

) else if /I "%TestType%"=="GenerateConfig" (
    REM Start UAT for GenerateConfig
    echo Generating config at %ConfigDir%
    RunUAT RunUnreal -project=%ProjectDir% -configdir=%ConfigDir% -genconfig -targetPlatform=Win64 -configuration=Development -test=Compare -build=%BuildDir% | findstr /R /C:"# - "
	
) else if /I "%TestType%"=="Database" (
    if "%~1"=="" (
        echo "Usage: BeOptimal.bat Database [Save|Remove] [..]"
	    cd ..\..\..
        exit /b 1
    )

    if "%~1"=="Remove" (

	    if "%~2"=="" (
        	echo "Usage: BeOptimal.bat Remove [TestID]"
		    cd ..\..\..
        	exit /b 1
    	) 

	    REM Removing testID
    	echo Removing testID %2
    	RunUAT Database -remove -testID=%2 | findstr /R /C:"# - "

    ) else if "%~1"=="Save" (
    	if "%~2"=="" (
        	echo "Usage: BeOptimal.bat Save [ClientRaw] [ServerRaw] [ClientPostProcessed] [ServerPostProcessed]"
		    cd ..\..\..
        	exit /b 1
    	)

    	if "%~3"=="" (
        	echo "Usage: BeOptimal.bat Save [ClientRaw] [ServerRaw] [ClientPostProcessed] [ServerPostProcessed]"
		    cd ..\..\..
        	exit /b 1
    	)

    	if "%~4"=="" (
        	echo "Usage: BeOptimal.bat Save [ClientRaw] [ServerRaw] [ClientPostProcessed] [ServerPostProcessed]"
		    cd ..\..\..
        	exit /b 1
    	)
	    if "%~5"=="" (
        	echo "Usage: BeOptimal.bat Save [ClientRaw] [ServerRaw] [ClientPostProcessed] [ServerPostProcessed]"
		    cd ..\..\..
        	exit /b 1
    	)

	    REM Saving files to DB
    	echo Saving files to DB
    	RunUAT Database -save -clientRawFile=%2 -serverRawFile=%3 -clientPostProcessedFile=%4 -serverPostProcessedFile=%5 | findstr /R /C:"# - "
    )
) else (
    echo Unknown TestType: %TestType%
    echo "Usage: BeOptimal.bat [RunTest|ListTests|Compare|GenerateConfig|Database] [..] [..]"
    cd ..\..\..
    exit /b 1
)

cd ..\..\..
exit /b 0