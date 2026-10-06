param(
 [ValidateSet('Soak','Lifecycle','Observe')][string]$Mode,
 [ValidatePattern('^[A-Za-z0-9-]+$')][string]$RunName,
 [ValidateSet('reliability','reliability-before','lifecycle-before')][string]$BuildTag='reliability',
 [ValidateRange(900,1200)][int]$Seconds=900,
 [ValidateSet('fresh','ready','load')][string]$Fixture='fresh'
)
$ErrorActionPreference='Stop'
$ztmRoot=Split-Path -Parent $PSScriptRoot
$outDir=Join-Path $ztmRoot ('.tooling\reliability-'+$Mode.ToLower()+'-'+$RunName)
if(Test-Path -LiteralPath $outDir){throw 'Use a fresh private audit directory'}
if($Mode -eq 'Observe'){
 Add-Type -Path (Join-Path $PSScriptRoot 'NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
 $desktop=[ZtmWindowsPointer]::OpenInputDesktop(0,$false,1)
 try{
  $input=[ZtmWindowsPointer]::DesktopName($desktop)
  $thread=[ZtmWindowsPointer]::DesktopName([ZtmWindowsPointer]::GetThreadDesktop([ZtmWindowsPointer]::GetCurrentThreadId()))
  if($input -ne $thread){throw ('OS pointer audit blocked: input desktop '+$input+'; process desktop '+$thread+'. No input sent.')}
 }finally{if($desktop -ne [IntPtr]::Zero){[ZtmWindowsPointer]::CloseDesktop($desktop)|Out-Null}}
}
$exe=Join-Path $ztmRoot ('Builds\Windows-dev-v0.4d-'+$BuildTag+'\ZeroToMillionaireDev.exe')
if(!(Test-Path -LiteralPath $exe)){throw 'Development audit build is missing'}
New-Item -ItemType Directory -Path $outDir|Out-Null
$env:ZTM_QA_SAVE_DIR=Join-Path $outDir 'save'
$env:ZTM_PERF_DIR=$null;$env:ZTM_INPUT_AUDIT_DIR=$null;$env:ZTM_SOAK_AUDIT_DIR=$null;$env:ZTM_LIFECYCLE_AUDIT_DIR=$null
$env:ZTM_SHUTDOWN_DIAG=if($Mode -eq 'Lifecycle'){'1'}else{$null}
if($Mode -eq 'Soak'){$env:ZTM_SOAK_AUDIT_DIR=$outDir;$env:ZTM_SOAK_SECONDS=[string]$Seconds}
elseif($Mode -eq 'Lifecycle'){$env:ZTM_LIFECYCLE_AUDIT_DIR=$outDir}
else{$env:ZTM_INPUT_AUDIT_DIR=$outDir;$env:ZTM_INPUT_FIXTURE=$Fixture}
# Explicit Normal is for the interactive game window, not a background helper.
$p=Start-Process -FilePath $exe -ArgumentList @('-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $outDir 'Player.log')) -WindowStyle Normal -PassThru
$receipt=[ordered]@{pid=$p.Id;mode=$Mode;exe=$exe;profile=$env:ZTM_QA_SAVE_DIR;output=$outDir;launchedUtc=(Get-Date -AsUTC -Format o)}
$receipt|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $outDir 'launch.json') -Encoding utf8
$receipt|ConvertTo-Json
# Observe supplies state/rectangles and records events; it never dispatches input.
# Any OS pointer driver must require foreground HWND == this process's UnityWndClass
# immediately before EACH SendInput, and correlate events/state; do not click other windows.