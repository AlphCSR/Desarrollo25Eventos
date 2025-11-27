@echo off
echo Ejecutando tests y recolectando cobertura...
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

echo.
echo Generando reporte de cobertura...
reportgenerator -reports:"UsersMS.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary

echo.
echo Reporte generado en UsersMS.Tests\coveragereport\UserSummary.txt
type UsersMS.Tests\coveragereport\UserSummary.txt
pause
