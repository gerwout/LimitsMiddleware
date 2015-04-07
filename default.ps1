properties {
	$projectName = "LimitsMiddleware"
	$buildNumber = 0
	$rootDir  = Resolve-Path .\
	$buildOutputDir = "$rootDir\build"
	$reportsDir = "$buildOutputDir\reports"
	$srcDir = "$rootDir\src"
	$solutionFilePath = "$srcDir\$projectName.sln"
	$assemblyInfoFilePath = "$srcDir\SharedAssemblyInfo.cs"
	$ilmerge_path = "$srcDir\packages\ilmerge.2.14.1208\tools\ilmerge.exe"
}

task default -depends Clean, UpdateVersion, RunTests, ILMerge, CreateNuGetPackages

task Clean {
	Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /t:Clean }
}

task UpdateVersion {
	$version = Get-Version $assemblyInfoFilePath
	$oldVersion = New-Object Version $version
	$newVersion = New-Object Version ($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build, $buildNumber)
	Update-Version $newVersion $assemblyInfoFilePath
}

task Compile {
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /p:Configuration=Release }
}

task ILMerge -depends Compile {
	$dllDir = "$srcDir\LimitsMiddleware\bin\Release"
	$input_dlls = "$dllDir\LimitsMiddleware.dll"
	Get-ChildItem -Path $dllDir -Filter *.dll |
		foreach-object {
			if ("$_" -ne "LimitsMiddleware.dll") {
				$input_dlls = "$input_dlls $dllDir\$_"
			}
	}

	New-Item $buildOutputDir -Type Directory -ErrorAction SilentlyContinue
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /out:$buildOutputDir\$projectName.dll $input_dlls"
}

task RunTests -depends Compile {
	$xunitRunner = "$srcDir\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe"
	gci . -Recurse -Include *Tests.csproj, Tests.*.csproj | % {
		$project = $_.BaseName
		if(!(Test-Path $reportsDir\xUnit\$project)){
			New-Item $reportsDir\xUnit\$project -Type Directory
		}
        exec { .$xunitRunner "$srcDir\$project\bin\Release\$project.dll" -html "$reportsDir\xUnit\$project\index.html" }
    }
}

task CreateNuGetPackages -depends Compile {
	$versionString = Get-Version $assemblyInfoFilePath
	$version = New-Object Version $versionString
	$packageVersion = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString() + "-build" + $buildNumber.ToString().PadLeft(5,'0')
	gci $srcDir -Recurse -Include *.nuspec | % {
		exec { .$srcDir\.nuget\nuget.exe pack $_ -o $buildOutputDir -version $packageVersion }
	}
}
