param([string]$RunName,[bool]$Trace=$false,[string]$BuildTag='baseline')
$ztmRoot=Split-Path -Parent $PSScriptRoot
$outDir=Join-Path $ztmRoot ('.tooling\perf-v0.4d-'+$RunName)
if(Test-Path -LiteralPath $outDir){throw 'Use a new audit output directory'}
New-Item -ItemType Directory -Path $outDir|Out-Null
$env:ZTM_PERF_DIR=$outDir
$env:ZTM_QA_SAVE_DIR=Join-Path $outDir 'save'
$env:ZTM_PERF_TRACE=if($Trace){'1'}else{'0'}
$env:ZTM_PERF_COMMIT='102b85a+documented-v0.4d-current-'+$BuildTag
$exe=Join-Path $ztmRoot ('Builds\Windows-dev-v0.4d-'+$BuildTag+'\ZeroToMillionaireDev.exe')
# This is the requested interactive game window, not a background helper.
# An explicit Normal startup avoids inheriting the hidden shell startup flag.
$proc=Start-Process -FilePath $exe -ArgumentList @('-screen-fullscreen','0','-screen-width','480','-screen-height','960','-force-d3d11','-logFile',(Join-Path $outDir 'Player.log')) -WindowStyle Normal -PassThru
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class ZtmNativeAuditWindow {
 public delegate bool EnumProc(IntPtr handle,IntPtr param);
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback,IntPtr param);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr handle,out uint pid);
 [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr handle,int show);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr handle);
 [DllImport("user32.dll")] public static extern int GetClassName(IntPtr handle,StringBuilder name,int count);
 public static IntPtr Find(int processId) {
  IntPtr result=IntPtr.Zero;
  EnumWindows((h,p)=>{uint id;GetWindowThreadProcessId(h,out id);if(id==processId){var name=new StringBuilder(256);GetClassName(h,name,256);if(name.ToString()=="UnityWndClass")result=h;}return true;},IntPtr.Zero);
  return result;
 }
}
'@
$handle=[IntPtr]::Zero
for($attempt=0;$attempt -lt 50 -and $handle -eq [IntPtr]::Zero;$attempt++){
 Start-Sleep -Milliseconds 100
 $handle=[ZtmNativeAuditWindow]::Find($proc.Id)
}
if($handle -eq [IntPtr]::Zero){throw 'Native Player HWND not found'}
for($waitVisible=0;$waitVisible -lt 50 -and -not [ZtmNativeAuditWindow]::IsWindowVisible($handle);$waitVisible++){Start-Sleep -Milliseconds 100; $handle=[ZtmNativeAuditWindow]::Find($proc.Id)}
# Let Unity create and show its own game window; no mid-initialization ShowWindow call.
$visible=[ZtmNativeAuditWindow]::IsWindowVisible($handle)
if(-not $visible){throw 'Audit Player still hidden'}
$receipt=[ordered]@{pid=$proc.Id;hwnd=$handle.ToInt64();visible=$visible;normal_unity_window_startup=$true;exe=$exe;run=$RunName;trace=$Trace;launched_utc=(Get-Date -AsUTC -Format o)}
[System.IO.File]::WriteAllText((Join-Path $outDir 'launch-window.json'),($receipt|ConvertTo-Json),(New-Object System.Text.UTF8Encoding($false)))
$receipt|ConvertTo-Json
