param([ValidateSet('Launch','Click','DragCancel','Capture','State','Close')][string]$Action,[string]$Label='os-pointer-v06-baseline',[string]$Exe='',[int]$X=0,[int]$Y=0,[int]$Count=1,[int]$GapMs=40,[int]$WaitMs=0,[string]$Shot='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$out=Join-Path $root ('.tooling\'+$Label)
$contextFile=Join-Path $out 'context.json'
Add-Type -Path (Join-Path $root 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmPointerSafety{
[StructLayout(LayoutKind.Sequential)]struct LASTINPUTINFO{public uint cbSize,dwTime;}
[DllImport("user32.dll")]static extern bool GetLastInputInfo(ref LASTINPUTINFO x);
[DllImport("kernel32.dll")]public static extern ulong GetTickCount64();
public static uint Last(){var x=new LASTINPUTINFO{cbSize=8};if(!GetLastInputInfo(ref x))throw new Exception("Last input query failed");return x.dwTime;}
[DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint msg,IntPtr w,IntPtr l);
}
"@
function Desktop {
 $h=[ZtmWindowsPointer]::OpenInputDesktop(0,$false,1)
 try{return [ZtmWindowsPointer]::DesktopName($h)}finally{if($h-ne[IntPtr]::Zero){[void][ZtmWindowsPointer]::CloseDesktop($h)}}
}
function Cursor {$p=New-Object ZtmWindowsPointer+POINT;[void][ZtmWindowsPointer]::GetCursorPos([ref]$p);return $p}
function Remember {$p=Cursor;$ctx.lastInput=[ZtmPointerSafety]::Last();$ctx.cursorX=$p.x;$ctx.cursorY=$p.y;$ctx|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $contextFile -Encoding utf8}
function Guard {
 if((Desktop)-ne 'Default'){throw 'STOP: input desktop changed; no input sent'}
 $proc=Get-Process -Id $ctx.pid -ErrorAction Stop
 if($proc.Path -ne $ctx.exe){throw 'STOP: target process identity mismatch'}
 if([ZtmWindowsPointer]::Find($ctx.pid)-ne$hwnd){throw 'STOP: target window identity mismatch'}
 if([ZtmWindowsPointer]::GetForegroundWindow()-ne$hwnd){throw 'STOP: focus changed; no input sent'}
 if([ZtmPointerSafety]::Last()-ne$ctx.lastInput){throw 'STOP: input outside automation detected; no input sent'}
 $p=Cursor;if([Math]::Abs($p.x-$ctx.cursorX)-gt2-or[Math]::Abs($p.y-$ctx.cursorY)-gt2){throw 'STOP: user moved cursor; no input sent'}
}
function Capture([string]$name){Guard;if(!$name){throw 'Screenshot name required'};[ZtmWindowsPointer]::Capture($hwnd,(Join-Path $out ($name+'.png')))}
function SaveState { $p=Join-Path $ctx.save 'level1-v1.json';if(Test-Path -LiteralPath $p){return Get-Content -LiteralPath $p -Raw|ConvertFrom-Json};return $null }
if($Action-eq'Launch'){
 if((Desktop)-ne'Default'){throw 'STOP: interactive Default desktop required'}
 $idle=([ZtmPointerSafety]::GetTickCount64()-[ZtmPointerSafety]::Last())
 if($idle-lt3000){throw 'STOP: recent external input; private test not launched'}
 if(Test-Path -LiteralPath $contextFile){$old=Get-Content -LiteralPath $contextFile -Raw|ConvertFrom-Json;if(Get-Process -Id $old.pid -ErrorAction SilentlyContinue){throw 'Test process still exists'}}
 if(!(Test-Path -LiteralPath $out)){[void](New-Item -ItemType Directory -Path $out)}
 $save=Join-Path $out 'save';if(!(Test-Path -LiteralPath $save)){[void](New-Item -ItemType Directory -Path $save)}
 if(!$Exe){$Exe=Join-Path $root 'Builds\Windows-FINAL-v0.5\ZeroToMillionaire.exe'}
 $args=@('--ztm-private-profile',$save,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $out ('Player-'+(Get-Date -Format 'HHmmss')+'.log')))
 $p=Start-Process -FilePath $Exe -WorkingDirectory (Split-Path -Parent $Exe) -ArgumentList $args -WindowStyle Normal -PassThru
 $hwnd=[IntPtr]::Zero;for($i=0;$i-lt100;$i++){$hwnd=[ZtmWindowsPointer]::Find($p.Id);if($hwnd-ne[IntPtr]::Zero-and[ZtmWindowsPointer]::IsWindowVisible($hwnd)){break};Start-Sleep -Milliseconds 100}
 if($hwnd-eq[IntPtr]::Zero){throw 'Test window missing'}
 $shell=New-Object -ComObject WScript.Shell;[void]$shell.AppActivate([int]$p.Id);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell)
 $ctx=[pscustomobject]@{pid=$p.Id;hwnd=$hwnd.ToInt64();exe=$Exe;save=$save;lastInput=0;cursorX=0;cursorY=0}
 Remember;Start-Sleep -Milliseconds 3000;Guard
 if($Shot){Capture $Shot}
}elseif($Action-ne'State'){
 $ctx=Get-Content -LiteralPath $contextFile -Raw|ConvertFrom-Json;$hwnd=[IntPtr]$ctx.hwnd
 if($Action-eq'Close'){
  $p=Get-Process -Id $ctx.pid -ErrorAction Stop;if($p.Path-ne$ctx.exe-or[ZtmWindowsPointer]::Find($ctx.pid)-ne$hwnd){throw 'Owned private close identity guard failed'}
  [void][ZtmPointerSafety]::PostMessage($hwnd,16,[IntPtr]::Zero,[IntPtr]::Zero)
  if(!$p.WaitForExit(10000)){throw 'Private test did not exit normally'}
 }else{
  Guard
  if($Action-eq'Click'-or$Action-eq'DragCancel'){
   for($i=0;$i-lt$Count;$i++){
    Guard;$rect=New-Object ZtmWindowsPointer+RECT;[void][ZtmWindowsPointer]::GetClientRect($hwnd,[ref]$rect)
    if($X-lt0-or$Y-lt0-or$X-ge$rect.right-or$Y-ge$rect.bottom){throw 'Pointer target outside private client'}
    $point=[ZtmWindowsPointer]::ClientPoint($hwnd,$X,$Y);[void][ZtmWindowsPointer]::SetCursorPos($point.x,$point.y);Remember;Guard
    if([ZtmWindowsPointer]::Button($true)-ne1){throw 'OS mouse-down rejected'}
    Remember
    try{
     if($Shot-and$i-eq0){Start-Sleep -Milliseconds 100;Capture ($Shot+'-pressed')}
     if($Action-eq'DragCancel'){$point=[ZtmWindowsPointer]::ClientPoint($hwnd,5,20);[void][ZtmWindowsPointer]::SetCursorPos($point.x,$point.y);Remember}
    }finally{[void][ZtmWindowsPointer]::Button($false);Remember}
    Start-Sleep -Milliseconds $GapMs
   }
  }
  if($WaitMs-gt0){Start-Sleep -Milliseconds $WaitMs};Guard
  if($Shot){Capture $Shot}
 }
}else{$ctx=Get-Content -LiteralPath $contextFile -Raw|ConvertFrom-Json}
$state=SaveState
$record=[ordered]@{utc=(Get-Date -AsUTC -Format o);action=$Action;pid=$ctx.pid;window=$ctx.hwnd;x=$X;y=$Y;count=$Count;state=$state}
$record|ConvertTo-Json -Depth 8 -Compress|Add-Content -LiteralPath (Join-Path $out 'actions.jsonl') -Encoding utf8
[pscustomobject]@{action=$Action;pid=$ctx.pid;shot=$Shot;spins=$state.spins;cash=$state.cash;freeSpins=$state.freeSpins;parts=$state.fragments;owned=$state.purchased;tutorial=$state.tutorial}|ConvertTo-Json -Depth 6 -Compress
