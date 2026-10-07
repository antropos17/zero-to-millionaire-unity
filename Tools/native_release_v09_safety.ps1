param([string]$Exe,[string]$Prefix='native-v09-safe-final-1',[string]$Source)
$ErrorActionPreference='Stop'
$projectRoot='X:\Projects\ZERO-TO-MILLIONAIRE'
if($Prefix -notmatch '^[a-z0-9-]+$' -or $Source -notmatch '^[0-9a-f]{40}$'){throw 'Scoped prefix and immutable source required'}
$expectedExe=Join-Path $projectRoot 'Builds\Windows-v0.9-candidate-3\ZeroToMillionaire.exe'
if([IO.Path]::GetFullPath($Exe) -ne [IO.Path]::GetFullPath($expectedExe) -or !(Test-Path -LiteralPath $Exe)){throw 'Only the owned v0.9 release can be tested'}
$qaRoot=Join-Path $projectRoot ('.tooling\'+$Prefix)
if(Test-Path -LiteralPath $qaRoot){throw 'Fresh private root required'}
[void](New-Item -ItemType Directory -Path $qaRoot)
$cases=@(
 @{name='fresh';width=360;height=640;fixture=$null;stake=1;cash=0;spins=0;owned=0},
 @{name='legacy5';width=320;height=640;fixture=(Join-Path $projectRoot '.tooling\native-input-v081-preparation-20261007-1\profiles\ready-surplus-full\level1-v1.json');stake=1;cash=180;spins=6;owned=0},
 @{name='legacy-backup';width=360;height=800;fixture=(Join-Path $projectRoot '.tooling\native-input-v081-preparation-20261007-1\profiles\ready-surplus-full\level1-v1.json');stake=1;cash=180;spins=6;owned=0;fromBackup=$true},
 @{name='snapshot-blocked';width=320;height=640;fixture=(Join-Path $projectRoot '.tooling\native-input-v081-preparation-20261007-1\profiles\ready-surplus-full\level1-v1.json');stake=1;cash=180;spins=6;owned=0;blockSnapshot=$true},
 @{name='stake3';width=360;height=800;fixture=(Join-Path $projectRoot '.tooling\v09-ui-checks-5\save\level1-v1.json');stake=3;cash=$null;spins=$null;owned=$null},
 @{name='complete';width=480;height=960;fixture=(Join-Path $projectRoot 'Evidence\final-v09-level-recheck-2-completed-state.json');stake=1;cash=$null;spins=$null;owned=8}
)
$results=@()
foreach($case in $cases){
 $profile=Join-Path $qaRoot ($case.name+'\save');[void](New-Item -ItemType Directory -Path $profile)
 $fixtureState=$null
 if($case.fixture){
  if(!(Test-Path -LiteralPath $case.fixture)){throw 'Private fixture missing'}
  $fixtureState=Get-Content -LiteralPath $case.fixture -Raw|ConvertFrom-Json
  if($case.name-eq'stake3'-and$fixtureState.stakeMultiplier-ne3){throw 'UI-produced stake3 state required'}
  Copy-Item -LiteralPath $case.fixture -Destination (Join-Path $profile 'level1-v1.json')
 }
 $primary=Join-Path $profile 'level1-v1.json'
 if($case.fromBackup){Move-Item -LiteralPath $primary -Destination ($primary+'.bak');[IO.File]::WriteAllText($primary,"original corrupt primary`n")}
 if($case.blockSnapshot){[IO.File]::WriteAllText(($primary+'.pre-schema6'),'private deliberately blocked snapshot directory')}
 $originalPrimary=if(Test-Path -LiteralPath $primary){(Get-FileHash -LiteralPath $primary -Algorithm SHA256).Hash}else{$null}
 $originalBackup=if(Test-Path -LiteralPath ($primary+'.bak')){(Get-FileHash -LiteralPath ($primary+'.bak') -Algorithm SHA256).Hash}else{$null}
 $snapshotStamp=$null
 $runs=@()
 foreach($runName in @('first','relaunch')){
  $out=Join-Path $qaRoot ($case.name+'\'+$runName);[void](New-Item -ItemType Directory -Path $out)
  $p=Start-Process -FilePath $Exe -WorkingDirectory (Split-Path -Parent $Exe) -ArgumentList @('--ztm-private-profile',$profile,'--ztm-private-auto-quit','-screen-fullscreen','0','-screen-width',$case.width,'-screen-height',$case.height,'-force-d3d11','-logFile',(Join-Path $out 'Player.log')) -WindowStyle Hidden -PassThru
  for ($identityAttempt=0; $identityAttempt-lt20; $identityAttempt++) { $p.Refresh(); if ($p.Path-eq$Exe-or$p.HasExited) { break }; Start-Sleep -Milliseconds 100 }; if($p.Path-ne$Exe){throw 'Owned process path could not be verified; stopping before further work'}
  if(!$p.WaitForExit(25000)){throw "Private QA process $($p.Id) did not self-quit; no input/close fallback attempted"}
  if($p.ExitCode-ne0){throw 'Private QA player nonzero exit'}
  $readonly=Join-Path $profile 'readonly\release-readonly.json';$shot=Join-Path $profile 'readonly\native-first.png'
  if(!(Test-Path -LiteralPath $readonly)-or!(Test-Path -LiteralPath $shot)){throw 'Native evidence missing'}
  $info=Get-Content -LiteralPath $readonly -Raw|ConvertFrom-Json
  if($info.development-or$info.qaHooksPresent-or$info.automatedHarnessPresent-or!$info.profileVerified-or$info.lastFrame-le$info.firstFrame-or$info.recoveryRequired){throw 'Native clean release checks failed'}
  if($info.width-ne$case.width-or$info.height-ne$case.height-or$info.state.stakeMultiplier-ne$case.stake-or$info.state.version-ne6){throw 'Portrait or stake/schema mismatch'}
  if($case.cash-ne$null-and$info.state.cash-ne$case.cash){throw 'Expected cash mismatch'}
  if($case.spins-ne$null-and$info.state.spins-ne$case.spins){throw 'Expected spin count mismatch'}
  if($case.owned-ne$null-and$info.owned-ne$case.owned){throw 'Expected room ownership mismatch'}
  if($fixtureState){
   foreach($field in @('cash','spins','freeSpins','recoveries')){if($info.state.$field-ne$fixtureState.$field){throw "Fixture resource mismatch $field"}}
   foreach($field in @('fragments','purchased','lastGrid')){if(($info.state.$field-join',')-ne($fixtureState.$field-join',')){throw "Fixture arrays mismatch $field"}}
  }
  $disk=Get-Content -LiteralPath (Join-Path $profile 'level1-v1.json') -Raw|ConvertFrom-Json
  $snapshotPath=$null
  if($case.blockSnapshot){
   if($disk.version-ne5-or(Get-FileHash -LiteralPath $primary -Algorithm SHA256).Hash-ne$originalPrimary-or(Test-Path -LiteralPath ($primary+'.tmp'))){throw 'Blocked snapshot must prevent schema6 write and retain original bytes'}
  }else{
   if($disk.version-ne6-or$disk.stakeMultiplier-ne$case.stake-or$disk.cash-ne$info.state.cash-or$disk.spins-ne$info.state.spins){throw 'Normal private OnApplicationQuit persistence failed'}
   if($fixtureState-and$fixtureState.version-lt6){
    $snapshots=@(Get-ChildItem -LiteralPath ($primary+'.pre-schema6') -Directory)
    if($snapshots.Count-ne1-or!(Test-Path -LiteralPath (Join-Path $snapshots[0].FullName 'snapshot.txt'))){throw 'Exact committed native snapshot required'}
    $snapshotPath=$snapshots[0].FullName
    if((Get-FileHash -LiteralPath (Join-Path $snapshotPath 'primary.json') -Algorithm SHA256).Hash-ne$originalPrimary){throw 'Native migration snapshot changed original primary bytes'}
    if($originalBackup){if((Get-FileHash -LiteralPath (Join-Path $snapshotPath 'backup.json') -Algorithm SHA256).Hash-ne$originalBackup){throw 'Native snapshot changed original backup bytes'}}elseif(Test-Path -LiteralPath (Join-Path $snapshotPath 'backup.json')){throw 'Snapshot must record original backup absence'}
    $stamp=(Get-Item -LiteralPath (Join-Path $snapshotPath 'primary.json')).LastWriteTimeUtc.Ticks
    if($snapshotStamp-and$stamp-ne$snapshotStamp){throw 'Relaunch must never rewrite durable snapshot'}
    $snapshotStamp=$stamp
    $expectedBackupVersion=if($runName-eq'first'-and!$case.fromBackup){5}else{6}
    if(!$case.fromBackup-and(Get-Content -LiteralPath ($primary+'.bak') -Raw|ConvertFrom-Json).version-ne$expectedBackupVersion){throw 'Ordinary backup rotation mismatch'}
   }
  }
  $evidenceBase=Join-Path $projectRoot ('Evidence\'+$Prefix+'-'+$case.name+'-'+$runName)
  Copy-Item -LiteralPath $readonly -Destination ($evidenceBase+'.json')
  # Hidden windows did not render; preserve rejected PNG only in private profile.
  $runs+=@{name=$runName;pid=$p.Id;cash=$info.state.cash;spins=$info.state.spins;freeSpins=$info.state.freeSpins;stake=$info.state.stakeMultiplier;owned=$info.owned;fragments=$info.state.fragments;grid=$info.state.lastGrid;purchased=$info.state.purchased;frames=$info.lastFrame;samples=$info.samples;draws=$info.drawCounterNonzero;width=$info.width;height=$info.height;exit=0;physicalInputSent=$false;nativePixelsVerified=$false;renderDraws=$info.drawCounterNonzero;selfQuit=$true;durableSnapshot=$snapshotPath;privateProfile=$profile}
 }
 foreach($field in @('cash','spins','freeSpins','stake','owned')){if($runs[0].$field-ne$runs[1].$field){throw "Relaunch mismatch $field"}}
 foreach($field in @('fragments','grid','purchased')){if(($runs[0].$field-join',')-ne($runs[1].$field-join',')){throw "Relaunch arrays mismatch $field"}}
 $result=@{status='PASS';case=$case.name;source=$Source;exe=$Exe;runs=$runs;scope='Actual hidden native release schema/save self-quit/relaunch only; black captures rejected; no native render, performance or OS input coverage'}
 $results+=,$result
 $result|ConvertTo-Json -Depth 10|Set-Content -LiteralPath (Join-Path $projectRoot ('Evidence\'+$Prefix+'-'+$case.name+'-checks.json')) -Encoding utf8
 Write-Output ('PASS '+$case.name+' first/relaunch; exact private save and snapshot checks')
}
@{status='PASS';scope='12 hidden private native processes,4 configured portraits; includes exact migration snapshots, backup-source recovery and blocked snapshot writes; save/lifecycle only; no native rendering, performance or pointer/keyboard coverage';cases=$results}|ConvertTo-Json -Depth 12|Set-Content -LiteralPath (Join-Path $projectRoot ('Evidence\'+$Prefix+'-summary.json')) -Encoding utf8
