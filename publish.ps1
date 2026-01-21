# Clean and publish script
Write-Host "Cleaning old publish folder..." -ForegroundColor Yellow
Remove-Item -Path publish -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Publishing application..." -ForegroundColor Cyan
dotnet publish NehaSurgicalAPI.csproj -c Release -o ./publish --self-contained true -r win-x64

Write-Host "Creating logs folder..." -ForegroundColor Cyan
New-Item -Path "publish\logs" -ItemType Directory -Force | Out-Null

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Publish Complete!" -ForegroundColor Green
Write-Host "  Location: $PWD\publish" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
