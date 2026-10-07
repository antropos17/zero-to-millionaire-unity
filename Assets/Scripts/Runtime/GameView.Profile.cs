using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public sealed partial class GameView
    {
        enum RoomSection { Room, Shop, Profile, Settings }
        LocalPlayerStore localPlayer;
        readonly SessionNotifications notifications = new SessionNotifications();
        VisualElement sections, sectionsPanel, sectionsBody, profile, sectionContent;
        Label sectionTitle, sectionError, roomError, profileStatus, profileStats, activityEmpty;
        VisualElement activityList;
        TextField profileName;
        Button notificationToggle, saveProfile, retryLocal;
        readonly Button[] sectionButtons = new Button[4], avatarButtons = new Button[3];
        readonly string[] avatars = { "heroine", "lamp", "stool" };
        string draftAvatar;
        RoomSection section;
        VisualElement sectionOpener;
        float sectionStarted;
        bool sectionEntering;
        long stakeEventId;
        bool SectionsOpen => sections != null && sections.style.display == DisplayStyle.Flex;

        void BuildSections()
        {
            sections = Overlay("sections-overlay", out sectionsPanel);
            sectionsPanel.name="sections-panel";sectionsPanel.style.height = Length.Percent(95);
            var heading = new VisualElement(); Row(heading); heading.style.flexShrink=0; heading.style.marginBottom=6;
            sectionTitle=Text("MY ROOM",20,PixelUi.Hex(0xffc472));sectionTitle.style.flexGrow=1;heading.Add(sectionTitle);
            var close=Action("BACK",CloseSections,Rust);close.name="close-sections";close.style.width=68;ButtonText(close,"BACK",13);heading.Add(close);sectionsPanel.Add(heading);
            var tabs=new VisualElement {name="section-tabs"};Row(tabs);tabs.style.flexShrink=0;tabs.style.marginBottom=8;
            string[] titles={"ROOM","SHOP","PROFILE","SETTINGS"};
            for(int i=0;i<4;i++)
            {
                int choice=i;var b=Action(titles[i],()=>SwitchSection((RoomSection)choice),Forest);b.name="section-"+titles[i].ToLowerInvariant();
                b.style.flexGrow=1;b.style.flexBasis=0;b.style.minWidth=0;b.style.marginRight=i<3?3:0;ButtonText(b,titles[i],11);
                sectionButtons[i]=b;tabs.Add(b);
            }
            sectionsPanel.Add(tabs);
            sectionError=Text("",UiTypography.Counter,PixelUi.Hex(0xffce76));sectionError.name="section-error";sectionError.style.flexShrink=0;sectionError.style.marginBottom=6;sectionsPanel.Add(sectionError);
            sectionsBody=new VisualElement {name="sections-body"};sectionsBody.style.flexGrow=1;sectionsBody.style.minHeight=0;sectionsPanel.Add(sectionsBody);

            // Reuse the existing shop content inside the same navigation shell.
            shopOverlay.RemoveFromHierarchy();shopOverlay.style.position=Position.Relative;
            shopOverlay.style.left=shopOverlay.style.right=shopOverlay.style.top=shopOverlay.style.bottom=StyleKeyword.Auto;
            shopOverlay.style.flexGrow=1;shopOverlay.style.minHeight=0;shopOverlay.style.alignItems=Align.Stretch;shopOverlay.style.justifyContent=Justify.FlexStart;
            shopOverlay.style.backgroundColor=Color.clear;
            var shopPanel=shopOverlay.Q<VisualElement>("shop-overlay-panel");shopPanel.style.width=Length.Percent(100);shopPanel.style.height=Length.Percent(100);
            shopPanel.style.maxWidth=shopPanel.style.maxHeight=StyleKeyword.None;
            shopPanel.style.paddingLeft=shopPanel.style.paddingRight=shopPanel.style.paddingTop=shopPanel.style.paddingBottom=0;
            // The common heading provides the section title and Back control.
            shopPanel[0].style.display=DisplayStyle.None;
            sectionsBody.Add(shopOverlay);

            settings.RemoveFromHierarchy();settings.style.position=Position.Relative;
            settings.style.left=settings.style.right=settings.style.top=StyleKeyword.Auto;
            settings.style.paddingLeft=settings.style.paddingRight=settings.style.paddingTop=settings.style.paddingBottom=0;
            settings.style.flexGrow=1;settings.style.minHeight=0;
            var settingsScroll=new ScrollView(ScrollViewMode.Vertical){name="settings-scroll"};settingsScroll.style.flexGrow=1;settingsScroll.style.minHeight=0;
            settingsScroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
            while(settings.childCount>0){var child=settings[0];child.RemoveFromHierarchy();child.style.flexShrink=0;settingsScroll.Add(child);}
            settingsScroll.contentContainer[0].style.display=DisplayStyle.None;
            notificationToggle=Action("",ToggleNotifications,Forest);notificationToggle.name="notification-toggle";notificationToggle.style.marginTop=8;
            settingsScroll.Insert(4,notificationToggle);
            var offline=Text("Local profile and settings. No login required. This prototype has no audio.",UiTypography.Body,Cream);offline.name="settings-offline";UiTypography.Apply(offline,UiTypography.Body,Cream,false);offline.style.marginTop=10;settingsScroll.Add(offline);
            retryLocal=Action("RETRY LOCAL DATA",RetryLocalPlayer,Rust);retryLocal.name="retry-local-data";retryLocal.style.marginTop=8;settingsScroll.Add(retryLocal);
            settings.Add(settingsScroll);sectionsBody.Add(settings);

            profile=new ScrollView(ScrollViewMode.Vertical){name="profile"};profile.style.flexGrow=1;profile.style.minHeight=0;
            ((ScrollView)profile).horizontalScrollerVisibility=ScrollerVisibility.Hidden;
            var nameHelp=Text("Your local display name",UiTypography.Body,Cream);UiTypography.Apply(nameHelp,UiTypography.Body,Cream,false);profile.Add(nameHelp);
            profileName=new TextField {name="profile-name",maxLength=40};profileName.style.height=46;profileName.style.minHeight=46;profileName.style.marginTop=5;profileName.style.marginBottom=8;
            UiTypography.Apply(profileName,18,Forest,false);profileName.style.backgroundColor=Cream;profile.Add(profileName);
            var inputText=profileName.Q<VisualElement>(className:"unity-text-input");if(inputText!=null){inputText.style.backgroundColor=Cream;inputText.style.color=Forest;inputText.style.paddingLeft=8;UiTypography.Apply(inputText,18,Forest,false);}
            profileName.RegisterValueChangedCallback(_=>ValidateProfileDraft());
            var avatarHelp=Text("Choose a profile badge",UiTypography.Body,Cream);UiTypography.Apply(avatarHelp,UiTypography.Body,Cream,false);profile.Add(avatarHelp);
            var badges=new VisualElement {name="profile-badges"};Row(badges);badges.style.marginTop=5;badges.style.marginBottom=8;
            string[] captions={"HEROINE","LAMP","STOOL"};
            for(int i=0;i<3;i++)
            {
                int index=i;var b=Action(captions[i],()=>{draftAvatar=avatars[index];RefreshAvatarChoices();ValidateProfileDraft();},Forest);
                b.name="avatar-"+avatars[i];b.style.flexGrow=1;b.style.flexBasis=0;b.style.height=86;b.style.marginRight=i<2?6:0;
                b.style.flexDirection=FlexDirection.Column;b.style.justifyContent=Justify.Center;ButtonText(b,captions[i],12);
                Texture2D texture=Resources.Load<Texture2D>(i==0?"Art/animation/front-idle":"Art/item-symbols/"+avatars[i]);
                var image=Icon(texture,46);image.name="profile-avatar-image-"+i;if(i==0)image.uv=new Rect(38f/128,130f/192,58f/128,58f/192);
                image.style.alignSelf=Align.Center;image.pickingMode=PickingMode.Ignore;b.Insert(0,image);avatarButtons[i]=b;badges.Add(b);
            }
            profile.Add(badges);
            profileStatus=Text("",UiTypography.Counter,PixelUi.Hex(0xffce76));profileStatus.name="profile-status";profileStatus.style.marginBottom=6;profile.Add(profileStatus);
            saveProfile=Action("SAVE PROFILE",SaveProfile,Rust);saveProfile.name="save-profile";profile.Add(saveProfile);
            profileStats=Text("",UiTypography.Body,Cream);profileStats.name="profile-stats";UiTypography.Apply(profileStats,UiTypography.Body,Cream,false);profileStats.style.marginTop=14;profileStats.style.marginBottom=12;profile.Add(profileStats);
            var activityTitle=Text("THIS SESSION",16,PixelUi.Hex(0xffc472));profile.Add(activityTitle);
            var activityHelp=Text("Only actions committed since this launch. Up to 20 entries; history clears on relaunch or a confirmed reset.",UiTypography.Counter,Cream);UiTypography.Apply(activityHelp,UiTypography.Counter,Cream,false);activityHelp.style.marginBottom=7;profile.Add(activityHelp);
            activityEmpty=Text("No activity yet.",UiTypography.Body,Cream);activityEmpty.name="activity-empty";profile.Add(activityEmpty);
            activityList=new VisualElement {name="session-activity"};profile.Add(activityList);sectionsBody.Add(profile);

            roomError=Text("",UiTypography.Counter,Rust);roomError.name="critical-status";roomError.style.flexShrink=0;roomError.style.paddingLeft=4;roomError.style.paddingRight=4;
            roomError.style.marginBottom=2;main.Insert(2,roomError);
            profile.style.display=settings.style.display=shopOverlay.style.display=DisplayStyle.None;
            notifications.SetOptional(localPlayer.Snapshot.Notifications);notifications.Reset(model.Snapshot,localPlayer.Snapshot.revision);
            RefreshLocalSettings();SyncCritical();
        }
        void OpenSection(RoomSection target,VisualElement opener)
        {
            if(StakeOpen||session.RecoveryRequired||confirmation.style.display==DisplayStyle.Flex)return;
            if(spinning){pendingMenu=true;reelMotion.Skip();return;}
            bool first=!SectionsOpen;
            if(first){sectionOpener=opener;LoadProfileDraft();}
            HideToast();main.SetEnabled(false);sections.style.display=DisplayStyle.Flex;sections.BringToFront();SwitchSection(target);
        }
        void SwitchSection(RoomSection target)
        {
            if(target==RoomSection.Room){CloseSections();return;}
            if(!SectionsOpen)return;
            input?.Close(sections,false);section=target;
            shopOverlay.style.display=target==RoomSection.Shop?DisplayStyle.Flex:DisplayStyle.None;
            settings.style.display=target==RoomSection.Settings?DisplayStyle.Flex:DisplayStyle.None;
            profile.style.display=target==RoomSection.Profile?DisplayStyle.Flex:DisplayStyle.None;
            sectionTitle.text=target==RoomSection.Shop?"ROOM SHOP":target==RoomSection.Profile?"LOCAL PROFILE":"SETTINGS";
            for(int i=0;i<4;i++){sectionButtons[i].style.backgroundColor=i==(int)target?Rust:Forest;sectionButtons[i].tooltip=i==(int)target?"Current section":"Open "+((RoomSection)i).ToString();}
            UpdateSaveStatus();RefreshLocalSettings();RefreshProfile();
            VisualElement first=sections.Q<Button>("close-sections");
            if(target==RoomSection.Shop){RefreshShop();shopList.scrollOffset=Vector2.zero;sectionContent=shopOverlay;}
            else if(target==RoomSection.Settings){sectionContent=settings;first=settings.Q<Button>("tutorial-replay");}
            else{sectionContent=profile;first=profileName;}
            sectionEntering=!presentation.ReducedMotion;sectionStarted=Time.unscaledTime;sectionContent.style.opacity=sectionEntering?.35f:1;
            input?.Open(sections,first,CloseSections,sectionOpener);
        }
        void CloseSections()
        {
            if(sections==null)return;
            bool shown=SectionsOpen;sections.style.display=DisplayStyle.None;
            shopOverlay.style.display=settings.style.display=profile.style.display=DisplayStyle.None;
            sectionEntering=false;if(sectionContent!=null)sectionContent.style.opacity=1;
            if(shown){input?.Close(sections);LoadProfileDraft();main.SetEnabled(!session.RecoveryRequired);}
        }
        void LoadProfileDraft(){if(profileName==null)return;var p=localPlayer.Snapshot;profileName.SetValueWithoutNotify(p.displayName);draftAvatar=p.avatarId;RefreshAvatarChoices();ValidateProfileDraft();}
        void RefreshAvatarChoices(){for(int i=0;i<3;i++){avatarButtons[i].style.backgroundColor=draftAvatar==avatars[i]?Rust:Forest;avatarButtons[i].tooltip=draftAvatar==avatars[i]?"Selected badge":"Choose "+avatars[i];}}
        void ValidateProfileDraft()
        {
            if(saveProfile==null)return;string name;bool valid=LocalPlayer.NormalizeName(profileName.value,out name);
            saveProfile.SetEnabled(valid&&!localPlayer.Blocked);
            profileStatus.text=!valid?"Use 1–20 letters or numbers, spaces, apostrophes or hyphens.":localPlayer.Error;
        }
        void SaveProfile()
        {
            string name;if(!SectionsOpen||section!=RoomSection.Profile||!LocalPlayer.NormalizeName(profileName.value,out name))return;
            var p=localPlayer.Snapshot;p.displayName=name;p.avatarId=draftAvatar;
            bool changed;if(!localPlayer.Save(p,out changed)){SyncCritical();ValidateProfileDraft();return;}
            profileName.SetValueWithoutNotify(localPlayer.Snapshot.displayName);ValidateProfileDraft();SyncCritical();
            if(changed)Record(SessionEvent.Profile,localPlayer.Snapshot.revision,"Profile saved · "+name);
            else if(string.IsNullOrEmpty(localPlayer.Error))profileStatus.text="Profile is up to date.";
            RefreshProfile();
        }
        void ToggleNotifications()
        {
            var p=localPlayer.Snapshot;p.notificationMode=p.Notifications?"off":"on";bool changed;
            if(localPlayer.Save(p,out changed)){notifications.SetOptional(localPlayer.Snapshot.Notifications);if(changed)Record(SessionEvent.Setting,localPlayer.Snapshot.revision,"Reward notifications "+(localPlayer.Snapshot.Notifications?"on":"off"));if(!localPlayer.Snapshot.Notifications)HideToast();}
            RefreshLocalSettings();SyncCritical();
        }
        void MotionChanged(bool changed)
        {
            if(changed)Record(SessionEvent.Setting,localPlayer.Snapshot.revision,"Animation "+(presentation.ReducedMotion?"calm":"full"));
            if(presentation.ReducedMotion){sectionEntering=false;if(sectionContent!=null)sectionContent.style.opacity=1;}
            RefreshLocalSettings();SyncCritical();
        }
        void RetryLocalPlayer()
        {
            if(localPlayer.Reload()){notifications.RebaseLocal(localPlayer.Snapshot.revision);PixelUi.ReducedMotion=presentation.ReducedMotion;notifications.SetOptional(localPlayer.Snapshot.Notifications);LoadProfileDraft();effects.Clear();}
            RefreshLocalSettings();SyncCritical();RefreshProfile();
        }
        void RefreshLocalSettings()
        {
            if(notificationToggle==null)return;
            ButtonText(notificationToggle,localPlayer.Snapshot.Notifications?"REWARD NOTICES: ON":"REWARD NOTICES: OFF",13);
            ButtonText(motionToggle,presentation.ReducedMotion?"ANIMATION: CALM":"ANIMATION: FULL",14);
            retryLocal.style.display=localPlayer.Blocked?DisplayStyle.Flex:DisplayStyle.None;
            motionToggle.SetEnabled(!localPlayer.Blocked);notificationToggle.SetEnabled(!localPlayer.Blocked);ValidateProfileDraft();
        }
        void RefreshProfile()
        {
            if(profileStats==null)return;var s=model.Snapshot;
            profileStats.text="SAVED PROGRESS\nCoins: "+s.cash+"\nSpins this save: "+s.spins+"\nRoom renovation: "+model.PurchasedCount+"/8\nSelected stake: "+model.SelectedStake+"x";
            activityList.Clear();var entries=notifications.History;activityEmpty.style.display=entries.Length==0?DisplayStyle.Flex:DisplayStyle.None;
            for(int i=entries.Length-1;i>=0;i--){var l=Text(entries[i],UiTypography.Counter,Cream);l.enableRichText=false;l.style.marginBottom=6;UiTypography.Apply(l,UiTypography.Counter,Cream,false);activityList.Add(l);}
        }
        void Record(SessionEvent channel,long id,string text){if(notifications.Record(notifications.Generation,channel,id,text)&&section==RoomSection.Profile&&SectionsOpen)RefreshProfile();}
        void SyncCritical()
        {
            if(localPlayer==null)return;
            bool gameWarning=!string.IsNullOrEmpty(saveError)||session.RecoveryRequired||session.Notice==SaveStore.MigrationSnapshotError||session.Notice==SaveStore.SaveChangedNotice||session.Notice=="Reset saved; backup update pending.";
            notifications.SetCritical(0,gameWarning?EnglishUi.SaveNotice(session.Notice,!string.IsNullOrEmpty(saveError)):"");
            notifications.SetCritical(1,localPlayer.Error);
            string message=notifications.Critical;
            if(roomError!=null){roomError.text=message;roomError.style.display=message.Length==0?DisplayStyle.None:DisplayStyle.Flex;}
            if(sectionError!=null){sectionError.text=message;sectionError.style.display=message.Length==0?DisplayStyle.None:DisplayStyle.Flex;}
        }
        void TickSections()
        {
            if(sectionEntering){float t=Mathf.Clamp01((Time.unscaledTime-sectionStarted)/.14f);sectionContent.style.opacity=.35f+.65f*t;if(t>=1)sectionEntering=false;}
            if(notifications.PendingCount>0&&!spinning&&!ModalOpen&&!rewardPending&&rewardFly.style.display==DisplayStyle.None&&!effects.PresentationBusy&&(toast.style.display==DisplayStyle.None||Time.unscaledTime>toastUntil))ShowToast(notifications.Take());
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD || ZTM_RELEASE_AUDIT
        public LocalPlayer QaLocalPlayer=>localPlayer.Snapshot;
        public SessionNotifications QaNotifications=>notifications;
#endif
    }
}
