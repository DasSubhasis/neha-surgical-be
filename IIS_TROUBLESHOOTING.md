# IIS Deployment Checklist

## Current Issue: 404 Error on https://nehaomapi.zicorpcloud.in/swagger/index.html

## Fixed in Latest Publish:
✅ Swagger now enabled in all environments
✅ Environment set to Development for better error messages
✅ Self-contained deployment (103 MB)

## IIS Server Checklist:

### 1. Check Application Pool
```powershell
# On IIS server, run in PowerShell as Administrator:
Import-Module WebAdministration
Get-WebAppPoolState -Name "YourAppPoolName"
# Should show: Started
```

### 2. Check stdout Logs
Location: `{publish-folder}\logs\stdout_{timestamp}.log`
- Create `logs` folder in publish directory if missing
- Check for startup errors

### 3. Check Event Viewer
- Windows Logs → Application
- Look for errors from IIS AspNetCore Module

### 4. Verify web.config in IIS
Check that `web.config` in publish folder has:
```xml
<aspNetCore processPath="dotnet" 
            arguments=".\NehaSurgicalAPI.dll"
```

### 5. Test Basic Connectivity
Try these URLs on the server:
- http://localhost/swagger
- http://localhost/api/doctors (or any API endpoint)

### 6. Check Permissions
Application pool identity needs:
- Read access to publish folder
- Execute access to NehaSurgicalAPI.exe

### 7. Common Fixes

**If 404 persists:**
```powershell
# Restart app pool
Restart-WebAppPool -Name "YourAppPoolName"

# Recycle
Stop-WebAppPool -Name "YourAppPoolName"
Start-WebAppPool -Name "YourAppPoolName"
```

**Enable detailed errors temporarily:**
In web.config, add inside `<system.webServer>`:
```xml
<httpErrors errorMode="Detailed" />
```
(Already added in current web.config)

**Check if .NET is running:**
```powershell
# In publish folder:
.\NehaSurgicalAPI.exe
# Should start the API - test locally first
```

### 8. Database Connection
Verify connection string in `appsettings.json`:
- PostgreSQL server is accessible from IIS server
- Firewall allows PostgreSQL port (default 5432)
- Connection string credentials are correct

### 9. HTTPS Binding
If using HTTPS:
- SSL certificate installed
- IIS binding configured for 443
- Update Program.cs if not using HTTPS redirection

### 10. Check These Files Exist in Publish:
- ✅ NehaSurgicalAPI.exe
- ✅ NehaSurgicalAPI.dll
- ✅ web.config
- ✅ appsettings.json
- ✅ wwwroot folder

## Next Steps:
1. Create `logs` folder in publish directory
2. Copy updated publish folder to IIS server
3. Restart application pool
4. Check stdout logs for errors
5. Check Event Viewer if still failing
