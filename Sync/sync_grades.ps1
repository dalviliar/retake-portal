# =====================================================
# RetakePortal — Sync Script
# Запускать внутри сети университета
# Расписание: Планировщик задач Windows каждые 15 минут
#
# Требует модуль SqlServer:
#   Install-Module SqlServer -Scope CurrentUser
# =====================================================

# === НАСТРОЙКИ (заполните перед первым запуском) ===
$SQL_SERVER   = "192.168.12.104"
$SQL_DB       = "KazNITU"
$SEMESTER_ID  = 83
$SEMESTER     = "2025-2026/2"
$SUPABASE_URL = "https://hkfyeivihhmvwyhvcpsq.supabase.co"
$SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhrZnllaXZpaGhtdnd5aHZjcHNxIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc3ODA2NDA4MSwiZXhwIjoyMDkzNjQwMDgxfQ.iOoSGPaep8_sSTA7B4JlJuBaHDmmdnU4kNCcb156N5E"
# ====================================================

$studentsQuery = @"
SELECT
    CAST(u.IIN AS NVARCHAR(20)) AS iin,
    CAST(u.LastName + ' ' + u.FirstName + ' ' + ISNULL(u.MiddleName, '') AS NVARCHAR(255)) AS full_name,
    CAST(sp.Title   AS NVARCHAR(255)) AS specialty,
    CAST(inst.Title AS NVARCHAR(255)) AS institute,
    CAST(dept.Title AS NVARCHAR(255)) AS department,
    s.Year      AS course,
    CASE sp.LevelID
        WHEN 1 THEN 'bachelor'
        WHEN 2 THEN 'master_sci'
        WHEN 3 THEN 'doctoral'
        ELSE 'bachelor'
    END AS education_level
FROM Edu_Students s
JOIN Edu_Users u         ON u.ID   = s.StudentID
JOIN Edu_Specialities sp ON sp.ID  = s.SpecialityID
LEFT JOIN Edu_OrgUnits dept ON dept.ID = sp.RupEditorOrgUnitID
LEFT JOIN Edu_OrgUnits inst ON inst.ID = dept.ParentID
WHERE s.StatusID IS NOT NULL
  AND s.StatusID <> 2
"@

$gradesQuery = @"
SELECT
    CAST(u.IIN AS NVARCHAR(20))       AS student_iin,
    CAST(smc.Title AS NVARCHAR(500))  AS discipline_name,
    CAST(sc.LetterGrade AS NVARCHAR(2)) AS grade,
    CAST(smc.Credits AS INT)          AS credits,
    '$SEMESTER'                       AS semester
FROM Edu_StudentCourses sc
JOIN Edu_Students s          ON s.StudentID  = sc.StudentID
JOIN Edu_Users u             ON u.ID         = s.StudentID
JOIN Edu_SemesterCourses smc ON smc.ID       = sc.SemesterCourseID
WHERE smc.SemesterID = $SEMESTER_ID
  AND sc.LetterGrade IN ('FX', 'F', 'I')
  AND (
      sc.LetterGrade IN ('FX', 'I')
      OR (sc.LetterGrade = 'F' AND sc.ExamGrade > 0)
  )
  AND s.StatusID IS NOT NULL
  AND s.StatusID <> 2
"@

Add-Type -AssemblyName System.Web.Extensions
$script:serializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$script:serializer.MaxJsonLength = [int]::MaxValue

function Format-Field {
    param($val)
    if ($null -eq $val -or $val -is [System.DBNull]) { return "" }
    return ([string]$val).Trim() -replace '[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]', ''
}

function Push-ToSupabase {
    param([string]$Table, [array]$Rows, [string]$OnConflict = "")
    if ($Rows.Count -eq 0) { return }

    $url = "$SUPABASE_URL/rest/v1/$Table"
    if ($OnConflict) { $url += "?on_conflict=$OnConflict" }

    $headers = @{
        "apikey"        = $SUPABASE_KEY
        "Authorization" = "Bearer $SUPABASE_KEY"
        "Content-Type"  = "application/json; charset=utf-8"
        "Prefer"        = "resolution=merge-duplicates,return=minimal"
    }

    for ($i = 0; $i -lt $Rows.Count; $i += 500) {
        $batch = $Rows[$i..([Math]::Min($i + 499, $Rows.Count - 1))]
        $json  = $script:serializer.Serialize($batch)

        try {
            $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($json)
            Invoke-RestMethod -Uri $url -Method POST -Headers $headers -Body $bodyBytes | Out-Null
        } catch {
            Write-Host "  [ERROR] $Table batch $i : $_" -ForegroundColor Red
        }
    }
}

$scheduleQuery = @"
SELECT
    CAST(u.IIN AS NVARCHAR(20))        AS student_iin,
    CAST(smc.Title AS NVARCHAR(500))   AS discipline_name,
    '$SEMESTER'                        AS semester,
    CONVERT(NVARCHAR(10), exz.ExamDate, 23)                        AS exam_date,
    ISNULL(CONVERT(NVARCHAR(5), CAST(ts.Title AS time), 108), '')  AS start_time,
    ISNULL(CONVERT(NVARCHAR(5), CAST(te.Title AS time), 108), '')  AS end_time,
    ISNULL(CAST(aud.Title AS NVARCHAR(100)), '')                   AS room,
    ISNULL(CAST(krp.ShortTitle AS NVARCHAR(100)), '')              AS building
FROM Edu_SemesterCourseExamStudents estud
JOIN Edu_SemesterCourseExams exz   ON exz.ID  = estud.SemesterCourseExamID
JOIN Edu_SemesterCourses smc       ON smc.ID  = exz.SemesterCourseID
JOIN Edu_Students s                ON s.StudentID = estud.StudentID
JOIN Edu_Users u                   ON u.ID    = s.StudentID
LEFT JOIN Edu_Times ts             ON ts.ID   = exz.StartID
LEFT JOIN Edu_Times te             ON te.ID   = exz.EndID
LEFT JOIN Edu_Rooms aud            ON aud.ID  = exz.RoomID
LEFT JOIN Edu_Buildings krp        ON krp.ID  = aud.BuildingID
WHERE smc.SemesterID = $SEMESTER_ID
  AND exz.ExamDate IS NOT NULL
  AND exz.ExamDate >= CAST(GETDATE() AS DATE)
  AND u.IIN IS NOT NULL
"@

# ── ЗАПУСК ──────────────────────────────────────────
Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Sync started (semester ID=$SEMESTER_ID)" -ForegroundColor Cyan

# 1. Студенты
Write-Host "  Loading students from SSO..."
$students = Invoke-Sqlcmd -ServerInstance $SQL_SERVER -Database $SQL_DB `
    -Query $studentsQuery -ErrorAction Stop

$studentRows = $students | ForEach-Object {
    @{
        iin             = Format-Field $_.iin
        full_name       = Format-Field $_.full_name
        specialty       = Format-Field $_.specialty
        institute       = Format-Field $_.institute
        department      = Format-Field $_.department
        course          = [int]$_.course
        education_level = Format-Field $_.education_level
    }
}
Write-Host "  Studentov: $($studentRows.Count) - otpravlyayu v Supabase..."
Push-ToSupabase -Table "students" -Rows $studentRows

# 2. Оценки FX / реальный F
Write-Host "  Loading FX/F grades from SSO..."
$grades = Invoke-Sqlcmd -ServerInstance $SQL_SERVER -Database $SQL_DB `
    -Query $gradesQuery -ErrorAction Stop

$gradeRows = $grades | ForEach-Object {
    @{
        student_iin     = Format-Field $_.student_iin
        discipline_name = Format-Field $_.discipline_name
        grade           = Format-Field $_.grade
        credits         = [int]$_.credits
        semester        = Format-Field $_.semester
    }
}
Write-Host "  Otsenok: $($gradeRows.Count) - otpravlyayu v Supabase..."
Push-ToSupabase -Table "grades" -Rows $gradeRows -OnConflict "student_iin,discipline_name,semester"

# 3. Расписание пересдач
Write-Host "  Loading retake schedules from SSO..."
$schedules = Invoke-Sqlcmd -ServerInstance $SQL_SERVER -Database $SQL_DB `
    -Query $scheduleQuery -ErrorAction Stop

$scheduleRows = $schedules | ForEach-Object {
    @{
        student_iin     = Format-Field $_.student_iin
        discipline_name = Format-Field $_.discipline_name
        semester        = Format-Field $_.semester
        exam_date       = Format-Field $_.exam_date
        start_time      = Format-Field $_.start_time
        end_time        = Format-Field $_.end_time
        room            = Format-Field $_.room
        building        = Format-Field $_.building
    }
}
Write-Host "  Schedules: $($scheduleRows.Count) - otpravlyayu v Supabase..."
Push-ToSupabase -Table "retake_schedules" -Rows $scheduleRows -OnConflict "student_iin,discipline_name,semester"

Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Done!" -ForegroundColor Green
