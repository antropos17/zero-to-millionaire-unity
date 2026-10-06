$ErrorActionPreference='Stop'
$ztmRoot=Split-Path -Parent $PSScriptRoot
$ztmExe=Join-Path $ztmRoot 'Builds\Windows-rc-v0.5\ZeroToMillionaire.exe'
$ztmSource='17f525238b1925819b857f9bfd31cb5ec4062126'
Remove-Item Env:ZTM_QA_SAVE_DIR,Env:ZTM_PERF_DIR -ErrorAction SilentlyContinue
Add-Type -Path (Join-Path $ztmRoot 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmCloseOwnedPlayer{[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint msg,IntPtr w,IntPtr l);}
"@
function Invoke-ZtmClean([string]$label,[string]$saveDir,[string]$inputSave=''){
 if(-not (Test-Path -LiteralPath $saveDir)){[void](New-Item -ItemType Directory -Path $saveDir)}
 if($inputSave){Copy-Item -LiteralPath $inputSave -Destination (Join-Path $saveDir 'level1-v1.json')}
 $ztmOut=Join-Path $ztmRoot ('.tooling\'+$label);if(Test-Path -LiteralPath $ztmOut){throw 'Use immutable new run label'};[void](New-Item -ItemType Directory -Path $ztmOut)
 $ztmProcess=Start-Process -FilePath $ztmExe -ArgumentList @('--ztm-private-profile',$saveDir,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $ztmOut 'Player.log')) -WindowStyle Normal -PassThru
 $ztmHwnd=[IntPtr]::Zero
 for($ztmAttempt=0;$ztmAttempt -lt 100;$ztmAttempt++){ $ztmHwnd=[ZtmWindowsPointer]::Find($ztmProcess.Id); if($ztmHwnd -ne [IntPtr]::Zero -and [ZtmWindowsPointer]::IsWindowVisible($ztmHwnd)){break};Start-Sleep -Milliseconds 100 }
 if($ztmHwnd -eq [IntPtr]::Zero -or -not [ZtmWindowsPointer]::IsWindowVisible($ztmHwnd)){throw 'Owned clean Player HWND not visible'}
 for($ztmAttempt=0;$ztmAttempt -lt 100 -and -not (Test-Path -LiteralPath (Join-Path $saveDir 'readonly\native-first.png'));$ztmAttempt++){Start-Sleep -Milliseconds 100}
 if(-not (Test-Path -LiteralPath (Join-Path $saveDir 'readonly\native-first.png'))){throw 'Actual native screenshot absent'}
 Start-Sleep -Milliseconds 1000
 [void][ZtmCloseOwnedPlayer]::PostMessage($ztmHwnd,16,[IntPtr]::Zero,[IntPtr]::Zero)
 if(-not $ztmProcess.WaitForExit(10000)){throw 'Owned private Player did not close normally'}
 Copy-Item -LiteralPath (Join-Path $saveDir 'readonly\native-first.png') -Destination (Join-Path $ztmOut 'native-first.png')
 Copy-Item -LiteralPath (Join-Path $saveDir 'readonly\release-readonly.json') -Destination (Join-Path $ztmOut 'release-readonly.json')
 $ztmInfo=Get-Content -LiteralPath (Join-Path $ztmOut 'release-readonly.json') -Raw|ConvertFrom-Json
 if($ztmInfo.development -or $ztmInfo.qaHooksPresent -or $ztmInfo.automatedHarnessPresent -or -not $ztmInfo.profileVerified -or $ztmInfo.drawCounterNonzero -lt 10 -or $ztmInfo.lastFrame -le $ztmInfo.firstFrame){throw 'Clean Release verification failed'}
 $ztmLaunch=[ordered]@{source=$ztmSource;exe=$ztmExe;pid=$ztmProcess.Id;hwnd=$ztmHwnd.ToInt64();visible=$true;normal_exit=$true;exit_code=$ztmProcess.ExitCode;actual_frameCount=$ztmInfo.lastFrame;privateSave=$saveDir;utc=(Get-Date -AsUTC -Format o)}
 $ztmLaunch|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $ztmOut 'launch.json') -Encoding utf8
 [pscustomobject]@{run=$label;frames=$ztmInfo.lastFrame;owned=$ztmInfo.owned;cash=$ztmInfo.state.cash;spins=$ztmInfo.state.spins;dialogue=$ztmInfo.dialogueVisible;taskCash=$ztmInfo.taskCashPrice;taskParts=$ztmInfo.taskFragmentGoal;qaHooks=$ztmInfo.qaHooksPresent;normal_exit=$true}|ConvertTo-Json -Compress
}
$ztmFresh=Join-Path $ztmRoot '.tooling\release-clean-v0.5-fresh\save'
Invoke-ZtmClean 'release-clean-v0.5-fresh-run' $ztmFresh
Invoke-ZtmClean 'release-clean-v0.5-fresh-relaunch' $ztmFresh
$ztmComplete=Join-Path $ztmRoot '.tooling\release-clean-v0.5-complete\save'
Invoke-ZtmClean 'release-clean-v0.5-complete-run' $ztmComplete (Join-Path $ztmRoot '.tooling\release-audit-v0.5-level\save\level1-v1.json')
$ztmLegacy=Join-Path $ztmRoot '.tooling\release-clean-v0.5-legacy\save'
Invoke-ZtmClean 'release-clean-v0.5-legacy-run' $ztmLegacy 'C:\Users\murtu\AppData\LocalLow\PrototypeWorkspace\ZERO TO MILLIONAIRE\level1-v1.json'
