@echo off
echo Ejecutando tests y recolectando cobertura...
dotnet test SeatingMS.Tests --collect:"XPlat Code Coverage" --settings coverlet.runsettings

echo.
echo Generando reporte de cobertura...
reportgenerator -reports:"SeatingMS.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary

echo.
echo Reporte generado en coveragereport\Summary.txt
type coveragereport\Summary.txt
pause
