using System;
using System.IO;
namespace ZeroToMillionaire
{
 public static class LayoutMigrationChecks
 {
  static int checks;static void Check(bool value,string label){checks++;if(!value)throw new Exception("LAYOUT MIGRATION: "+label);}
  public static string Run(IStateCodec codec,string folder){checks=0;int legacyPoints=0,moved=0;float max=0;
   for(int x=34;x<=266;x+=2)for(int y=34;y<=266;y+=2){var old=new FloorPoint(x/100f,y/100f);if(!RoomGeometry.IsFreeForSave(old,3))continue;legacyPoints++;var now=RoomGeometry.ProjectLegacyFeet(old);float d=FloorPoint.Distance(old,now);Check(RoomGeometry.IsFree(now),"every sampled valid legacy point remains loadable");Check(d<=RoomGeometry.MaximumLegacyRelocation,"bounded relocation");Check(FloorPoint.Distance(now,RoomGeometry.ProjectLegacyFeet(old))<.000001f,"deterministic nearest projection");if(d>0){moved++;max=Math.Max(max,d);}else Check(now.U==old.U&&now.V==old.V,"already clear coordinates unchanged");}
   var source=new PlayerState{version=3,cash=321,spins=19,freeSpins=2,recoveries=7,heroineU=1.90f,heroineV=2.30f};source.purchased[3]=true;source.purchased[6]=true;source.fragments[0]=17;source.fragments[7]=99;source.quests=Economy.QuestRecords(source);string reason,error,notice;
   Check(GameModel.IsPersistedValid(source,out reason)&&!RoomGeometry.IsFree(new FloorPoint(source.heroineU,source.heroineV)),"old valid position is now reserved");
   foreach(int version in new[]{2,3}){source.version=version;var migrated=new GameModel(source).Snapshot;Check(migrated.version==4&&RoomGeometry.IsFree(new FloorPoint(migrated.heroineU,migrated.heroineV)),"schema2/3 position migrates to4");Check(migrated.cash==321&&migrated.spins==19&&migrated.freeSpins==2&&migrated.recoveries==7&&migrated.fragments[0]==17&&migrated.fragments[7]==99&&migrated.purchased[3]&&migrated.purchased[6],"all progress and out-of-order ownership kept");Check(codec.Encode(new GameModel(migrated).Snapshot)==codec.Encode(migrated),"migration is idempotent");
    string path=Path.Combine(folder,"layout-legacy-"+version+".json");File.WriteAllText(path,codec.Encode(source));var disk=new SaveStore(path,codec);var loaded=disk.Load(out notice);Check(loaded.cash==321&&loaded.version==4&&notice.Contains("upgraded"),"primary legacy save upgraded without fallback/reset");Check(disk.Save(loaded,out error)&&disk.Load(out notice).heroineU==loaded.heroineU,"migrated coordinate survives relaunch");}
   source.version=4;Check(!GameModel.IsPersistedValid(source,out reason),"new schema cannot claim blocked coordinates as legacy");source.version=3;source.heroineU=float.NaN;Check(!GameModel.IsPersistedValid(source,out reason),"nonfinite legacy feet rejected");source.heroineU=0;Check(!GameModel.IsPersistedValid(source,out reason),"outside legacy bounds rejected");source.heroineU=1.9f;source.quests[1].id=source.quests[0].id;Check(!GameModel.IsPersistedValid(source,out reason),"legacy schema3 still validates raw stable IDs");
   var walker=new RoomWanderer(new FloorPoint(1.85f,1.45f),new SeededRandom(57));for(int i=0;i<100;i++)walker.Step(1f/60);var before=walker.Position;float travel=walker.Travelled;for(int i=0;i<30;i++)walker.HoldForTurn();Check(FloorPoint.Distance(before,walker.Position)==0&&walker.Travelled==travel&&!walker.Walking,"planted hold neither moves nor teleports");for(int i=0;i<100;i++)walker.Step(1f/60);Check(RoomGeometry.IsFree(walker.Position)&&walker.Travelled>travel,"turn resumes valid path smoothly");
   return "PASS "+checks+" layout assertions; "+legacyPoints+" valid legacy gridpoints, "+moved+" projected; max_relocation_m="+max.ToString("F3")+"; schema2/3->4 keeps balances/owned/parts; reload/idempotence/raw rejection and planted hold";
  }
 }
}
