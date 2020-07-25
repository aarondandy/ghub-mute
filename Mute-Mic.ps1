param(
  [Parameter(Mandatory = $True, ParameterSetName = 'Setup')]
  [switch] $Setup,

  [Parameter(Mandatory = $True, ParameterSetName = 'Toggle')]
  [switch] $Toggle,

  [Parameter(Mandatory = $True, ParameterSetName = 'Mute')]
  [switch] $Mute,

  [Parameter(Mandatory = $True, ParameterSetName = 'Unmute')]
  [switch] $Unmute
)

$configFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'config.json'
$naudioDirPath = Join-Path -Path $PSScriptRoot -ChildPath '/NAudio/'
$naudioDllPath = Join-Path -Path $naudioDirPath -ChildPath '/NAudio.dll'
$naudioArchivePath = Join-Path -Path $naudioDirPath -ChildPath 'NAudio.zip'

if (Test-Path $configFilePath) {
  $config = Get-Content -Path $configFilePath | ConvertFrom-Json
} else {
  $config = @{}
}

if ($Setup) {
  if (-not (Test-Path $naudioDirPath)) {
    New-Item -ItemType Directory -Path $naudioDirPath
  }

  if (-not (Test-Path $naudioArchivePath)) {
    Invoke-WebRequest -Uri 'https://github.com/naudio/NAudio/releases/download/v1.8.4/NAudio-Release.zip' -OutFile $naudioArchivePath
  }

  if (-not (Test-Path $naudioDllPath)) {
    Expand-Archive -Path $naudioArchivePath -DestinationPath $naudioDirPath -Force
  }

  Add-Type -Path $naudioDllPath

  if (-not $config.devices) {
    $config.devices = @()
  }

  $selection = 0
  do {
    $deviceEnumerator = new-object NAudio.CoreAudioApi.MMDeviceEnumerator
    $deviceIdList = @()
    Write-Host "0 ) Save & Quit"
    try {
      foreach ($device in $deviceEnumerator.EnumerateAudioEndPoints('Capture','Active'))
      {
        try{
          $deviceIdList += $device.Id
          Write-Host "$($deviceIdList.Length) ) $(if ($config.devices -contains $device.Id) {'TARGET'} else {'ignore'}); $($device.FriendlyName)"
        }
        finally {
          $device.Dispose()
        }
      }

      $selection = Read-Host -Prompt "Select"
      if ($selection -eq 0) {
        break;
      }
      elseif ($selection -lt 0 -or $selection -gt $deviceIdList.Length) {
        Write-Host "Bad selection"
      }
      else {
        $selectedId = $deviceIdList[$selection - 1]
        if ($config.devices -contains $selectedId) {
          $config.devices = $selectedIdList | Where-Object -FilterScript {$_ -ne $selectedId}
          if (-not $config.devices) {
            $config.devices = @()
          }
        } else {
          $config.devices = @($config.devices) + @($selectedId)
        }
      }
    }
    finally {
      $deviceEnumerator.Dispose();
    }
  } while ($True)

  if ($config.devices.Length -eq 0) {
    throw "No devices selected"
  }

  $config | ConvertTo-Json | Out-File $configFilePath
}

if ($Toggle) {
  throw 'TODO'
}

if ($Mute) {
  throw 'TODO'
}
elseif ($Unmute) {
  throw 'TODO'
}