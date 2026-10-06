param([ValidateSet('Launch','Click','Key','Capture','State','Close')][string]$Action,[string]$Exe='',[string]$Tag='native-v07-candidate',[int]$X=0,[int]$Y=0,[int]$Count=1,[int]$VirtualKey=9,[int]$WaitMs=0,[string]$Shot='')
$ErrorActionPreference='Stop';$r=Split-Path -Parent $PSScriptRoot
if($Tag-notmatch'^[a-z0-9-]+$'){throw 'Scoped tag required'}
$out=Join-Path $r ('.tooling\'+$Tag);$context=Join-Path $out 'owned-target.json'
Add-Type -Path (Join-Path $r 'Tools\NativeWindowsPointer.cs') -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core
Add-Type -TypeDefinition @"
using System;using System.Runtime.InteropServices;public static class ZtmV07Input{
 [StructLayout(LayoutKind.Sequential)]struct LAST{public uint size,time;}
 [StructLayout(LayoutKind.Sequential)]public struct KEY{public ushort vk,scan;public uint flags,time;public UIntPtr extra;}
 [StructLayout(LayoutKind.Explicit,Size=40)]public struct INPUT{[FieldOffset(0)]public uint type;[FieldOffset(8)]public KEY key;}
 [DllImport("user32.dll")]static extern bool GetLastInputInfo(ref LAST x);
 [DllImport("kernel32.dll")]public static extern ulong GetTickCount64();
 [DllImport("user32.dll")]static extern uint SendInput(uint n,INPUT[] x,int s);
 [DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
 public static uint Last(){var x=new LAST{size=8};if(!GetLastInputInfo(ref x))throw new Exception("Input state unavailable");return x.time;}
 public static uint Tap(ushort vk){if(IntPtr.Size!=8)throw new Exception("64-bit input required");return SendInput(2,new[]{new INPUT{type=1,key=new KEY{vk=vk,extra=new UIntPtr(0x5A544D)}},new INPUT{type=1,key=new KEY{vk=vk,flags=2,extra=new UIntPtr(0x5A544D)}}},40);}
}
"@
function Desktop{$d=[ZtmWindowsPointer]::OpenInputDesktop(0,$false,1);try{return [ZtmWindowsPointer]::DesktopName($d)}finally{if($d-ne[IntPtr]::Zero){[void][ZtmWindowsPointer]::CloseDesktop($d)}}}
function Cursor{$v=[ZtmWindowsPointer+POINT]::new();[void][ZtmWindowsPointer]::GetCursorPos([ref]$v);return $v}
function Remember{$v=Cursor;$ctx.lastInput=[ZtmV07Input]::Last();$ctx.cursorX=$v.x;$ctx.cursorY=$v.y;$ctx|ConvertTo-Json|Set-Content -LiteralPath $context -Encoding utf8}
function Identity{$p=Get-Process -Id $ctx.pid -ErrorAction Stop;if($p.Path-ne$ctx.exe-or[ZtmWindowsPointer]::Find($ctx.pid)-ne$hwnd){throw 'STOP: owned process/window identity changed'};return $p}
function Guard{[void](Identity);if((Desktop)-ne'Default'){throw 'STOP: input desktop changed'};if([ZtmWindowsPointer]::GetForegroundWindow()-ne$hwnd){throw 'STOP: foreground changed'};if([ZtmV07Input]::Last()-ne$ctx.lastInput){throw 'STOP: external input detected'};$v=Cursor;if($v.x-ne$ctx.cursorX-or$v.y-ne$ctx.cursorY){throw 'STOP: external cursor movement detected'}}
function Capture{Guard;if(!$Shot){throw 'Screenshot name required'};if($Shot-notmatch'^[a-z0-9-]+$'){throw 'Scoped screenshot name required'};[ZtmWindowsPointer]::Capture($hwnd,(Join-Path $out ($Shot+'.png')))}
if($Action-eq'Launch'){
 if((Desktop)-ne'Default'){throw 'STOP: Default desktop unavailable'}
 $last=[ZtmV07Input]::Last();if(([ZtmV07Input]::GetTickCount64()-$last)-lt5000){throw 'STOP: recent external input; no private launch'}
 if(Test-Path -LiteralPath $context){throw 'Immutable run context exists'}
 if(!$Exe-or!(Test-Path -LiteralPath $Exe)){throw 'Explicit candidate executable required'}
 $prior=[ZtmWindowsPointer]::GetForegroundWindow();$priorCursor=Cursor;$save=Join-Path $out 'save';[void](New-Item -ItemType Directory -Path $save -Force)
 $p=Start-Process -FilePath $Exe -WorkingDirectory (Split-Path -Parent $Exe) -ArgumentList @('--ztm-private-profile',$save,'-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
 $hwnd=[IntPtr]::Zero;for($i=0;$i-lt100;$i++){$hwnd=[ZtmWindowsPointer]::Find($p.Id);if($hwnd-ne[IntPtr]::Zero){break};Start-Sleep -Milliseconds 100}
 $ctx=[pscustomobject]@{pid=$p.Id;hwnd=$hwnd.ToInt64();exe=$Exe;save=$save;lastInput=0;cursorX=0;cursorY=0};Remember
 if($hwnd-eq[IntPtr]::Zero){throw 'Owned native window missing'}
 $nowCursor=Cursor;$fg=[ZtmWindowsPointer]::GetForegroundWindow()
 if((Desktop)-ne'Default'-or[ZtmV07Input]::Last()-ne$last-or$nowCursor.x-ne$priorCursor.x-or$nowCursor.y-ne$priorCursor.y-or($fg-ne$prior-and$fg-ne$hwnd)){throw 'STOP: external activity during launch; no activation/input'}
 [void][ZtmWindowsPointer]::ShowWindow($hwnd,5);[void][ZtmWindowsPointer]::SetForegroundWindow($hwnd);Remember;Guard
 Start-Sleep -Milliseconds 3500;Guard;if($Shot){Capture}
}else{
 $ctx=Get-Content -LiteralPath $context -Raw|ConvertFrom-Json;$hwnd=[IntPtr]$ctx.hwnd
 if($Action-eq'Close'){$p=Identity;[void][ZtmV07Input]::PostMessage($hwnd,16,[IntPtr]::Zero,[IntPtr]::Zero);if(!$p.WaitForExit(10000)){throw 'Owned private Player did not exit normally'};$exit=$p.ExitCode}
 elseif($Action-ne'State'){
  Guard
  if($Action-eq'Click'){
   for($i=0;$i-lt$Count;$i++){
    Guard;$rect=[ZtmWindowsPointer+RECT]::new();[void][ZtmWindowsPointer]::GetClientRect($hwnd,[ref]$rect);if($X-lt0-or$Y-lt0-or$X-ge$rect.right-or$Y-ge$rect.bottom){throw 'Target outside owned client'}
    $point=[ZtmWindowsPointer]::ClientPoint($hwnd,$X,$Y);[void][ZtmWindowsPointer]::SetCursorPos($point.x,$point.y);Remember;Guard
    $down=[ZtmWindowsPointer+INPUT]::new();$down.type=0;$m=[ZtmWindowsPointer+MOUSEINPUT]::new();$m.dwFlags=2;$m.dwExtraInfo=[UIntPtr]0x5A544D;$down.mouse=$m
    $up=[ZtmWindowsPointer+INPUT]::new();$up.type=0;$m.dwFlags=4;$up.mouse=$m
    if([ZtmWindowsPointer]::SendInput(2,[ZtmWindowsPointer+INPUT[]]@($down,$up),[Runtime.InteropServices.Marshal]::SizeOf($down))-ne2){throw 'Atomic mouse tap rejected'};Remember;Start-Sleep -Milliseconds 50
   }
  }elseif($Action-eq'Key'){if([ZtmV07Input]::Tap([ushort]$VirtualKey)-ne2){throw 'Keyboard tap rejected'};Remember}
  if($WaitMs-gt0){if($WaitMs-gt10000){throw 'Bounded wait required'};Start-Sleep -Milliseconds $WaitMs};Guard;if($Shot){Capture}
 }
}
$saveFile=Join-Path $ctx.save 'level1-v1.json';$s=if(Test-Path -LiteralPath $saveFile){Get-Content -LiteralPath $saveFile -Raw|ConvertFrom-Json}else{$null}
$record=[ordered]@{utc=(Get-Date -AsUTC -Format o);action=$Action;pid=$ctx.pid;window=$ctx.hwnd;privateSave=$ctx.save;x=$X;y=$Y;count=$Count;key=$VirtualKey;shot=$Shot;exit=$exit;state=$s};$record|ConvertTo-Json -Depth 8 -Compress|Add-Content -LiteralPath (Join-Path $out 'actions.jsonl') -Encoding utf8
[pscustomobject]@{action=$Action;pid=$ctx.pid;shot=$Shot;spins=$s.spins;cash=$s.cash;parts=$s.fragments;owned=$s.purchased;exit=$exit}|ConvertTo-Json -Compress