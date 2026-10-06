$ErrorActionPreference='Stop'
$r=Split-Path -Parent $PSScriptRoot
$base=Join-Path $r '.tooling\native-render-compare-v06'
if(Test-Path -LiteralPath $base){throw 'Immutable render comparison exists'}
[void](New-Item -ItemType Directory -Path $base)
Add-Type -Path (Join-Path $r 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmRenderCompare {
[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
[DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern bool SetWindowText(IntPtr h,string s);
}
"@
$exe=Join-Path $r 'Builds\Windows-v0.6-motion-checkpoint\ZeroToMillionaire.exe';$rows=@()
function Run([string]$name,[bool]$visible){
 $out=Join-Path $base $name;[void](New-Item -ItemType Directory -Path $out);$save=Join-Path $out 'save';[void](New-Item -ItemType Directory -Path $save)
 $desktopHandle=[ZtmWindowsPointer]::OpenInputDesktop(0,$false,1)
 try{$desktop=[ZtmWindowsPointer]::DesktopName($desktopHandle)}finally{if($desktopHandle-ne[IntPtr]::Zero){[void][ZtmWindowsPointer]::CloseDesktop($desktopHandle)}}
 if($desktop-ne'Default'){throw 'Input desktop is not Default; no desktop/security changes allowed'}
 $foregroundBefore=[ZtmWindowsPointer]::GetForegroundWindow();$p=$null;$h=[IntPtr]::Zero
 try{
  $p=Start-Process -FilePath $exe -WorkingDirectory (Split-Path -Parent $exe) -ArgumentList @('--ztm-private-profile',$save,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
  for($i=0;$i-lt100;$i++){$h=[ZtmWindowsPointer]::Find($p.Id);if($h-ne[IntPtr]::Zero){break};Start-Sleep -Milliseconds 100}
  if($h-eq[IntPtr]::Zero){throw 'Owned private HWND missing'}
  if($visible){[void][ZtmRenderCompare]::SetWindowText($h,'ZERO TO MILLIONAIRE - PRIVATE RENDER QA');[void][ZtmWindowsPointer]::ShowWindow($h,4)}
  $foregroundAfterShow=[ZtmWindowsPointer]::GetForegroundWindow()
  $shot=Join-Path $save 'readonly\native-first.png'
  for($i=0;$i-lt400-and!(Test-Path -LiteralPath $shot);$i++){if($p.HasExited){throw 'Owned process exited before capture'};Start-Sleep -Milliseconds 100}
  if(!(Test-Path -LiteralPath $shot)){throw 'Native first screenshot absent'}
  Start-Sleep -Milliseconds 1000
  if($p.Path-ne$exe-or[ZtmWindowsPointer]::Find($p.Id)-ne$h){throw 'Owned close identity failed'}
  [void][ZtmRenderCompare]::PostMessage($h,16,[IntPtr]::Zero,[IntPtr]::Zero)
  if(!$p.WaitForExit(10000)){throw 'Normal private close failed'}
  $infoPath=Join-Path $save 'readonly\release-readonly.json';$info=Get-Content -LiteralPath $infoPath -Raw|ConvertFrom-Json
  if($info.development-or$info.qaHooksPresent-or$info.automatedHarnessPresent-or!$info.profileVerified){throw 'Exact clean binary/profile check failed'}
  Copy-Item -LiteralPath $shot -Destination (Join-Path $r ('Evidence\native-render-v06-'+$name+'.png'))
  Copy-Item -LiteralPath $infoPath -Destination (Join-Path $r ('Evidence\native-render-v06-'+$name+'.json'))
  return [pscustomobject]@{run=$name;source='d425e9b69fe823948bfb7f71f0f304e1b9a3ff5f';exeSHA=(Get-FileHash -LiteralPath $exe).Hash;pid=$p.Id;hwnd=$h.ToInt64();requestedVisible=$visible;foregroundBefore=$foregroundBefore.ToInt64();foregroundAfterShow=$foregroundAfterShow.ToInt64();noActivationVerified=($foregroundBefore-eq$foregroundAfterShow);osInputSent=$false;desktop=$desktop;normalExit=($p.ExitCode-eq0);frames=$info.lastFrame;drawCounterNonzero=$info.drawCounterNonzero;samples=$info.samples;medianMs=$info.medianMs;p95Ms=$info.p95Ms}
 }finally{if($p-and!$p.HasExited-and$p.Path-eq$exe){if($h-ne[IntPtr]::Zero){[void][ZtmRenderCompare]::PostMessage($h,16,[IntPtr]::Zero,[IntPtr]::Zero);[void]$p.WaitForExit(10000)}}}
}
$rows+=Run 'hidden' $false
$rows+=Run 'shown-without-activation' $true
$rows|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $r 'Evidence\native-render-v06-summary.json') -Encoding utf8
$rows|ConvertTo-Json -Depth 6 -Compress