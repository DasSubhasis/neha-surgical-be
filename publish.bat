@echo off
echo Cleaning old publish folder...
if exist publish rmdir /s /q publish

echo Publishing application...
dotnet publish NehaSurgicalAPI.csproj -c Release -o ./publish --self-contained true -r win-x64

echo Creating logs folder...
if not exist publish\logs mkdir publish\logs

echo.
echo ============================================
echo   Publish Complete!
echo   Location: %cd%\publish
echo ============================================
echo.
pause
