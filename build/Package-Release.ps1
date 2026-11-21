[CmdletBinding()]
param(
	[Parameter(Mandatory)]
	[string] $Tag,
	[string] $OutputDirectory,
	[string] $InnoCompiler = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$versionMatch = [regex]::Match($Tag, '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$')
if (-not $versionMatch.Success) {
	throw "Invalid tag '$Tag'. Use v1.2.3 or a prerelease such as v1.2.3-rc.1."
}
$coreParts = @(1..3 | ForEach-Object { $versionMatch.Groups[$_].Value })
foreach ($part in $coreParts) {
	if ([long]$part -gt 65534) { throw 'Version components must be between 0 and 65534 for Windows file version metadata.' }
}
if ($versionMatch.Groups[4].Success) {
	foreach ($identifier in $versionMatch.Groups[4].Value.Split('.')) {
		if ($identifier -match '^0[0-9]+$') { throw 'Numeric prerelease identifiers must not have leading zeroes.' }
	}
}
if (-not (Test-Path -LiteralPath $InnoCompiler -PathType Leaf)) {
	throw "Inno Setup 6 is required. Compiler not found: $InnoCompiler"
}

$root = Split-Path $PSScriptRoot -Parent
$version = $Tag.Substring(1)
$fileVersion = ($coreParts -join '.') + '.0'
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'artifacts\release' }
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $OutputDirectory) -and (Get-ChildItem -LiteralPath $OutputDirectory -Force | Select-Object -First 1)) {
	throw "Output directory must be empty to avoid packaging stale files: $OutputDirectory"
}
$publishDir = Join-Path $OutputDirectory 'publish'
$downloadsDir = Join-Path $OutputDirectory 'downloads'
New-Item -ItemType Directory -Path $publishDir, $downloadsDir -Force | Out-Null

$publishArgs = @(
	'publish', (Join-Path $root 'ComScanner.csproj'),
	'-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
	'-o', $publishDir,
	'-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true',
	'-p:PublishTrimmed=false', '-p:DebugType=None', '-p:DebugSymbols=false',
	"-p:Version=$version", "-p:AssemblyVersion=$fileVersion", "-p:FileVersion=$fileVersion"
)
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
if (-not (Test-Path (Join-Path $publishDir 'ComScanner.exe'))) { throw 'Published executable is missing.' }

Copy-Item (Join-Path $root 'README.md'), (Join-Path $root 'LICENSE.txt') -Destination $publishDir
Copy-Item (Join-Path $root 'Images') -Destination $publishDir -Recurse
$zipPath = Join-Path $downloadsDir "ComScanner-$version-win-x64-Portable.zip"
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -CompressionLevel Optimal

$installerArgs = @(
	"/DAppVersion=$version", "/DFileVersion=$fileVersion",
	"/DPublishDir=$publishDir", "/DOutputDir=$downloadsDir",
	(Join-Path $PSScriptRoot 'ComScanner.iss')
)
& $InnoCompiler @installerArgs
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compilation failed.' }
$installerPath = Join-Path $downloadsDir "ComScanner-$version-win-x64-Setup.exe"
if (-not (Test-Path $installerPath)) { throw 'Installer executable is missing.' }

$checksumLines = @($installerPath, $zipPath) | ForEach-Object {
	$hash = Get-FileHash -LiteralPath $_ -Algorithm SHA256
	'{0}  {1}' -f $hash.Hash.ToLowerInvariant(), [System.IO.Path]::GetFileName($_)
}
$checksumLines | Set-Content (Join-Path $downloadsDir "ComScanner-$version-SHA256SUMS.txt") -Encoding ascii
Write-Host "Release packages ready: $downloadsDir"
