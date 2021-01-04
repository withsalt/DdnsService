param([string]$buildtfm = 'all',[string]$app_name = '',[string]$ingorefiles = 'wwwroot')
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK version'
dotnet --version

if($app_name -eq $null -or $app_name -eq ""){
	Write-Host -ForegroundColor Red "Please input build project name."
	return;
}

#以下非必要情况不用修改
$build_file_name = "$app_name.exe"
$net_tfm = 'net5.0'
$dllpatcher_tfm = 'net5.0'
$configuration = 'Release'
$output_dir = "$PSScriptRoot\$app_name\bin\$configuration"
$dllpatcher_dir = "$PSScriptRoot\Build\DotNetDllPathPatcher"
$dllpatcher_exe = "$dllpatcher_dir\bin\$configuration\$dllpatcher_tfm\DotNetDllPathPatcher.exe"
$proj_path = "$PSScriptRoot\$app_name\$app_name.csproj"

$build           = $buildtfm -eq 'all' -or $buildtfm -eq 'platform'   #当前平台，框架依赖
$buildWinX86     = $buildtfm -eq 'all' -or $buildtfm -eq 'win-x86'    #windows x86
$buildWinX64     = $buildtfm -eq 'all' -or $buildtfm -eq 'win-x64'    #windows x64
$buildWinArm     = $buildtfm -eq 'all' -or $buildtfm -eq 'win-arm'    #windows arm 32
$buildLinuxX64   = $buildtfm -eq 'all' -or $buildtfm -eq 'linux-x64'  #linux x64
$buildLinuxArm32 = $buildtfm -eq 'all' -or $buildtfm -eq 'linux-arm'  #linux arm 32

if(!($build -or $buildWinX86 -or $buildWinX64 -or $buildWinArm -or $buildLinuxX64 -or $buildLinuxArm32)){
	Write-Host -ForegroundColor Red "Unsupport build platform name($buildtfm), only support all,platform,win-x86,win-x64,win-arm,linux-x64,linux-arm"
	return;
}

function Build-App
{
	Write-Host 'Building .NET App'
	
	$outdir = "$output_dir\$net_tfm"
	$publishDir = "$outdir\publish"

	Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore
	
	dotnet publish -c $configuration -f $net_tfm $proj_path
	if ($LASTEXITCODE) { exit $LASTEXITCODE }

	& $dllpatcher_exe -s "$publishDir\$build_file_name" -d "bin" -ig "$ingorefiles"
	if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function Build-SelfContained
{
	param([string]$rid)

	Write-Host "Building .NET App SelfContained $rid"

	$outdir = "$output_dir\$net_tfm\$rid"
	$publishDir = "$outdir\publish"

	Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

	dotnet publish -c $configuration -f $net_tfm -r $rid --self-contained true $proj_path
	if ($LASTEXITCODE) { exit $LASTEXITCODE }

	$new_file_name = $build_file_name

	if ($rid -match "linux") {
      $new_file_name = $build_file_name -creplace ".exe",""
    }

	Write-Host "Building $publishDir\$new_file_name"


	& $dllpatcher_exe -s "$publishDir\$new_file_name" -d "bin" -ig "$ingorefiles"
	if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function BuildDotNetDllPathPatcher{
	dotnet build -c $configuration -f $dllpatcher_tfm $dllpatcher_dir\DotNetDllPathPatcher.csproj
	if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function BuildApps{
	if ($build)
	{
		Build-App
	}

	if ($buildWinX64)
	{
		Build-SelfContained win-x64
	}

	if ($buildWinX86)
	{
		Build-SelfContained win-x86
	}

	if ($buildWinArm)
	{
		Build-SelfContained win-arm
	}

	if ($buildLinuxX64)
	{
		Build-SelfContained linux-x64
	}

	if ($buildLinuxArm32)
	{
		Build-SelfContained linux-arm
	}
}

BuildDotNetDllPathPatcher
BuildApps