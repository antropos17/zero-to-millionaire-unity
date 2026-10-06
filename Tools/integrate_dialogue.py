from pathlib import Path
r=Path(r"X:\Projects\ZERO-TO-MILLIONAIRE")
p=r/'Assets/Scripts/Runtime/GameView.cs';s=p.read_text(encoding='utf-8')
s=s.replace('        private PanelSettings ownedPanel;','        private PanelSettings ownedPanel;\n        private DialoguePresenter dialogue;')
s=s.replace('            BuildUI();\n            Refresh();','            BuildUI();\n            dialogue=new DialoguePresenter(session,root,room,()=>spinning||rewardPending||rewardFly.style.display==DisplayStyle.Flex,Refresh);\n            Refresh();')
s=s.replace('            var reset=Action("RESET ROOM",ShowReset,Rust);','            var replay=Action("Повторить обучение",()=>{HideSettings();dialogue.Replay();},Forest);replay.name="tutorial-replay";replay.style.marginTop=8;settings.Add(replay);\n            var reset=Action("RESET ROOM",ShowReset,Rust);')
s=s.replace('            UpdateRewardFlight();','            UpdateRewardFlight();\n            if(dialogue!=null)dialogue.Tick();')
s=s.replace('model.Complete?"COMPLETE":spinning?"...":"SPIN"','model.Complete?"ГОТОВО":spinning?"...":"СПИН"')
s=s.replace('ButtonText(menuButton,"MENU",10);ButtonText(recoveryButton,"FREE SPIN",24);','ButtonText(menuButton,"МЕНЮ",10);ButtonText(recoveryButton,"БЕСПЛАТНО",22);')
s=s.replace('Action("BUY &\\nINSTALL",()=>Buy(index)','Action("КУПИТЬ",()=>Buy(index)')
# QA fixture replaces session; presenters must follow the replacement as well.
s=s.replace('session=new GameSession(saves,new SeededRandom(57),state); wanderer', 'session=new GameSession(saves,new SeededRandom(57),state); if(dialogue!=null){root.Q<VisualElement>("heroine-dialogue")?.RemoveFromHierarchy();dialogue=new DialoguePresenter(session,root,room,()=>spinning||rewardPending||rewardFly.style.display==DisplayStyle.Flex,Refresh);} wanderer')
# A confirmed reset creates fresh tutorial state in the existing session.
p.write_text(s,encoding='utf-8')
print('Dialogue presenter integrated')