# =====================================================
# Регистрация задачи Windows Task Scheduler
# Запускать один раз (скрипт сам запросит права админа)
# =====================================================

# Автоматически перезапустить от имени Администратора
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process powershell.exe -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$taskName   = "RetakePortal - Sync Grades"
$scriptPath = Join-Path $PSScriptRoot "sync_grades.ps1"

# Действие
$action = New-ScheduledTaskAction `
    -Execute  "powershell.exe" `
    -Argument "-NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`""

# Удалить старую задачу если есть
schtasks /delete /tn $taskName /f 2>$null | Out-Null

# Регистрация через schtasks.exe — каждые 15 минут с 06:00 до 22:00 каждый день
$psExe    = "$env:SystemRoot\system32\WindowsPowerShell\v1.0\powershell.exe"
$taskArgs = "-NonInteractive -ExecutionPolicy Bypass -File `"$scriptPath`""

schtasks /create `
    /tn  $taskName `
    /tr  "`"$psExe`" $taskArgs" `
    /sc  DAILY `
    /st  "06:00" `
    /et  "22:00" `
    /ri  15 `
    /du  "0016:00" `
    /ru  "KAZNITU\s.berdibekov" `
    /rp  "1Dalvi12909891*" `
    /rl  HIGHEST `
    /f

Write-Host ""
Write-Host "Задача '$taskName' зарегистрирована." -ForegroundColor Green
Write-Host "Расписание: каждые 15 минут с 06:00 до 22:00" -ForegroundColor Cyan
Write-Host "Лог: $(Join-Path $PSScriptRoot 'sync.log')" -ForegroundColor Cyan
Write-Host ""
Write-Host "Управление:" -ForegroundColor Yellow
Write-Host "  Запустить вручную : Start-ScheduledTask  -TaskName '$taskName'"
Write-Host "  Остановить        : Stop-ScheduledTask   -TaskName '$taskName'"
Write-Host "  Удалить           : Unregister-ScheduledTask -TaskName '$taskName'"
Write-Host "  Статус            : Get-ScheduledTaskInfo -TaskName '$taskName'"
Write-Host ""
pause
