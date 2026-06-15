$ErrorActionPreference = 'Stop'
$installDir = Join-Path $env:LOCALAPPDATA 'IRAS SPOT POS'
$zipPath = Join-Path $PSScriptRoot 'app.zip'

if (Test-Path $installDir) { Remove-Item -LiteralPath $installDir -Recurse -Force }
New-Item -ItemType Directory -Path $installDir | Out-Null
Expand-Archive -LiteralPath $zipPath -DestinationPath $installDir -Force

$exePath = Join-Path $installDir 'POSales.exe'
$iconPath = Join-Path $installDir 'app_logo.ico'
$reportsDir = Join-Path $installDir 'Reports'
$requiredReports = @('rptCancelled.rdlc','rptInventory.rdlc','rptRecept.rdlc','rptSoldItems.rdlc','rptSoldReport.rdlc','rptStockInHist.rdlc','rptTopSell.rdlc')
foreach ($report in $requiredReports) {
    if (!(Test-Path (Join-Path $reportsDir $report))) { throw "Missing report file: $report" }
}
if (!(Test-Path $iconPath)) { throw "Missing app logo icon." }

$wsh = New-Object -ComObject WScript.Shell
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'IRAS SPOT POS.lnk'
if (Test-Path $desktopShortcut) { Remove-Item -LiteralPath $desktopShortcut -Force }
$shortcut = $wsh.CreateShortcut($desktopShortcut)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $installDir
$shortcut.IconLocation = $iconPath
$shortcut.Description = 'IRAS SPOT POS'
$shortcut.Save()

$programs = [Environment]::GetFolderPath('Programs')
$startDir = Join-Path $programs 'IRAS SPOT POS'
if (Test-Path $startDir) { Remove-Item -LiteralPath $startDir -Recurse -Force }
New-Item -ItemType Directory -Path $startDir -Force | Out-Null
$startShortcut = Join-Path $startDir 'IRAS SPOT POS.lnk'
$shortcut = $wsh.CreateShortcut($startShortcut)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $installDir
$shortcut.IconLocation = $iconPath
$shortcut.Description = 'IRAS SPOT POS'
$shortcut.Save()

Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.MessageBox]::Show("IRAS SPOT POS has been installed with the logo icon and all report files.", "Installation complete", 'OK', 'Information') | Out-Null
