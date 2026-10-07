param([string]$Exe,[string]$Prefix='native-v08-candidate-1',[ValidateSet('Fresh','Complete','Legacy','Reset')][string]$Case='Fresh',[int]$Width=360,[int]$Height=640,[int]$ObserveSeconds=4,[string]$Source)
$ErrorActionPreference='Stop';$r='X:\Projects\ZERO-TO-MILLIONAIRE'
if($Prefix-notmatch'^[a-z0-9-]+$'-or!(Test-Path -LiteralPath $Exe)-or$Source-notmatch'^[0-9a-f]{40}$'){throw 'Explicit immutable owned executable and source required'}
$base=Join-Path $r ('.tooling\'+$Prefix);if(Test-Path -LiteralPath $base){throw 'Immutable run exists'};[void](New-Item -ItemType Directory -Path $base)
$privateRoot=[IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $Exe) '..\..\.tooling'));$profile=Join-Path $privateRoot ($Prefix+'\save');if(Test-Path -LiteralPath $profile){throw 'Fresh profile required'};[void](New-Item -ItemType Directory -Path $profile)
if($Case-eq'Complete'){$inputSave=Join-Path $r 'Evidence\final-v08-2-level-completed-state.json'}
elseif($Case-eq'Legacy'){$inputSave=Join-Path $r '.tooling\native-v06-safety-final\story-save\level1-v1.json'}
elseif($Case-eq'Reset'){$inputSave=Join-Path $r '.tooling\v08-final-full-level-2\save\level1-v1.json'}
if($inputSave){if(!(Test-Path -LiteralPath $inputSave)){throw 'Actual private test state missing'};Copy-Item -LiteralPath $inputSave -Destination (Join-Path $profile 'level1-v1.json')}
Add-Type -Path (Join-Path $r 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public static class ZtmReleaseV08Close{[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);}'
function Run([string]$name){
 $out=Join-Path $base $name;[void](New-Item -ItemType Directory -Path $out);$launched=[DateTime]::UtcNow
 $p=Start-Process -FilePath $Exe -WorkingDirectory (Split-Path -Parent $Exe) -ArgumentList @('--ztm-private-profile',$profile,'-screen-fullscreen','0','-screen-width',$Width,'-screen-height',$Height,'-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
 $h=[IntPtr]::Zero;for($i=0;$i-lt100;$i++){$h=[ZtmWindowsPointer]::Find($p.Id);if($h-ne[IntPtr]::Zero){break};if($p.HasExited){throw 'Private Player exited before window'};Start-Sleep -Milliseconds 100}
 if($h-eq[IntPtr]::Zero){throw 'Owned Player HWND missing'}
 [void][ZtmWindowsPointer]::ShowWindow($h,4)
 $shot=Join-Path $profile 'readonly\native-first.png';$ready=$false
 for($i=0;$i-lt150;$i++){if((Test-Path -LiteralPath $shot)-and(Get-Item -LiteralPath $shot).LastWriteTimeUtc-ge$launched){$ready=$true;break};if($p.HasExited){throw 'Private native Player exited before render'};Start-Sleep -Milliseconds 100}
 if(!$ready){throw 'Fresh actual native pixels absent'}
 Start-Sleep -Seconds $ObserveSeconds
 if($p.Path-ne$Exe-or[ZtmWindowsPointer]::Find($p.Id)-ne$h){throw 'Owned process identity changed'}
 [void][ZtmReleaseV08Close]::PostMessage($h,16,[IntPtr]::Zero,[IntPtr]::Zero);if(!$p.WaitForExit(10000)){throw 'Owned private Player did not close normally'}
 $shotOut=Join-Path $r ('Evidence\'+$Prefix+'-'+$name+'.png');Copy-Item -LiteralPath $shot -Destination $shotOut
 $infoPath=Join-Path $profile 'readonly\release-readonly.json';$info=Get-Content -LiteralPath $infoPath -Raw|ConvertFrom-Json;Copy-Item -LiteralPath $infoPath -Destination (Join-Path $r ('Evidence\'+$Prefix+'-'+$name+'.json'))
 if($p.ExitCode-ne0-or$info.development-or$info.qaHooksPresent-or$info.automatedHarnessPresent-or!$info.profileVerified-or$info.drawCounterNonzero-lt50-or$info.lastFrame-le$info.firstFrame){throw 'Clean actual Release observer checks failed'}
 if($info.width-ne$Width-or$info.height-ne$Height){throw 'Actual native portrait sizing mismatch'}
 if($info.dialogueText-match'[\u0400-\u04ff]' -or $info.questTitle-match'[\u0400-\u04ff]'){throw 'English text check failed'}
 return [pscustomobject]@{name=$name;pid=$p.Id;privateProfile=$profile;frames=$info.lastFrame;draws=$info.drawCounterNonzero;cash=$info.state.cash;freeSpins=$info.state.freeSpins;spins=$info.state.spins;owned=$info.owned;fragments=$info.state.fragments;purchased=$info.state.purchased;grid=$info.state.lastGrid;storySeen=$info.state.tutorial.storySeenMask;dialogue=$info.dialogueVisible;complete=$info.complete;recovery=$info.recoveryRequired;width=$info.width;height=$info.height;medianMs=$info.medianMs;p95Ms=$info.p95Ms;samples=$info.samples;distance=$info.distance;physicalInputSent=$false;exitCode=$p.ExitCode;nativePixels=$true}
}
$first=Run 'first';$second=Run 'relaunch'
foreach($key in @('cash','spins','freeSpins','owned','storySeen','complete')){if($first.$key-ne$second.$key){throw ('Native persistence mismatch '+$key)}}
foreach($key in @('fragments','purchased','grid')){if(($first.$key-join',')-ne($second.$key-join',')){throw ('Native persistence mismatch '+$key)}}
if($Case-eq'Complete'){$expected=Get-Content -LiteralPath $inputSave -Raw|ConvertFrom-Json;if(!$first.complete-or$first.owned-ne8-or$first.cash-ne$expected.cash-or$first.spins-ne$expected.spins-or$first.storySeen-ne15-or$first.dialogue){throw 'Actual full-level state mismatch'}}
if($Case-eq'Legacy'-and($first.cash-ne170-or$first.spins-ne15-or$first.owned-ne2-or!$first.dialogue)){throw 'Legacy resource compatibility mismatch'}
if($Case-in@('Fresh','Reset')-and($first.cash-ne0-or$first.freeSpins-ne3-or$first.spins-ne0-or$first.owned-ne0-or!$first.dialogue)){throw 'Fresh/reset state mismatch'}
$result=[ordered]@{result='PASS';kind=$Case;exe=$Exe;source=$Source;scope='actual native clean Release launch/quit/relaunch, rendered pixels and runtime frame observation; no OS input; progression state originated in private Editor checks';runs=@($first,$second)}
$result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $r ('Evidence\'+$Prefix+'-checks.json')) -Encoding utf8
$result|ConvertTo-Json -Depth 8 -Compress
