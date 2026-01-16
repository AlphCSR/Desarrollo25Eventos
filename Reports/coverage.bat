@echo off
echo Limpiando resultados anteriores...
if exist ReportsMS.Tests\TestResults rd /s /q ReportsMS.Tests\TestResults
if exist coveragereport rd /s /q coveragereport

echo.
echo Ejecutando tests y recolectando cobertura para ReportsMS...
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

echo.
echo Generando reporte de cobertura filtrado...
reportgenerator -reports:"ReportsMS.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary

echo.
echo Reporte generado en coveragereport\Summary.txt
if exist coveragereport\Summary.txt type coveragereport\Summary.txt
pause
