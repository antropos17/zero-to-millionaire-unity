param([string]$Exe='',[string]$Prefix='native-v07-readonly',[ValidateSet('Legacy','Reset','Extracted')][string]$Cases='Legacy',[string]$ResetSave='')
$ErrorActionPreference='Stop';$r=Split-Path -Parent $PSScriptRoot
if($Prefix-notmatch'^[a-z0-9-]+$'-or!(Test-Path -LiteralPath $Exe)){throw 'Explicit scoped executable required'}
$base=Join-Path $r ('.tooling\'+$Prefix);if(Test-Path -LiteralPath $base){throw 'Immutable run exists'};[void](New-Item -ItemType Directory -Path $base)
Add-Type -Path (Join-Path $r 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmReadOnlyV07{[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);}
"@
$profileRoot=[IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $Exe) '..\..\.tooling'))
$profile=Join-Path $profileRoot ($Prefix+'\save');if(Test-Path -LiteralPath $profile){throw 'Fresh private profile required'};[void](New-Item -ItemType Directory -Path $profile)
if($Cases-eq'Legacy'){$inputSave=Join-Path $r '.tooling\native-v06-safety-final\story-save\level1-v1.json';Copy-Item -LiteralPath $inputSave -Destination (Join-Path $profile 'level1-v1.json')}
elseif($Cases-eq'Reset'){if(!(Test-Path -LiteralPath $ResetSave)){throw 'Confirmed-reset source missing'};Copy-Item -LiteralPath $ResetSave -Destination (Join-Path $profile 'level1-v1.json')}
function Run([string]$name){
 $out=Join-Path $base $name;[void](New-Item -ItemType Directory -Path $out);$launchTime=[DateTime]::UtcNow
 $p=Start-Process -FilePath $Exe -WorkingDirectory (Split-Path -Parent $Exe) -ArgumentList @('--ztm-private-profile',$profile,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
 $h=[IntPtr]::Zero;for($i=0;$i-lt100;$i++){$h=[ZtmWindowsPointer]::Find($p.Id);if($h-ne[IntPtr]::Zero){break};Start-Sleep -Milliseconds 100}
 if($h-eq[IntPtr]::Zero){throw 'Owned Player HWND missing'}
 # Hidden Windows skip GPU drawing. Show only this owned test window without activation.
 [void][ZtmWindowsPointer]::ShowWindow($h,4)
 $shot=Join-Path $profile 'readonly\native-first.png';$observed=$false
 for($i=0;$i-lt150;$i++){if((Test-Path -LiteralPath $shot)-and(Get-Item -LiteralPath $shot).LastWriteTimeUtc-ge$launchTime){$observed=$true;break};if($p.HasExited){throw 'Private native Player exited before render'};Start-Sleep -Milliseconds 100}
 if(!$observed){throw 'Fresh actual native screenshot absent'}
 $fg=[ZtmWindowsPointer]::GetForegroundWindow();$d=[ZtmWindowsPointer]::OpenInputDesktop(0,$false,1);try{$desktop=[ZtmWindowsPointer]::DesktopName($d)}finally{if($d-ne[IntPtr]::Zero){[void][ZtmWindowsPointer]::CloseDesktop($d)}}
 # This run uses no keyboard/mouse actions or activation. Input is skipped when not owned foreground.
 if($p.Path-ne$Exe-or[ZtmWindowsPointer]::Find($p.Id)-ne$h){throw 'Owned close identity failed'}
 [void][ZtmReadOnlyV07]::PostMessage($h,16,[IntPtr]::Zero,[IntPtr]::Zero);if(!$p.WaitForExit(10000)){throw 'Private Player did not close'}
 Copy-Item -LiteralPath $shot -Destination (Join-Path $r ('Evidence\'+$Prefix+'-'+$name+'.png'))
 $infoPath=Join-Path $profile 'readonly\release-readonly.json';Copy-Item -LiteralPath $infoPath -Destination (Join-Path $r ('Evidence\'+$Prefix+'-'+$name+'.json'))
 $info=Get-Content -LiteralPath $infoPath -Raw|ConvertFrom-Json
 if($info.development-or$info.qaHooksPresent-or$info.automatedHarnessPresent-or!$info.profileVerified-or$info.drawCounterNonzero-lt50-or$info.lastFrame-le$info.firstFrame){throw 'Clean actual native verification failed'}
 return [pscustomobject]@{name=$name;pid=$p.Id;privateProfile=$profile;frames=$info.lastFrame;draws=$info.drawCounterNonzero;cash=$info.state.cash;freeSpins=$info.state.freeSpins;spins=$info.state.spins;owned=$info.owned;fragments=$info.state.fragments;purchased=$info.state.purchased;storySeen=$info.state.tutorial.storySeenMask;dialogue=$info.dialogueVisible;recovery=$info.recoveryRequired;desktop=$desktop;ownedForeground=($fg-eq$h);physicalInputSent=$false;exitCode=$p.ExitCode;nativePixels=$true}
}
$first=Run 'first';$second=Run 'relaunch'
if($first.cash-ne$second.cash-or$first.spins-ne$second.spins-or$first.freeSpins-ne$second.freeSpins-or$first.owned-ne$second.owned-or($first.fragments-join',')-ne($second.fragments-join',')-or($first.purchased-join',')-ne($second.purchased-join',')){throw 'Native resource persistence mismatch'}
if($Cases-eq'Legacy'-and($first.cash-ne170-or$first.spins-ne15-or$first.owned-ne2-or$first.storySeen-ne0-or!$first.dialogue-or!$second.dialogue)){throw 'Private v0.6 compatibility mismatch'}
if($Cases-ne'Legacy'-and($first.cash-ne0-or$first.freeSpins-ne3-or$first.spins-ne0-or$first.owned-ne0-or!$first.dialogue)){throw 'Fresh/reset state mismatch'}
$result=[ordered]@{result='PASS';kind=$Cases;exe=$Exe;source='dd74c983dcb22eaa7a03b54027dfa3b3aefe8926';scope=('actual native clean Release launch/quit/relaunch and rendered pixels; no OS input; case='+$Cases);runs=@($first,$second)}
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $r ('Evidence\'+$Prefix+'-checks.json')) -Encoding utf8
$result|ConvertTo-Json -Depth 6 -Compress