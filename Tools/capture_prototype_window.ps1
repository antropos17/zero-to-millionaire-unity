param([string]$OutputName = 'ui-v0.3-before-window.png')
$projectRoot = Split-Path -Parent $PSScriptRoot
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PrototypeWindowCapture {
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window,out Rect rect);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr window,int command);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window,IntPtr dc,uint flags);
}
'@
[PrototypeWindowCapture]::SetProcessDPIAware() | Out-Null
$players = @(Get-Process ZeroToMillionaire -ErrorAction SilentlyContinue | Where-Object { $_.Path.StartsWith((Join-Path $projectRoot 'Builds'),[System.StringComparison]::OrdinalIgnoreCase) -and $_.MainWindowHandle -ne 0 })
if ($players.Count -ne 1) { throw "Expected one project player window; found $($players.Count)" }
$player = $players[0]
[PrototypeWindowCapture]::ShowWindow($player.MainWindowHandle,9) | Out-Null
[PrototypeWindowCapture]::SetForegroundWindow($player.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 400
$bounds = New-Object PrototypeWindowCapture+Rect
if (-not [PrototypeWindowCapture]::GetWindowRect($player.MainWindowHandle,[ref]$bounds)) { throw 'Cannot read window bounds' }
$bitmap = New-Object System.Drawing.Bitmap ($bounds.Right-$bounds.Left),($bounds.Bottom-$bounds.Top)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$deviceContext = $graphics.GetHdc()
$captured = [PrototypeWindowCapture]::PrintWindow($player.MainWindowHandle,$deviceContext,2)
$graphics.ReleaseHdc($deviceContext)
if (-not $captured) { $graphics.Dispose(); $bitmap.Dispose(); throw 'Own-window capture unavailable; no desktop fallback used' }
$outputPath = Join-Path $projectRoot ('Evidence\'+[System.IO.Path]::GetFileName($OutputName))
$bitmap.Save($outputPath,[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $bitmap.Dispose()
[pscustomobject]@{Path=$outputPath;PlayerId=$player.Id;WindowTitle=$player.MainWindowTitle}
