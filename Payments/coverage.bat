@echo off
echo Limpiando resultados anteriores...
if exist PaymentsMS.Tests\TestResults rd /s /q PaymentsMS.Tests\TestResults
if exist coveragereport rd /s /q coveragereport

echo.
echo Ejecutando tests y recolectando cobertura para PaymentsMS...
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

echo.
echo Generando reporte de cobertura filtrado...
reportgenerator -reports:"PaymentsMS.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary

echo.
echo Reporte generado en coveragereport\Summary.txt
if exist coveragereport\Summary.txt type coveragereport\Summary.txt
pause
