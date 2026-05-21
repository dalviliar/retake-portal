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

# Триггер: каждые 15 минут, 16 часов в сутки (06:00–22:00)
$trigger = New-ScheduledTaskTrigger `
    -Once `
    -At                 "06:00" `
    -RepetitionInterval (New-TimeSpan -Minutes 15) `
    -RepetitionDuration (New-TimeSpan -Hours 16)

# Настройки
$settings = New-ScheduledTaskSettingsSet `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 10) `
    -MultipleInstances  IgnoreNew `
    -StartWhenAvailable

# Регистрация — запускается от имени доменного пользователя без активной сессии
Register-ScheduledTask `
    -TaskName    $taskName `
    -Description "Синхронизация студентов, оценок и расписания из SSO (KazNITU) в Supabase" `
    -Action      $action `
    -Trigger     $trigger `
    -Settings    $settings `
    -RunLevel    Highest `
    -User        "KAZNITU\s.berdibekov" `
    -Password    "1Dalvi12909891*" `
    -Force

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
