# PowerShell script to setup Kuzu on Windows

# Configuration - hardcode your architecture here
$arch = "win-x64"  # Change this to "win-x86" if needed

function Copy-FileIfNotExists {
    param([string]$Source, [string]$DestDir)

    if (!(Test-Path $DestDir)) {
        New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
    }

    $fileName = Split-Path $Source -Leaf
    $destPath = Join-Path $DestDir $fileName

    if (Test-Path $destPath) {
        Write-Host "File '$fileName' already exists in '$DestDir'. Skipping."
        return
    }

    Copy-Item -Path $Source -Destination $destPath
    Write-Host "File '$fileName' copied to '$DestDir'."
}

function Add-ToPath {
    param([string]$Path)

    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")

    if ($currentPath -notlike "*$Path*") {
        $newPath = if ($currentPath) { "$currentPath;$Path" } else { $Path }
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
        $env:PATH = $newPath
        Write-Host "Added to PATH: $Path"
    } else {
        Write-Host "Already in PATH: $Path"
    }
}

Write-Host "Using Architecture: $arch" -ForegroundColor Cyan

# Define paths
$projects = @("deeplynx.tests", "deeplynx.api", "deeplynx.graph")
$paths = @{}

foreach ($project in $projects) {
    $path = "$project\bin\Debug\net10.0\runtimes\$arch\native"

    if (!(Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        Write-Host "Created: $path"
    }

    $paths[$project] = (Resolve-Path $path).Path
    Write-Host "$project path: $($paths[$project])"
}

# Define Kuzu library files
$libkuzunetFile = "deeplynx.graph\KuzuFiles\kuzunet.dll"
$libkuzuFile = "deeplynx.graph\KuzuFiles\kuzu_shared.dll"

# Check if files exist
$filesToCopy = @()

if (Test-Path $libkuzunetFile) {
    $filesToCopy += $libkuzunetFile
    Write-Host "Found: $libkuzunetFile" -ForegroundColor Green
} else {
    Write-Host "Warning: Cannot find $libkuzunetFile" -ForegroundColor Yellow
}

if (Test-Path $libkuzuFile) {
    $filesToCopy += $libkuzuFile
    Write-Host "Found: $libkuzuFile" -ForegroundColor Green
} else {
    Write-Host "Warning: Cannot find $libkuzuFile" -ForegroundColor Yellow
}

# Copy files to each project
foreach ($file in $filesToCopy) {
    foreach ($project in $projects) {
        Copy-FileIfNotExists -Source $file -DestDir $paths[$project]
    }
}

# Add to PATH
Add-ToPath $paths["deeplynx.tests"]
Add-ToPath $paths["deeplynx.api"]

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Set ENABLE_KUZU=True in .env_sample"
Write-Host "2. Restart your terminal/IDE for PATH changes"
Write-Host "3. Run KuzuDatabaseManagerTests"
