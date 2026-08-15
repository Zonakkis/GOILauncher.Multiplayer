param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("Any CPU")]
    [string]$Platform = "Any CPU",

    [switch]$SkipRestore,
    [switch]$Clean,
    [switch]$RunTests
)

$ErrorActionPreference = "Stop"

$solutionDir = Split-Path -Parent $PSScriptRoot
$solutionFile = Join-Path $solutionDir "GOILauncher.Multiplayer.sln"

# ---------- Helper ----------
function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }

# ---------- Locate MSBuild ----------
function Find-MSBuild {
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $installPath = & $vsWhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($installPath) {
            $msbuild = Join-Path $installPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuild) { return $msbuild }
        }
    }

    # Fallback: check PATH
    $onPath = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    return $null
}

# ---------- Locate NuGet ----------
function Find-NuGet {
    $onPath = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $nugetPath = Join-Path $solutionDir ".nuget\nuget.exe"
    if (Test-Path $nugetPath) { return $nugetPath }

    return $null
}

function Find-VSTest {
    $onPath = Get-Command vstest.console.exe -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $installPath = & $vsWhere -latest -products * -property installationPath
        if ($installPath) {
            $candidate = Join-Path $installPath "Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
            if (Test-Path $candidate) { return $candidate }
        }
    }

    return $null
}

# ---------- Build ----------
$msbuild = Find-MSBuild
if (-not $msbuild) {
    Write-Host "ERROR: MSBuild not found. Please run from a Developer PowerShell or install Visual Studio Build Tools." -ForegroundColor Red
    exit 1
}
Write-Host "MSBuild: $msbuild" -ForegroundColor DarkGray

# ---------- Restore packages ----------
if (-not $SkipRestore) {
    Write-Step "Restoring NuGet packages"
    $nuget = Find-NuGet
    if ($nuget) {
        Write-Host "NuGet: $nuget" -ForegroundColor DarkGray
        & $nuget restore $solutionFile
        if ($LASTEXITCODE -ne 0) { Write-Host "NuGet restore failed." -ForegroundColor Red; exit 1 }
    } else {
        Write-Host "nuget.exe not found, attempting MSBuild /t:Restore ..." -ForegroundColor Yellow
        & $msbuild $solutionFile /t:Restore /p:Configuration="$Configuration" /p:Platform="$Platform" /v:minimal
        if ($LASTEXITCODE -ne 0) { Write-Host "Restore failed." -ForegroundColor Red; exit 1 }
    }
}

# ---------- Clean (optional) ----------
if ($Clean) {
    Write-Step "Cleaning solution"
    & $msbuild $solutionFile /t:Clean /p:Configuration="$Configuration" /p:Platform="$Platform" /v:minimal
    if ($LASTEXITCODE -ne 0) { Write-Host "Clean failed." -ForegroundColor Red; exit 1 }
}

# ---------- Build solution ----------
Write-Step "Building solution ($Configuration|$Platform)"
& $msbuild $solutionFile /t:Build /p:Configuration="$Configuration" /p:Platform="$Platform" /v:minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild FAILED." -ForegroundColor Red
    exit 1
}

# ---------- Tests (optional) ----------
if ($RunTests) {
    Write-Step "Running tests"
    $vstest = Find-VSTest
    $testAssembly = Join-Path $solutionDir "src\Core.Test\bin\$Configuration\Core.Test.dll"

    if ($vstest -and (Test-Path $testAssembly)) {
        & $vstest $testAssembly
        if ($LASTEXITCODE -ne 0) { Write-Host "Tests failed." -ForegroundColor Red; exit 1 }
    } else {
        Write-Host "vstest.console.exe not found or test assembly missing, skipping tests." -ForegroundColor Yellow
    }
}

Write-Host "`nBuild succeeded." -ForegroundColor Green
