param(
	[int]$buildNumber = 0
	)

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER){
	$buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
	Write-Host "Using APPVEYOR_BUILD_NUMBER"
}

"Build number $buildNumber"

Import-Module .\src\packages\psake.4.3.2\tools\psake.psm1
Import-Module .\BuildFunctions.psm1
Invoke-Psake .\default.ps1 default -framework "4.0x64" -properties @{ buildNumber=$buildNumber }
Remove-Module BuildFunctions
Remove-Module psake