using System;
using System.IO;

namespace ZeroToMillionaire
{
    // Offline application boundary. Publish a candidate only after its disk commit.
    // A cosmetic spin lease rejects repeated input and installation during animation.
    public sealed class GameSession
    {
        readonly object gate=new object();
        readonly IStateStore store;
        readonly IRandomSource rewards;
        GameModel model;
        bool presentingSpin;
        public string Notice { get; private set; }
        public string Error { get; private set; }
        public GameModel Model { get {lock(gate)return model;} }
        public bool PresentingSpin { get {lock(gate)return presentingSpin;} }

        public GameSession(IStateStore stateStore,IRandomSource rewardRandom)
        {
            store=stateStore??throw new ArgumentNullException(nameof(stateStore));
            rewards=rewardRandom??throw new ArgumentNullException(nameof(rewardRandom));
            string notice;model=new GameModel(store.Load(out notice));Notice=notice;Error="";
        }
        public GameSession(IStateStore stateStore,IRandomSource rewardRandom,PlayerState initial)
            :this(stateStore,rewardRandom){model=new GameModel(initial);Notice="";}

        bool Write(PlayerState candidate)
        {
            string error;
            try{
                if(!store.Save(candidate,out error)){Error=error;Notice=error;return false;}
            }catch(IOException e){Error="Save failed: "+e.GetType().Name;Notice=Error;return false;}
            catch(UnauthorizedAccessException e){Error="Save failed: "+e.GetType().Name;Notice=Error;return false;}
            Error="";Notice="Saved locally · offline";return true;
        }
        bool Publish(GameModel candidate)
        {
            if(!Write(candidate.Snapshot))return false;
            model=candidate;return true;
        }
        public bool TryBeginSpin(out SpinResult result)
        {
            lock(gate){
                result=null;Error="";
                if(presentingSpin)return false;
                var candidate=new GameModel(model.Snapshot);SpinResult pending;
                if(!candidate.TrySpin(rewards,out pending)||!Publish(candidate))return false;
                presentingSpin=true;result=pending;return true;
            }
        }
        public void FinishSpinPresentation(){lock(gate)presentingSpin=false;}
        public bool TryPurchase(int index)
        {
            lock(gate){
                Error="";if(presentingSpin)return false;
                var candidate=new GameModel(model.Snapshot);
                return candidate.TryBuy(index)&&Publish(candidate);
            }
        }
        public bool TryRecover()
        {
            lock(gate){
                Error="";if(presentingSpin)return false;
                var candidate=new GameModel(model.Snapshot);
                return candidate.GrantRecovery()&&Publish(candidate);
            }
        }
        public bool TryReset()
        {
            lock(gate){
                Error="";if(presentingSpin)return false;
                var fresh=new GameModel();
                if(!Publish(fresh))return false;
                // Primary already commits the reset. A backup failure must not
                // resurrect the old live state; retry on a later save/pause.
                if(!Write(fresh.Snapshot)){Notice="Reset saved; backup update pending.";Error="";}
                return true;
            }
        }
        public bool TrySetTutorial(TutorialState tutorial){lock(gate){var candidate=new GameModel(model.Snapshot);candidate.SetTutorial(tutorial);return Publish(candidate);}}
        public bool Save(){lock(gate)return Write(model.Snapshot);}
        public void SetHeroinePosition(FloorPoint feet){lock(gate)model.SetHeroinePosition(feet);}
    }
}
