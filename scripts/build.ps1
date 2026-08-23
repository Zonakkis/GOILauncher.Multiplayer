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

# ---------- Locate test assemblies ----------
# Convention: a test project lives in a directory ending in ".Test" or ".Tests" directly
# under src\ or tests\, and its assembly name matches that directory name. Every such
# project must have produced an assembly; a project that exists but was not built is an
# error, not something to skip silently.
function Find-TestAssemblies([string]$Configuration) {
    $roots = @("src", "tests") |
        ForEach-Object { Join-Path $solutionDir $_ } |
        Where-Object { Test-Path $_ }

    $assemblies = New-Object System.Collections.Generic.List[string]
    $missing = New-Object System.Collections.Generic.List[string]

    foreach ($root in $roots) {
        $projectDirs = Get-ChildItem -Path $root -Directory |
            Where-Object { $_.Name -match '\.Tests?$' }

        foreach ($projectDir in $projectDirs) {
            $binDir = Join-Path $projectDir.FullName "bin\$Configuration"
            $hit = $null
            if (Test-Path $binDir) {
                # -Recurse so SDK-style projects with a TFM subdirectory are found too.
                $hit = Get-ChildItem -Path $binDir -Recurse -Filter "$($projectDir.Name).dll" |
                    Select-Object -First 1
            }

            if ($hit) { $assemblies.Add($hit.FullName) }
            else { $missing.Add($projectDir.FullName) }
        }
    }

    return [pscustomobject]@{
        Assemblies = $assemblies
        Missing    = $missing
    }
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
# -RunTests means the tests MUST run. Missing tooling, a missing assembly or a run that
# executed zero tests are all hard failures: vstest.console.exe exits 0 when it discovers
# no tests, so the executed count has to be asserted explicitly or a broken test adapter
# would show up as a green build.
if ($RunTests) {
    Write-Step "Running tests"

    $vstest = Find-VSTest
    if (-not $vstest) {
        Write-Host "ERROR: vstest.console.exe not found, so tests could not run." -ForegroundColor Red
        Write-Host "       Install the Visual Studio 'Testing tools core features' component," -ForegroundColor Red
        Write-Host "       or build without -RunTests if you deliberately want no test gate." -ForegroundColor Red
        exit 1
    }
    Write-Host "VSTest: $vstest" -ForegroundColor DarkGray

    $discovery = Find-TestAssemblies $Configuration
    foreach ($dir in $discovery.Missing) {
        Write-Host "ERROR: test project '$dir' produced no assembly for configuration '$Configuration'." -ForegroundColor Red
    }
    if ($discovery.Missing.Count -gt 0) { exit 1 }

    if ($discovery.Assemblies.Count -eq 0) {
        Write-Host "ERROR: no test assemblies found under src\ or tests\." -ForegroundColor Red
        Write-Host "       A test project directory must end in '.Test' or '.Tests' and its" -ForegroundColor Red
        Write-Host "       assembly name must match the directory name." -ForegroundColor Red
        exit 1
    }

    foreach ($assembly in $discovery.Assemblies) {
        Write-Host "Test assembly: $assembly" -ForegroundColor DarkGray
    }

    $resultsDir = Join-Path $solutionDir "artifacts\test-results"
    if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

    $trxName = "vstest-$Configuration.trx"
    & $vstest @($discovery.Assemblies) "/Logger:trx;LogFileName=$trxName" "/ResultsDirectory:$resultsDir"
    $vstestExit = $LASTEXITCODE

    $trxPath = Join-Path $resultsDir $trxName
    if (-not (Test-Path $trxPath)) {
        Write-Host "`nERROR: vstest produced no result file ($trxPath); the run cannot be verified." -ForegroundColor Red
        exit 1
    }

    [xml]$trx = Get-Content -LiteralPath $trxPath
    $counters = $trx.TestRun.ResultSummary.Counters
    $total = [int]$counters.total
    $passed = [int]$counters.passed
    $notExecuted = [int]$counters.notExecuted
    $failed = [int]$counters.failed + [int]$counters.error + [int]$counters.timeout + [int]$counters.aborted

    Write-Host "`nTest summary: total=$total passed=$passed failed=$failed notExecuted=$notExecuted" -ForegroundColor DarkGray

    if ($total -eq 0) {
        Write-Host "ERROR: zero tests were discovered, so nothing was verified." -ForegroundColor Red
        Write-Host "       This usually means the NUnit test adapter was not imported — run a" -ForegroundColor Red
        Write-Host "       build without -SkipRestore to restore packages, then try again." -ForegroundColor Red
        exit 1
    }

    if ($failed -gt 0 -or $vstestExit -ne 0) {
        Write-Host "Tests FAILED (vstest exit code $vstestExit)." -ForegroundColor Red
        exit 1
    }

    Write-Host "$passed/$total tests passed." -ForegroundColor Green
}

Write-Host "`nBuild succeeded." -ForegroundColor Green
