@echo off
echo Cleaning previous results...
if exist BookingMS.Tests\TestResults rmdir /s /q BookingMS.Tests\TestResults
if exist coveragereport rmdir /s /q coveragereport

echo Running tests with coverage...
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

echo Generating report...
reportgenerator -reports:"BookingMS.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary

echo Done. Report generated in coveragereport\Summary.txt
pause
