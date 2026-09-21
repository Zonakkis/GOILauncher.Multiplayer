<#
.SYNOPSIS
    构建和测试 GOILauncher.Multiplayer。

.DESCRIPTION
    默认处理整个解决方案。-Project 指定单个项目时，只构建该项目以及它自己的依赖闭包；
    -RunTests 时只跑（传递）引用该项目的测试项目。测试范围按引用关系推导，不按命名约定，
    所以 Core / Server 这类被多个测试项目依赖的项目会把它们都跑上。

    全量测试很便宜（约 4 秒），慢的是构建和 React 构建，所以 -Project 的价值主要在编译一侧。

.EXAMPLE
    .\scripts\build.ps1 -Configuration Debug -SkipRestore
    全量构建。

.EXAMPLE
    .\scripts\build.ps1 -Configuration Debug -SkipRestore -RunTests
    全量构建并跑全部测试。

.EXAMPLE
    .\scripts\build.ps1 -Project DedicatedServer -RunTests
    只构建 DedicatedServer 并只跑 DedicatedServer.Tests。
    构建 DedicatedServer 会顺带构建 React Web UI（可用 -SkipWebUI 关掉）。

.EXAMPLE
    .\scripts\build.ps1 -Project Core
    只构建 Core，不碰测试也不碰 Web UI。
#>
param(
    [string]$Project,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipRestore,
    [switch]$Clean,
    [switch]$RunTests,
    [switch]$SkipWebUI
)

$ErrorActionPreference = "Stop"

$solutionDir  = Split-Path -Parent $PSScriptRoot
$solutionFile = Join-Path $solutionDir "GOILauncher.Multiplayer.sln"
$webRoot      = Join-Path $solutionDir "src\DedicatedServer\React"

# ---------- Helpers ----------
function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }
function Fail($msg) {
    Write-Host "ERROR: $msg" -ForegroundColor Red
    exit 1
}

function Assert-DotNet {
    $onPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $onPath) {
        Fail "dotnet not found on PATH. Install the .NET SDK (8.0 or later)."
    }
    return $onPath.Source
}

# ---------- Project discovery ----------
# A "source project" is any *.csproj directly under src\<Name>\. The name used on the
# command line is the csproj base name, which currently matches the directory name.
function Get-SourceProjects {
    $srcRoot = Join-Path $solutionDir "src"
    if (-not (Test-Path $srcRoot)) { return @() }

    $projects = New-Object System.Collections.Generic.List[object]
    foreach ($dir in (Get-ChildItem -Path $srcRoot -Directory)) {
        foreach ($csproj in (Get-ChildItem -Path $dir.FullName -Filter *.csproj -File -ErrorAction SilentlyContinue)) {
            $projects.Add([pscustomobject]@{ Name = $csproj.BaseName; Path = $csproj.FullName })
        }
    }
    return $projects
}

# Test projects keep the existing convention: a directory ending in .Test / .Tests
# directly under src\ or tests\.
function Get-TestProjects {
    $roots = @("src", "tests") |
        ForEach-Object { Join-Path $solutionDir $_ } |
        Where-Object { Test-Path $_ }

    $projects = New-Object System.Collections.Generic.List[object]
    foreach ($root in $roots) {
        $dirs = Get-ChildItem -Path $root -Directory | Where-Object { $_.Name -match '\.Tests?$' }
        foreach ($dir in $dirs) {
            $csproj = Get-ChildItem -Path $dir.FullName -Filter *.csproj -File -ErrorAction SilentlyContinue |
                Select-Object -First 1
            if ($csproj) {
                $projects.Add([pscustomobject]@{ Name = $csproj.BaseName; Path = $csproj.FullName })
            }
        }
    }
    return $projects
}

# ---------- Project reference graph ----------
# Read straight from the csproj files rather than asking MSBuild, so the graph costs
# nothing and works before anything is restored.
function Get-ProjectReferences([string]$CsprojPath) {
    [xml]$xml = Get-Content -LiteralPath $CsprojPath -Raw
    $baseDir = Split-Path -Parent $CsprojPath

    $refs = New-Object System.Collections.Generic.List[string]
    foreach ($node in $xml.SelectNodes('//ProjectReference')) {
        $include = $node.Include
        if (-not $include) { continue }
        $full = [System.IO.Path]::GetFullPath((Join-Path $baseDir $include))
        if (Test-Path -LiteralPath $full) { $refs.Add($full) }
    }
    return $refs
}

# Everything the given project depends on, transitively. The root itself is not included.
function Get-ReferenceClosure([string]$CsprojPath) {
    $seen = @{}
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue($CsprojPath)

    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        foreach ($ref in (Get-ProjectReferences $current)) {
            $key = $ref.ToLowerInvariant()
            if (-not $seen.ContainsKey($key)) {
                $seen[$key] = $ref
                $queue.Enqueue($ref)
            }
        }
    }
    return $seen.Values
}

# ---------- Web UI ----------
function Invoke-WebUI {
    if (-not (Test-Path (Join-Path $webRoot "package.json"))) {
        Fail "React project not found at $webRoot"
    }

    $pnpm = Get-Command pnpm -ErrorAction SilentlyContinue
    if (-not $pnpm) {
        Fail "pnpm not found. Install Node.js and enable Corepack before building."
    }

    if (-not $SkipRestore) {
        Write-Step "Restoring Web UI packages"
        & pnpm --dir $webRoot install --frozen-lockfile
        if ($LASTEXITCODE -ne 0) { Fail "pnpm install failed." }
    } elseif (-not (Test-Path (Join-Path $webRoot "node_modules"))) {
        Fail "Web UI packages are missing and -SkipRestore was specified."
    }

    Write-Step "Building Web UI"
    & pnpm --dir $webRoot build
    if ($LASTEXITCODE -ne 0) { Fail "Web UI build failed." }
}

# ---------- Tests ----------
function Read-TrxCounters([string]$TrxPath) {
    [xml]$trx = Get-Content -LiteralPath $TrxPath
    return $trx.TestRun.ResultSummary.Counters
}

# ============================================================
# Resolve what to build
# ============================================================
$dotnet = Assert-DotNet
Write-Host "dotnet: $dotnet" -ForegroundColor DarkGray

$sourceProjects = Get-SourceProjects

if ($Project) {
    $match = $sourceProjects | Where-Object { $_.Name -eq $Project } | Select-Object -First 1
    if (-not $match) {
        $names = ($sourceProjects | ForEach-Object { $_.Name } | Sort-Object) -join ", "
        Fail "Unknown project '$Project'. Valid names: $names"
    }
    $primaryTargets = @($match.Path)
    Write-Host "Project: $($match.Name)" -ForegroundColor DarkGray
} else {
    $primaryTargets = @($solutionFile)
    Write-Host "Project: (entire solution)" -ForegroundColor DarkGray
}

# Which test projects to run, and therefore also to build.
$testProjects = @()
if ($RunTests) {
    $all = Get-TestProjects
    if ($Project) {
        $targetKey = ([System.IO.Path]::GetFullPath($primaryTargets[0])).ToLowerInvariant()
        $testProjects = @($all | Where-Object {
            $closure = Get-ReferenceClosure $_.Path
            @($closure | Where-Object { $_.ToLowerInvariant() -eq $targetKey }).Count -gt 0
        })
    } else {
        $testProjects = @($all)
    }
}

# Building the primary target does not build test projects that depend on it, so with -Project
# they have to be built explicitly before `dotnet test --no-build` can be used. Without -Project
# the solution build already covers them, and adding them here would build each one twice.
$buildTargets = New-Object System.Collections.Generic.List[object]
foreach ($t in $primaryTargets) { $buildTargets.Add($t) }
if ($Project) {
    foreach ($t in $testProjects) { $buildTargets.Add($t.Path) }
}

# ============================================================
# Restore
# ============================================================
if (-not $SkipRestore) {
    Write-Step "Restoring packages"
    foreach ($target in $buildTargets) {
        & $dotnet restore $target
        if ($LASTEXITCODE -ne 0) { Fail "Restore failed for $target" }
    }
}

# ============================================================
# Clean (optional)
# ============================================================
if ($Clean) {
    Write-Step "Cleaning"
    foreach ($target in $buildTargets) {
        & $dotnet clean $target -c $Configuration
        if ($LASTEXITCODE -ne 0) { Fail "Clean failed for $target" }
    }
}

# ============================================================
# Web UI
# ============================================================
# DedicatedServer serves the panel out of wwwroot, so building it alone without the UI
# would produce a server with a stale or missing panel. Every other single project skips it.
$wantsWebUI = (-not $Project) -or ($Project -eq "DedicatedServer")
if ($wantsWebUI -and -not $SkipWebUI) {
    Invoke-WebUI
}

# ============================================================
# Build
# ============================================================
Write-Step "Building ($Configuration)"
foreach ($target in $buildTargets) {
    & $dotnet build $target -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { Fail "Build failed for $target" }
}

# ============================================================
# Tests (optional)
# ============================================================
# -RunTests means the tests MUST run. No test project, a missing result file, or a run
# that executed zero tests are all hard failures: `dotnet test` exits 0 when it discovers
# nothing, so the executed count has to be asserted explicitly or a broken adapter would
# show up as a green build.
if ($RunTests) {
    Write-Step "Running tests"

    if ($testProjects.Count -eq 0) {
        if ($Project) {
            Fail "No test project references '$Project', so -RunTests has nothing to verify."
        }
        Fail "No test projects found under src\ or tests\. A test project directory must end in '.Test' or '.Tests'."
    }

    $resultsDir = Join-Path $solutionDir "artifacts\test-results"
    if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

    $total = 0
    $passed = 0
    $failed = 0
    $notExecuted = 0

    foreach ($testProject in $testProjects) {
        Write-Host "Test project: $($testProject.Name)" -ForegroundColor DarkGray

        $trxName = "$($testProject.Name)-$Configuration.trx"
        $trxPath = Join-Path $resultsDir $trxName

        & $dotnet test $testProject.Path -c $Configuration --no-build `
            --logger "trx;LogFileName=$trxName" --results-directory $resultsDir
        $testExit = $LASTEXITCODE

        if (-not (Test-Path -LiteralPath $trxPath)) {
            Fail "$($testProject.Name) produced no result file ($trxPath); the run cannot be verified."
        }

        $counters = Read-TrxCounters $trxPath
        $total        += [int]$counters.total
        $passed       += [int]$counters.passed
        $failed       += [int]$counters.failed + [int]$counters.error +
                         [int]$counters.timeout + [int]$counters.aborted
        $notExecuted  += [int]$counters.notExecuted

        if ($testExit -ne 0) {
            Write-Host "$($testProject.Name) reported failure (dotnet test exit code $testExit)." -ForegroundColor Red
        }
    }

    Write-Host "`nTest summary: total=$total passed=$passed failed=$failed notExecuted=$notExecuted" -ForegroundColor DarkGray

    if ($total -eq 0) {
        Fail "Zero tests were discovered, so nothing was verified. Re-run without -SkipRestore to restore packages."
    }
    if ($failed -gt 0) {
        Fail "Tests FAILED."
    }

    Write-Host "$passed/$total tests passed." -ForegroundColor Green
}

Write-Host "`nBuild succeeded." -ForegroundColor Green
