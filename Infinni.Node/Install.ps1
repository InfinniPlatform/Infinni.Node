<#
.Synopsis
	Installs Infinni.Node.
#>
param
(
	[Parameter(HelpMessage = "Version of Infinni.Node. The latest version will install by default.")]
	[String] $version = ''
)

# Install NuGet package manager

Write-Host 'Install NuGet package manager'

$nugetDir = Join-Path $env:ProgramData 'NuGet'
$nugetPath = Join-Path $nugetDir 'nuget.exe'

if (-not (Test-Path $nugetPath))
{
	if (-not (Test-Path $nugetDir))
	{
		New-Item $nugetDir -ItemType Directory -ErrorAction SilentlyContinue
	}

	$nugetSourceUri = 'http://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
	Invoke-WebRequest -Uri $nugetSourceUri -OutFile $nugetPath
}

# Find Infinni.Node version

Write-Host 'Find Infinni.Node version'

$sources = 'https://api.nuget.org/v3/index.json;http://nuget.org/api/v2;https://www.myget.org/F/infinniplatform;'

if (-not $version)
{
	$version = (((& "$nugetPath" list 'Infinni.Node' -NonInteractive -Prerelease -Source $sources) | Out-String) -split '[\r\n]' `
				| Where { $_ -match 'Infinni.Node' } `
				| Select-Object -First 1) -split '\s' `
				| Select-Object -Last 1
}

Write-Host "Infinni.Node.$version"

# Create install directory

$outputDir = Join-Path '.' "Infinni.Node.$version"

if (Test-Path $outputDir)
{
	Write-Host "Infinni.Node.$version is already installed"

	return
}

New-Item $outputDir -ItemType Directory -ErrorAction SilentlyContinue | Out-Null

# Install Infinni.Node package

Write-Host "Install Infinni.Node.$version"

& "$nugetPath" install 'Infinni.Node' -Version $version -OutputDirectory 'packages' -NonInteractive -Prerelease -Source $sources

# Copy all references

Write-Host "Copy files"

$projectRefs = (Get-ChildItem -Path 'packages' -Filter 'Infinni.Node.references' -Recurse | Select-Object -First 1)

Get-Content $projectRefs.FullName | Foreach-Object {
	if ($_ -match '^.*?\\lib(\\.*?){0,1}\\(?<path>.*?)$')
	{
		$item = Join-Path (Join-Path "$outputDir" 'bin') $matches.path

		$itemParent = Split-Path $item

		if (-not (Test-Path $itemParent))
		{
			New-Item $itemParent -ItemType Directory | Out-Null
		}

		Copy-Item -Path (Join-Path 'packages' $_) -Destination $item -Recurse -ErrorAction SilentlyContinue
	}
}
	
Copy-Item -Path (Join-Path $projectRefs.Directory.FullName "*") -Destination $outputDir -Exclude @( '*.ps1', '*references' ) -Recurse -ErrorAction SilentlyContinue

# Remove temp files

Remove-Item -Path 'packages' -Recurse -ErrorAction SilentlyContinue
