param(
    [string]$Source = 'C:\Users\murtu\Documents\Codex\2026-10-06\task\zero-to-millionaire-first-playable',
    [string]$Destination = 'X:\Projects\ZERO-TO-MILLIONAIRE'
)
$ErrorActionPreference='Stop'
$sourceRoot=[IO.Path]::GetFullPath($Source).TrimEnd('\')
$targetRoot=[IO.Path]::GetFullPath($Destination).TrimEnd('\')
if($sourceRoot -ne 'C:\Users\murtu\Documents\Codex\2026-10-06\task\zero-to-millionaire-first-playable'){throw 'Unexpected source'}
if($targetRoot -ne 'X:\Projects\ZERO-TO-MILLIONAIRE'){throw 'Unexpected target'}
if(Test-Path -LiteralPath $targetRoot){throw 'Target must be fresh; no overwrite allowed'}
$active=@(Get-CimInstance Win32_Process | Where-Object {($_.Name -eq 'Unity.exe' -or $_.Name -eq 'ZeroToMillionaire.exe') -and $_.CommandLine -like ('*'+$sourceRoot+'*')})
if($active.Count){throw 'Close only project Editor/player normally before copying'}
$reparse=@(Get-ChildItem -LiteralPath $sourceRoot -Directory -Recurse | Where-Object {($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0})
if($reparse.Count){throw 'Reparse directory requires separate review'}
$files=@(Get-ChildItem -LiteralPath $sourceRoot -File -Recurse)
$totalBytes=($files|Measure-Object Length -Sum).Sum
New-Item -ItemType Directory -Path $targetRoot | Out-Null
$copyLog=Join-Path $targetRoot 'migration-copy.log'
& robocopy.exe $sourceRoot $targetRoot /E /COPY:DAT /DCOPY:DAT /XJ /R:1 /W:1 /NP /NFL /NDL /NJH /NJS "/LOG:$copyLog" | Out-Null
$copyExit=$LASTEXITCODE
if($copyExit -ge 8){throw "Robocopy failed with $copyExit; source retained"}
Write-Output "Copied $($files.Count) files; verifying every SHA256"
$manifest=[Collections.Generic.List[object]]::new()
$timer=[Diagnostics.Stopwatch]::StartNew();$lastNotice=0;$index=0
foreach($file in $files){
    $relative=$file.FullName.Substring($sourceRoot.Length+1)
    $target=Join-Path $targetRoot $relative
    if(-not(Test-Path -LiteralPath $target -PathType Leaf)){throw "Missing target: $relative"}
    $targetInfo=Get-Item -LiteralPath $target
    if($targetInfo.Length -ne $file.Length){throw "Size mismatch: $relative"}
    $sourceHash=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $targetHash=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
    if($sourceHash -ne $targetHash){throw "Hash mismatch: $relative"}
    $manifest.Add([pscustomobject]@{relative_path=$relative;bytes=$file.Length;sha256=$sourceHash})
    $index++
    if($timer.Elapsed.TotalSeconds-$lastNotice -ge 20){Write-Output "Verified $index/$($files.Count)";$lastNotice=$timer.Elapsed.TotalSeconds}
}
$manifestPath=Join-Path $targetRoot 'Evidence\migration-to-x-manifest.json'
$manifest|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $manifestPath -Encoding UTF8
$receipt=[pscustomobject]@{source=$sourceRoot;destination=$targetRoot;source_preserved=$true;copied_files=$files.Count;copied_bytes=$totalBytes;verified_files=$manifest.Count;all_sha256_equal=$true;robocopy_exit=$copyExit;manifest_file='Evidence/migration-to-x-manifest.json';manifest_sha256=(Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant();elapsed_seconds=[Math]::Round($timer.Elapsed.TotalSeconds,2);verified_at_utc=[DateTime]::UtcNow.ToString('o')}
$receipt|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $targetRoot 'Evidence\migration-to-x-verification.json') -Encoding UTF8
$receipt|ConvertTo-Json -Compress|Write-Output
