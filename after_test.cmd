nuget install OpenCover -Version 4.6.519 -OutputDirectory tools
nuget install coveralls.net -Version 0.7.0 -OutputDirectory tools

.\tools\coveralls.net.0.7.0\tools\csmacnz.Coveralls.exe --opencover -i .\results.xml