cd /d "Engine\Build\BatchFiles"
RunUAT.bat BuildCookRun -project=Z:\ProjectName\Game\ProjectName\ProjectName.uproject -platform=Win64 -configuration=Development -build -cook -iterate -pak -stage -client -server
cd ..\..\..
