using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
 public sealed class DialoguePresenter
 {
  readonly GameSession session;readonly VisualElement ui,room,bubble;readonly Label words,title;readonly Button next,skip;
  readonly UiInputCoordinator input;bool focusReplayNext;readonly Func<bool> busy;readonly Action changed;VisualElement ring;int replay=-1,story=-1,storyMask,lastMode=-99;float nextCheck,enterAt;IVisualElementScheduledItem entrance;
  static readonly Color Cream=new Color32(249,237,217,255),Forest=new Color32(20,44,40,255),Rust=new Color32(186,75,45,255);
  public DialoguePresenter(GameSession session,VisualElement root,VisualElement room,Func<bool> busy,Action changed){
   this.session=session;input=UiInputCoordinator.ForRoot(root);ui=root;this.room=room;this.busy=busy;this.changed=changed;
   bubble=new VisualElement{name="heroine-dialogue"};PixelUi.Skin(bubble,PixelUi.Finish.Cream);bubble.style.position=Position.Absolute;bubble.style.left=2;bubble.style.right=48;bubble.style.top=42;bubble.style.height=150;bubble.style.paddingLeft=bubble.style.paddingRight=8;bubble.style.paddingTop=5;bubble.style.paddingBottom=6;
   title=Text("One small step",UiTypography.Meta);title.name="dialogue-title";title.style.height=16;bubble.Add(title);
   var line=new VisualElement();line.style.flexDirection=FlexDirection.Row;line.style.flexGrow=1;
   var avatar=new Image{image=Resources.Load<Texture2D>("Art/animation/front-idle"),uv=new Rect(38f/128,130f/192,58f/128,58f/192),scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore,name="dialogue-portrait"};avatar.style.width=36;avatar.style.height=43;avatar.style.flexShrink=0;avatar.style.marginRight=5;line.Add(avatar);
   words=Text("",UiTypography.DialogueBody);words.name="dialogue-text";words.style.flexGrow=1;words.style.flexShrink=1;words.style.whiteSpace=WhiteSpace.Normal;words.style.unityTextAlign=TextAnchor.MiddleLeft;line.Add(words);bubble.Add(line);
   var actions=new VisualElement();actions.style.flexDirection=FlexDirection.Row;actions.style.height=45;actions.style.flexShrink=0;
   next=Button("Next",Advance,"tutorial-next");next.style.width=62;next.style.marginRight=4;actions.Add(next);
   skip=Button("Skip tutorial",Skip,"tutorial-skip");skip.style.flexGrow=1;actions.Add(skip);bubble.Add(actions);room.Add(bubble);bubble.style.display=DisplayStyle.None;
  }
  static Label Text(string text,int size){var l=new Label(text);UiTypography.Apply(l,size,Forest,false);l.style.marginTop=l.style.marginBottom=l.style.marginLeft=l.style.marginRight=0;l.style.paddingTop=l.style.paddingBottom=l.style.paddingLeft=l.style.paddingRight=0;return l;}
  static Button Button(string text,Action action,string name){Button b=null;b=new Button(()=>{if(!UiInputCoordinator.CanActivate(b))return;UiInputCoordinator.NoteAction(b);action();}){name=name};PixelUi.Skin(b,PixelUi.Finish.Forest);b.style.height=45;b.style.minHeight=45;b.style.minWidth=45;b.style.flexShrink=0;b.style.marginLeft=b.style.marginRight=b.style.marginTop=b.style.marginBottom=0;b.style.paddingLeft=b.style.paddingRight=2;var l=Text(text,UiTypography.SmallAction);l.name="button-caption";l.style.color=Cream;l.style.unityTextAlign=TextAnchor.MiddleCenter;l.style.flexGrow=1;l.pickingMode=PickingMode.Ignore;b.Add(l);return b;}
  void Caption(Button b,string text){b.Q<Label>("button-caption").text=text;}
  void FinishEntrance(){entrance?.Pause();entrance=null;bubble.style.opacity=1;bubble.style.translate=new Translate(0,0,0);}
  void Hide(){FinishEntrance();bubble.style.display=DisplayStyle.None;}
  public void ResetUi(){FinishEntrance();focusReplayNext=false;replay=story=-1;lastMode=-99;nextCheck=0;if(ring!=null){ring.RemoveFromHierarchy();ring=null;}bubble.style.display=DisplayStyle.None;}
  public void Dispose(){ResetUi();bubble.RemoveFromHierarchy();}
  public void Replay(){focusReplayNext=true;replay=0;story=-1;lastMode=-99;nextCheck=0;Tick();}
  void Advance(){
   if(replay>=0){if(++replay>4){replay=-1;input?.FocusAfterLayout(ui.Q<Button>("menu"));}else focusReplayNext=true;lastMode=-99;nextCheck=0;Tick();return;}
   if(story>=0){DismissStory();return;}
   var s=session.Model.Snapshot;if(TutorialPolicy.CanNext(s)){bool saved=session.TrySetTutorial(TutorialPolicy.Advance(s));changed();if(saved){lastMode=-99;nextCheck=0;Tick();}}
  }
  void Skip(){
   if(replay>=0){replay=-1;focusReplayNext=false;input?.FocusAfterLayout(ui.Q<Button>("menu"));lastMode=-99;nextCheck=0;Tick();return;}if(story>=0){DismissStory();return;}
   var s=session.Model.Snapshot;bool saved=session.TrySetTutorial(TutorialPolicy.Skip(s));changed();if(saved){lastMode=-99;nextCheck=0;Tick();}
  }
  void DismissStory(){var state=session.Model.Snapshot.tutorial.Clone();state.storySeenMask|=storyMask;bool saved=session.TrySetTutorial(state);changed();if(saved){story=-1;storyMask=0;lastMode=-99;nextCheck=0;Tick();}}
   VisualElement HighlightTarget(int step){return step==1?ui.Q<Button>("spin"):step==2?ui.Q<VisualElement>("active-quest"):step==3?ui.Q<Button>("buy-"+session.Model.ActiveQuest):null;}
   void Highlight(int step){
   if(ring!=null){ring.RemoveFromHierarchy();ring=null;}
   VisualElement target=HighlightTarget(step);
   if(target==null)return;ring=new VisualElement{name="tutorial-highlight",pickingMode=PickingMode.Ignore};ring.style.position=Position.Absolute;ring.style.left=ring.style.top=-2;ring.style.right=ring.style.bottom=-2;ring.style.borderLeftWidth=ring.style.borderRightWidth=ring.style.borderTopWidth=ring.style.borderBottomWidth=2;ring.style.borderLeftColor=ring.style.borderRightColor=ring.style.borderTopColor=ring.style.borderBottomColor=Rust;target.Add(ring);
  }
  void AnimateEntrance(){if(bubble.style.display!=DisplayStyle.Flex)return;float t=PixelUi.ReducedMotion?1:Mathf.Clamp01((Time.unscaledTime-enterAt)/.16f);bubble.style.opacity=t;bubble.style.translate=new Translate(0,6*Mathf.Pow(1-t,3),0);}
  void Enter(){entrance?.Pause();enterAt=Time.unscaledTime;AnimateEntrance();if(!PixelUi.ReducedMotion)entrance=bubble.schedule.Execute(()=>{AnimateEntrance();if(Time.unscaledTime-enterAt>=.16f)entrance?.Pause();}).Every(16);}
  public void Tick(){
   if(Time.unscaledTime<nextCheck)return;nextCheck=Time.unscaledTime+.12f;
   if(busy()){Hide();if(ring!=null)ring.style.display=DisplayStyle.None;return;}
   var s=session.Model.Snapshot;int step=replay>=0?replay:TutorialPolicy.Step(s);
   if(step<0&&story<0){int pending=TutorialPolicy.PendingStory(s);if(pending>=0){story=pending;storyMask=TutorialPolicy.MetMilestones(s);}}
   int mode=story>=0?10+story:step;
   if(mode<0){Hide();if(ring!=null){ring.RemoveFromHierarchy();ring=null;}lastMode=-99;return;}
   bubble.style.display=DisplayStyle.Flex;bubble.BringToFront();if(ring!=null)ring.style.display=DisplayStyle.Flex;
   bool canNext=replay>=0||story>=0||TutorialPolicy.CanNext(s);next.style.display=canNext?DisplayStyle.Flex:DisplayStyle.None;
   Caption(next,story>=0||step==4?"Done":"Next");Caption(skip,replay>=0||story>=0?"Close":"Skip tutorial");
   if(story<0&&mode>=0&&(ring==null||ring.parent!=HighlightTarget(step)||ring.panel!=ui.panel))Highlight(step);
    if(lastMode!=mode){Enter();title.text=replay>=0?"Tutorial replay · no rewards":story>=0?"Our small room":"One small step";
    words.text=story>=0?TutorialPolicy.Stories[story]:replay==3?"When BOTH bars are full, press BUY to install the upgrade yourself.":TutorialPolicy.Steps[step];Highlight(story>=0?-1:step);lastMode=mode;}
    if(focusReplayNext&&replay>=0){focusReplayNext=false;input?.FocusAfterLayout(next);}
  }
 }
}