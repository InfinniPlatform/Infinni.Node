function Create-Nuspec
{
	<#
	.Synopsis
		Creates nuspec-files from projects files.
	#>
	param
	(
		[Parameter(HelpMessage = "Path to the solution directory.")]
		[String] $solutionDir = '.',

		[Parameter(HelpMessage = "Path to the solution output directory.")]
		[String] $outputDir = 'Assemblies',

		[Parameter(HelpMessage = "Path to GlobalAssemblyInfo.cs.")]
		[String] $assemblyInfo = '.files\Packaging\GlobalAssemblyInfo.cs',

		[Parameter(HelpMessage = "VCS branch name.")]
		[String] $branchName = '',

		[Parameter(HelpMessage = "VCS commit hash.")]
		[String] $commitHash = '',

		[Parameter(HelpMessage = ".NET version.")]
		[String] $framework = 'net45'
	)

	process
	{
		### Build the version number

		$version = Get-Content $assemblyInfo `
			| Select-String -Pattern 'AssemblyVersion\s*\(\s*\"(?<version>.*?)\"\s*\)' `
			| ForEach-Object { $_.Matches[0].Groups['version'].Value }

		if ($branchName -and $branchName -notlike '*release-*')
		{
			$version = $version + '-' + ($branchName -replace '^(refs/heads/){0,1}(f\-){0,1}', '')
		}

		### Create nuspec-files for specified projects

		$projects = $projects = Get-ChildItem -Path $solutionDir -Filter '*.csproj' -Exclude '*.Tests.csproj' -Recurse

		foreach ($project in $projects)
		{
			[xml] $projectXml = Get-Content $project

			$projectName = (Get-ChildItem $project).BaseName
			$projectAssemblyName = ($projectXml.Project.PropertyGroup.AssemblyName[0])

			Write-Host "Create $projectName.nuspec"

			$projectNuspec = 
				"<?xml version=""1.0"" encoding=""utf-8""?>`r`n" + `
				"<package xmlns=""http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd"">`r`n" + `
				"    <metadata>`r`n" + `
				"        <id>$projectName</id>`r`n" + `
				"        <version>$version</version>`r`n" + `
				"        <authors>Infinnity Solutions</authors>`r`n" + `
				"        <owners>Infinnity Solutions</owners>`r`n" + `
				"        <requireLicenseAcceptance>false</requireLicenseAcceptance>`r`n" + `
				"        <description>Commit $commitHash</description>`r`n" + `
				"        <copyright>Infinnity Solutions $(Get-Date -Format yyyy)</copyright>`r`n" + `
				"        <dependencies>`r`n"

			# Add external dependencies

			$packagesConfigPath = Join-Path $project.Directory.FullName 'packages.config'

			if (Test-Path $packagesConfigPath)
			{
				[xml] $packagesConfigXml = Get-Content $packagesConfigPath

				$packages = $packagesConfigXml.packages.package

				if ($packages)
				{
					foreach ($package in $packages)
					{
						$projectNuspec = $projectNuspec + "            <dependency id=""$($package.id)"" version=""[$($package.version)]"" />`r`n"
					}
				}
			}

			# Add internal dependencies

			$projectReferences = $projectXml.Project.ItemGroup.ProjectReference.Name | Sort-Object | Get-Unique -AsString

			if ($projectReferences)
			{
				foreach ($projectReference in $projectReferences)
				{
					$projectNuspec = $projectNuspec + "            <dependency id=""$projectReference"" version=""[$version]"" />`r`n"
				}
			}

			$projectNuspec = $projectNuspec + `
				"        </dependencies>`r`n" + `
				"    </metadata>`r`n" + `
				"    <files>`r`n"

			# Add project assembly

			$projectIsLibrary = $projectXml.Project.PropertyGroup.OutputType -like '*Library*'
			$projectAssembly = $projectAssemblyName + $(if ($projectIsLibrary) { '.dll' } else { '.exe' })
			$projectNuspec = $projectNuspec + "        <file target=""lib\$framework\$projectAssembly"" src=""$projectAssembly"" />`r`n"

			# Add resources for ru-RU (if exists)

			$projectResourcesRu = $projectXml.Project.ItemGroup.EmbeddedResource.Include | Where { $_ -like '*.ru-RU.*' }

			if ($projectResourcesRu -and $projectResourcesRu.Count -gt 0 -and $projectResourcesRu[0])
			{
				$projectNuspec = $projectNuspec + "        <file target=""lib\$framework\ru-RU"" src=""ru-RU\$projectAssemblyName.resources.dll"" />`r`n"
			}

			# Add resources for en-US (if exists)

			$projectResourcesEn = $projectXml.Project.ItemGroup.EmbeddedResource.Include | Where { $_ -like '*.en-US.*' }

			if ($projectResourcesEn -and $projectResourcesEn.Count -gt 0 -and $projectResourcesEn[0])
			{
				$projectNuspec = $projectNuspec + "        <file target=""lib\$framework\en-US"" src=""en-US\$projectAssemblyName.resources.dll"" />`r`n"
			}

			# Add symbol file

			$projectNuspec = $projectNuspec + "        <file target=""lib\$framework"" src=""$projectAssemblyName.pdb"" />`r`n"

			# Add XML-documentation (if exists)

			$projectDocs = $projectXml.Project.PropertyGroup.DocumentationFile

			if ($projectDocs -and $projectDocs.Count -gt 0 -and $projectDocs[0])
			{
				$projectNuspec = $projectNuspec + "        <file target=""lib\$framework"" src=""$projectAssemblyName.xml"" />`r`n"
			}

			# Add config-file (if applicable)

			if (-not $projectIsLibrary)
			{
				$projectNuspec = $projectNuspec + "        <file target=""lib\$framework"" src=""$projectAssemblyName.exe.config"" />`r`n"
			}

			# Add log config-file (if applicable)

			if (-not $projectIsLibrary)
			{
				$projectNuspec = $projectNuspec + "        <file target=""lib\$framework"" src=""$projectAssemblyName.Log.config"" />`r`n"
			}

			$projectNuspec = $projectNuspec + `
				"    </files>`r`n" + `
				"</package>"

			Set-Content (Join-Path $outputDir ($projectName + '.nuspec')) -Value $projectNuspec
		}
	}
}