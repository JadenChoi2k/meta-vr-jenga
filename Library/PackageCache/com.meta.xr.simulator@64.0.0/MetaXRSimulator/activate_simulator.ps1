param(
  [int]$MessageTime = 5
)

function Get-ScriptDirectory {
	Split-Path -Parent $PSCommandPath
}

function LocateSimulatorJson
{
	$locations = @("SIMULATOR.json", "meta_openxr_simulator_ci.json", "meta_openxr_simulator.json", "..\config\meta_openxr_simulator.json")
	$scriptDir = Get-ScriptDirectory
	foreach ($l in $locations)
	{
		$p = Join-Path -Path $scriptDir -ChildPath $l | Resolve-Path -ErrorAction SilentlyContinue
		if ($p -eq $null)
		{
			continue
		}
		if (Test-Path -Path $p)
		{
			return $p
		}
	}
	Write-Error -Message "Unable to locate Meta XR Simulator configuration file" -ErrorAction Stop
}

function GetActiveRuntimeJsonPath
{
	try
	{
		$v = Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Khronos\OpenXR\1 -Name ActiveRuntime -ErrorAction SilentlyContinue
		return $v
	}
	catch
	{
		return $null
	}
}

function CreateActionRuntimeRegistryKeyIfNotExist
{
	$keyExist = Test-Path -Path HKLM:SOFTWARE\Khronos\OpenXR\1
	if ($keyExist -eq $False)
	{
		New-Item -Path HKLM:SOFTWARE\Khronos\OpenXR\1 -ItemType Directory -Force
	}
}

try
{
	$simulatorJsonPath = LocateSimulatorJson
	# Write-Output "Simulator JSON Path: $simulatorJsonPath"

	$activeRuntimeJsonPath = GetActiveRuntimeJsonPath
	# Write-Output "OpenXR ActiveRuntime JSON Path: $activeRuntimeJsonPath"

	if ($activeRuntimeJsonPath -eq $simulatorJsonPath)
	{
		Write-Output "OpenXR ActivateRuntime has already been set to Meta XR Simulator"
		Timeout $MessageTime
		return
	}

	# We need to modify the registry, self-elevate the script if required
	if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator'))
	{
		if ([int](Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty BuildNumber) -ge 6000)
		{
			Write-Output "Relaunch the script with elevated priviledge ..."
			$CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
			Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine -Wait
			Exit
		}
	}

	CreateActionRuntimeRegistryKeyIfNotExist

	if ($activeRuntimeJsonPath -ne $null)
	{
		Set-ItemProperty -Path HKLM:SOFTWARE\Khronos\OpenXR\1 -Name "PreviousActiveRuntime" -Value $activeRuntimeJsonPath
	}

	Set-ItemProperty -Path HKLM:SOFTWARE\Khronos\OpenXR\1 -Name "ActiveRuntime" -Value $simulatorJsonPath

	Write-Output "OpenXR ActiveRuntime set to Meta XR Simulator: $simulatorJsonPath"

	Start-Sleep -s $MessageTime
}
catch
{
	Write-Error $_
	Start-Sleep -s 5
}
