$ErrorActionPreference = "Stop"

# Define paths
$binDebugDir = "C:\Users\USER\source\repos\POSales\POSales\POSales\bin\Debug"
$installerDir = "C:\Users\USER\source\repos\POSales\installer-build-compiled"
$tempPayloadDir = Join-Path $installerDir "payload_temp"
$zipPath = Join-Path $installerDir "app.zip"
$icoPath = Join-Path $binDebugDir "app_logo.ico"
$tempIcoPath = Join-Path $installerDir "app_logo.ico"
$setupExePath = Join-Path $installerDir "IRAS_SPOT_POS_Setup.exe"
$downloadsDir = "C:\Users\USER\Downloads"
$targetSetupPath = Join-Path $downloadsDir "IRAS_SPOT_POS_Setup.exe"

# Cleanup from previous runs
if (Test-Path $tempPayloadDir) { Remove-Item -Recurse -Force $tempPayloadDir }
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
if (Test-Path $tempIcoPath) { Remove-Item -Force $tempIcoPath }

Write-Host "Creating temporary payload folder..."
New-Item -ItemType Directory -Path $tempPayloadDir

Write-Host "Copying build output to payload..."
Copy-Item -Path (Join-Path $binDebugDir "*") -Destination $tempPayloadDir -Recurse -Force

# Remove development-only files
Get-ChildItem -Path $tempPayloadDir -Filter "*.pdb" -Recurse | Remove-Item -Force
if (Test-Path (Join-Path $tempPayloadDir "dbconnection_local.txt")) {
    Remove-Item (Join-Path $tempPayloadDir "dbconnection_local.txt") -Force
}

Write-Host "Zipping payload..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempPayloadDir, $zipPath)

# Copy icon for compilation
Copy-Item -Path $icoPath -Destination $tempIcoPath -Force

Write-Host "Compiling setup installer with csc.exe..."
$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$arguments = @(
    "/target:winexe",
    "/out:$setupExePath",
    "/reference:System.Windows.Forms.dll",
    "/reference:System.Drawing.dll",
    "/reference:System.IO.Compression.dll",
    "/reference:System.IO.Compression.FileSystem.dll",
    "/resource:$zipPath,app.zip",
    "/win32icon:$tempIcoPath",
    "Setup.cs"
)

# Run compiler
$process = Start-Process -FilePath $cscPath -ArgumentList $arguments -Wait -NoNewWindow -PassThru
if ($process.ExitCode -ne 0) {
    throw "csc.exe compilation failed with exit code $($process.ExitCode)"
}

Write-Host "Cleaning up temporary files..."
Remove-Item -Recurse -Force $tempPayloadDir
Remove-Item -Force $zipPath
Remove-Item -Force $tempIcoPath

Write-Host "Copying installer to Downloads folder..."
if (!(Test-Path $downloadsDir)) {
    New-Item -ItemType Directory -Path $downloadsDir
}
Copy-Item -Path $setupExePath -Destination $targetSetupPath -Force

Write-Host "Installer successfully built and copied to: $targetSetupPath"
