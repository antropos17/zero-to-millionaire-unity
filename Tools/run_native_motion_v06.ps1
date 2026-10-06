$ErrorActionPreference='Stop';$r=Split-Path -Parent $PSScriptRoot;$base=Join-Path $r '.tooling\native-v06-motion-checkpoint';if(Test-Path -LiteralPath $base){throw 'Immutable native run exists'};[void](New-Item -ItemType Directory -Path $base)
Add-Type -Path (Join-Path $r 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmSafetyClose{[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);}
"@
$exe=Join-Path $r 'Builds\Windows-v0.6-motion-checkpoint\ZeroToMillionaire.exe';$rows=@()
function Run([string]$name,[string]$save){
 $out=Join-Path $base $name;[void](New-Item -ItemType Directory -Path $out);$p=Start-Process -FilePath $exe -ArgumentList @('--ztm-private-profile',$save,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
 $script:currentProc=$p;$h=[IntPtr]::Zero;for($i=0;$i-lt100;$i++){$h=[ZtmWindowsPointer]::Find($p.Id);if($h-ne[IntPtr]::Zero){break};Start-Sleep -Milliseconds 100}
 if($h-eq[IntPtr]::Zero){throw 'Owned private HWND missing'}
 # No activation or OS input: own private background player only.
 $script:currentH=$h; $shot=Join-Path $save 'readonly\native-first.png';for($i=0;$i-lt400-and!(Test-Path -LiteralPath $shot);$i++){Start-Sleep -Milliseconds 100}
 if(!(Test-Path -LiteralPath $shot)){throw 'Native actual first render missing'}
 Start-Sleep -Milliseconds 400
 if($p.Path-ne$exe-or[ZtmWindowsPointer]::Find($p.Id)-ne$h){throw 'Owned close identity failed'}
 [void][ZtmSafetyClose]::PostMessage($h,16,[IntPtr]::Zero,[IntPtr]::Zero);if(!$p.WaitForExit(10000)){throw 'Owned private native exit failed'}
 $infoPath=Join-Path $save 'readonly\release-readonly.json';$info=Get-Content -LiteralPath $infoPath -Raw|ConvertFrom-Json
 if($info.development-or$info.qaHooksPresent-or$info.automatedHarnessPresent-or!$info.profileVerified-or$info.drawCounterNonzero-lt10-or$info.samples-lt100){throw 'Native clean Release verification failed'}
 Copy-Item -LiteralPath $shot -Destination (Join-Path $r ('Evidence\native-v06-motion-'+$name+'.png'));Copy-Item -LiteralPath $infoPath -Destination (Join-Path $r ('Evidence\native-v06-motion-'+$name+'.json'))
 return [pscustomobject]@{run=$name;pid=$p.Id;frames=$info.lastFrame;cash=$info.state.cash;storySeen=$info.state.tutorial.storySeenMask;dialogue=$info.dialogueVisible;recovery=$info.recoveryRequired;recoveryVisible=$info.recoveryVisible;spinEnabled=$info.spinEnabled;normalExit=($p.ExitCode-eq0);medianMs=$info.medianMs;p95Ms=$info.p95Ms}
}
try {
$story=Join-Path $base 'story-save';[void](New-Item -ItemType Directory -Path $story)
$ids=@('floor-patches','surface-repair','cheap-wallpaper','door-repair','window-repair','first-mattress','small-lamp','wooden-stool');$prices=@(60,115,155,195,235,300,315,355);$goals=@(20,30,38,44,50,58,62,70);$quests=@();for($i=0;$i-lt8;$i++){$quests+=@{id=$ids[$i];cashPrice=$prices[$i];fragmentGoal=$goals[$i];status=$(if($i-lt2){3}elseif($i-eq2){1}else{0})}}
$state=@{version=5;cash=170;freeSpins=3;spins=15;recoveries=0;heroineU=1.85;heroineV=1.45;fragments=@(0,0,0,0,0,0,0,0);purchased=@($true,$true,$false,$false,$false,$false,$false,$false);lastGrid=@(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);quests=$quests;tutorial=@{version=1;step=0;storySeenMask=0;completed=$true;skipped=$false}}
$json=$state|ConvertTo-Json -Depth 8;Set-Content -LiteralPath (Join-Path $story 'level1-v1.json') -Value $json -Encoding utf8
$first=Run 'story-first' $story;$rows+=$first;if(!$first.dialogue-or$first.storySeen-ne0-or$first.cash-ne170){throw 'Pending story native state invalid'}
Remove-Item -LiteralPath (Join-Path $story 'readonly\native-first.png');$second=Run 'story-relaunch' $story;$rows+=$second;if(!$second.dialogue-or$second.storySeen-ne0-or$second.cash-ne170){throw 'Undismissed story lost on actual native quit/relaunch'}
$future=Join-Path $base 'future-save';[void](New-Item -ItemType Directory -Path $future);$state.version=6;$primary=Join-Path $future 'level1-v1.json';Set-Content -LiteralPath $primary -Value ($state|ConvertTo-Json -Depth 8) -Encoding utf8;Set-Content -LiteralPath ($primary+'.bak') -Value $json -Encoding utf8;$before=(Get-FileHash -LiteralPath $primary).Hash;$backupBefore=(Get-FileHash -LiteralPath ($primary+'.bak')).Hash
$f=Run 'future-blocked' $future;$rows+=$f;if(!$f.recovery-or!$f.recoveryVisible-or$f.spinEnabled-or(Get-FileHash -LiteralPath $primary).Hash-ne$before-or(Get-FileHash -LiteralPath ($primary+'.bak')).Hash-ne$backupBefore){throw 'Unsupported future save was modified or playable'}
$rows|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $r 'Evidence\native-v06-motion-safety-summary.json') -Encoding utf8
$rows|ConvertTo-Json -Compress

} finally { if ($script:currentProc -and !$script:currentProc.HasExited -and $script:currentProc.Path -eq $exe) { if ($script:currentH -ne [IntPtr]::Zero) {[void][ZtmSafetyClose]::PostMessage($script:currentH,16,[IntPtr]::Zero,[IntPtr]::Zero);[void]$script:currentProc.WaitForExit(10000)} } }
