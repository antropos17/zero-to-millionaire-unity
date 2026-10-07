using System;
using System.IO;
namespace ZeroToMillionaire
{
    public sealed class Level1SessionV7
    {
        readonly object gate=new object();readonly ILevel1StoreV7 store;readonly IRandomSource random;Level1ModelV7 model;bool presenting;
        public Level1ModelV7 Model{get{lock(gate)return model;}}
        public string Error{get;private set;}="";public string Notice{get;private set;}="";
        public bool PresentingSpin{get{lock(gate)return presenting;}}
        public Level1SessionV7(ILevel1StoreV7 store,IRandomSource random){this.store=store??throw new ArgumentNullException(nameof(store));this.random=random??throw new ArgumentNullException(nameof(random));string notice;model=new Level1ModelV7(store.Load(out notice));Notice=notice;if(store.RecoveryRequired)Error=notice;}
        bool Publish(Level1ModelV7 candidate)
        {
            if(store.RecoveryRequired){Error="Save requires recovery. Original files kept.";Notice=Error;return false;}
            try{string error;if(!store.Save(candidate.Snapshot,out error)){Error=error;Notice=error;return false;}}
            catch(IOException){Error=Notice="Save failed. No changes applied.";return false;}
            catch(UnauthorizedAccessException){Error=Notice="Save failed. No changes applied.";return false;}
            model=candidate;Error="";Notice="Saved locally · offline";return true;
        }
        public bool TryStartDecorating(){lock(gate){if(presenting||store.RecoveryRequired||!model.Complete)return false;if(model.Snapshot.decorStarted)return true;var c=new Level1ModelV7(model.Snapshot);return c.TryStartDecorating()&&Publish(c);}}
        public bool TryBeginSpin(out SpinResult result){lock(gate){result=null;if(presenting)return false;var c=new Level1ModelV7(model.Snapshot);SpinResult pending;if(!c.TrySpin(random,out pending)||!Publish(c))return false;presenting=true;result=pending;return true;}}
        public void FinishSpinPresentation(){lock(gate)presenting=false;}
        public bool TryBasePurchase(int index){lock(gate){if(presenting)return false;var c=new Level1ModelV7(model.Snapshot);return c.TryBasePurchase(index)&&Publish(c);}}
        public bool TryDecorPurchase(int index){lock(gate){if(presenting)return false;var c=new Level1ModelV7(model.Snapshot);return c.TryDecorPurchase(index)&&Publish(c);}}
        public bool TryRecover(){lock(gate){if(presenting)return false;var c=new Level1ModelV7(model.Snapshot);return c.TryRecover()&&Publish(c);}}
        public bool TrySelectStake(int value){lock(gate){if(presenting||store.RecoveryRequired||!Economy.ValidStake(value))return false;if(model.Snapshot.stakeMultiplier==value)return true;var c=new Level1ModelV7(model.Snapshot);return c.TrySelectStake(value)&&Publish(c);}}
        public bool Save(){lock(gate)return Publish(new Level1ModelV7(model.Snapshot));}
        public bool TryReset(bool confirmed)
        {
            lock(gate)
            {
                if(!confirmed||presenting||store.RecoveryRequired)return false;
                var fresh=new Level1ModelV7(new Level1StateV7());if(!Publish(fresh))return false;
                if(!Publish(new Level1ModelV7(fresh.Snapshot))){Error="";Notice="Reset saved; backup update pending.";}return true;
            }
        }
        public bool TryReload(){lock(gate){if(presenting)return false;string notice;var s=store.Load(out notice);if(store.RecoveryRequired){Error=Notice=notice;return false;}model=new Level1ModelV7(s);Error="";Notice=notice;return true;}}
    }
}
