
function Copy-File {
    param (
        [string]$sourceFilePath,
        [string]$destinationDirectory
    )

    if (-not (Test-Path -Path $destinationDirectory)) {
        New-Item -ItemType Directory -Path $destinationDirectory
    }

    $fileName = [System.IO.Path]::GetFileName($sourceFilePath)
    $destinationFilePath = Join-Path -Path $destinationDirectory -ChildPath $fileName

    if (Test-Path -Path $destinationFilePath) {
        Write-Output "File '$fileName' already exists in '$destinationDirectory'. Skipping copy."
        return
    fi
    }

    Copy-Item -Path $sourceFilePath -Destination $destinationFilePath
    Write-Output "File '$fileName' copied to '$destinationDirectory'."
}

function Add-ToProfile {
    param (
        [string]$variable,
        [string]$value
    )

    $profilePath = "$HOME\Documents\WindowsPowerShell\profile.ps1"
    $lineToAdd = "`${variable}=`"$value`${variable}`""

    if (Test-Path -Path $profilePath) {
        $profileContent = Get-Content -Path $profilePath
        if (-not ($profileContent -contains $lineToAdd)) {
            Add-Content -Path $profilePath -Value $lineToAdd
            Write-Output "Added '$lineToAdd' to profile.ps1."
        } else {
            Write-Output "The line '$lineToAdd' already exists in profile.ps1."
        }
    } else {
        New-Item -ItemType File -Path $profilePath -Force
        Add-Content -Path $profilePath -Value $lineToAdd
        Write-Output "Created profile.ps1 and added '$lineToAdd'."
    }
}

function Delete-Directory {
    param (
        [string]$targetDir
    )

    if (Test-Path -Path $targetDir) {
        Remove-Item -Path $targetDir -Recurse -Force
        Write-Output "Directory '$targetDir' deleted successfully."
    } else {
        Write-Output "Directory '$targetDir' does not exist."
    }
}

$libraryPath = "deeplynx.tests\bin\Debug\net10.0\runtimes\osx\native"

$absolutePath = [System.IO.Path]::GetFullPath($libraryPath)

Write-Output "Absolute Path: $absolutePath"

$kuzuFilesDirectory = "deeplynx.graph\KuzuFiles"
$libkuzunetFilePath = "deeplynx.graph\KuzuFiles\libkuzunet.dylib"
$libkuzuFilePath = "deeplynx.graph\KuzuFiles\libkuzu.dylib"
$destinationDirectory = $libraryPath

Copy-File -sourceFilePath $libkuzunetFilePath -destinationDirectory $destinationDirectory
Copy-File -sourceFilePath $libkuzuFilePath -destinationDirectory $destinationDirectory

Delete-Directory -targetDir $kuzuFilesDirectory

Add-ToProfile -variable "DYLD_LIBRARY_PATH" -value $absolutePath

$env:DYLD_LIBRARY_PATH = "$absolutePath;$env:DYLD_LIBRARY_PATH"
Write-Output "DYLD_LIBRARY_PATH set to: $env:DYLD_LIBRARY_PATH"