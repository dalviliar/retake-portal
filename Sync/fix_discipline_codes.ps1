# Fix NULL discipline_code in Supabase grades table
# Run inside university network (needs SQL Server access)

$SQL_SERVER   = "192.168.12.104"
$SQL_DB       = "KazNITU"
$SEMESTER_ID  = 83
$SUPABASE_URL = "https://hkfyeivihhmvwyhvcpsq.supabase.co"
$SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhrZnllaXZpaGhtdnd5aHZjcHNxIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc3ODA2NDA4MSwiZXhwIjoyMDkzNjQwMDgxfQ.iOoSGPaep8_sSTA7B4JlJuBaHDmmdnU4kNCcb156N5E"

$mappingQuery = "SELECT DISTINCT CAST(smc.Title AS NVARCHAR(500)) AS discipline_name, CAST(smc.Code AS NVARCHAR(50)) AS discipline_code, ISNULL(CAST(ou.Title AS NVARCHAR(255)),'') AS discipline_department FROM Edu_SemesterCourses smc LEFT JOIN Edu_OrgUnits ou ON ou.ID = smc.OrgUnitID WHERE smc.SemesterID = $SEMESTER_ID AND smc.Code IS NOT NULL"

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Loading discipline codes from SSO..." -ForegroundColor Cyan
$mappings = Invoke-Sqlcmd -ServerInstance $SQL_SERVER -Database $SQL_DB -Query $mappingQuery -ErrorAction Stop
Write-Host "  Found: $($mappings.Count) disciplines with codes" -ForegroundColor Green

$headers = @{
    "apikey"        = $SUPABASE_KEY
    "Authorization" = "Bearer $SUPABASE_KEY"
    "Content-Type"  = "application/json; charset=utf-8"
    "Prefer"        = "return=minimal"
}

$updated = 0
$errors  = 0

foreach ($m in $mappings) {
    $name = ([string]$m.discipline_name).Trim()
    $code = ([string]$m.discipline_code).Trim()
    $dept = ([string]$m.discipline_department).Trim()

    if ([string]::IsNullOrEmpty($name) -or [string]::IsNullOrEmpty($code)) { continue }

    $encodedName = [Uri]::EscapeDataString($name)
    $url = "$SUPABASE_URL/rest/v1/grades?discipline_name=eq.$encodedName&discipline_code=is.null"

    $bodyObj = [PSCustomObject]@{ discipline_code = $code; discipline_department = $dept }
    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes(($bodyObj | ConvertTo-Json -Compress))

    try {
        Invoke-RestMethod -Uri $url -Method PATCH -Headers $headers -Body $bodyBytes | Out-Null
        $updated++
    } catch {
        Write-Host "  [ERROR] $name : $_" -ForegroundColor Red
        $errors++
    }
}

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Done. Updated: $updated, errors: $errors" -ForegroundColor Green
