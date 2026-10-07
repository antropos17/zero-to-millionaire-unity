using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public sealed partial class GameView
    {
        VisualElement stakeOverlay, shopOverlay;
        ScrollView shopList;
        Button stakeButton, shopButton;
        Label stakeTitle, stakeBalance, shopSummary, shopNotice;
        readonly Button[] stakeChoices = new Button[3];
        SpinResult activeSpin;

        VisualElement Overlay(string name, out VisualElement panel)
        {
            var overlay = new VisualElement { name = name };
            overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.right = overlay.style.top = overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(Forest.r, Forest.g, Forest.b, .78f);
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            panel = Frame(name + "-panel", PixelUi.Finish.Forest);
            panel.style.width = Length.Percent(93);
            panel.style.maxWidth = 420;
            panel.style.maxHeight = Length.Percent(95);
            panel.style.paddingLeft = panel.style.paddingRight = 12;
            panel.style.paddingTop = panel.style.paddingBottom = 12;
            overlay.Add(panel);
            overlay.style.display = DisplayStyle.None;
            root.Add(overlay);
            return overlay;
        }

        void BuildStakesShop()
        {
            stakeOverlay = Overlay("stake-overlay", out var stakePanel);
            var title = Text("CHOOSE STAKE", 20, PixelUi.Hex(0xffc472)); title.style.marginBottom = 8; stakePanel.Add(title);
            stakeBalance = Text("", UiTypography.Body, Cream); stakeBalance.name = "stake-balance"; stakeBalance.style.marginBottom = 8; stakePanel.Add(stakeBalance);
            var rules = Text(EnglishUi.StakeRules, UiTypography.Body, Cream); rules.name = "stake-rules";
            UiTypography.Apply(rules, UiTypography.Body, Cream, false); rules.style.marginBottom = 10; stakePanel.Add(rules);
            for (int value = 1; value <= 3; value++)
            {
                int selected = value;
                var choice = Action("", () => SelectStake(selected), Forest);
                choice.name = "stake-" + value; choice.style.height = 48; choice.style.marginBottom = 6;
                stakeChoices[value - 1] = choice; stakePanel.Add(choice);
            }
            var closeStake = Action("BACK TO ROOM", HideStakes, Rust); closeStake.name = "close-stakes"; closeStake.style.marginTop = 4; stakePanel.Add(closeStake);

            shopOverlay = Overlay("shop-overlay", out var shopPanel);
            shopPanel.style.height = Length.Percent(95);
            var heading = new VisualElement(); Row(heading); heading.style.flexShrink = 0; heading.style.marginBottom = 8;
            var shopTitle = Text("ROOM SHOP", 20, PixelUi.Hex(0xffc472)); shopTitle.style.flexGrow = 1; heading.Add(shopTitle);
            var closeShop = Action("BACK", HideShop, Rust); closeShop.name = "close-shop"; closeShop.style.width = 74; heading.Add(closeShop); shopPanel.Add(heading);
            shopSummary = Text("", UiTypography.Body, Cream); shopSummary.name = "shop-summary"; shopSummary.style.flexShrink = 0; shopSummary.style.marginBottom = 6; shopPanel.Add(shopSummary);
            var help = Text("Each upgrade needs BOTH coins and its matching parts. Only your current task can be installed.", UiTypography.Body, Cream);
            help.name = "shop-rules"; UiTypography.Apply(help, UiTypography.Body, Cream, false); help.style.flexShrink = 0; help.style.marginBottom = 6; shopPanel.Add(help);
            shopNotice = Text("", UiTypography.Counter, PixelUi.Hex(0xffce76)); shopNotice.name = "shop-notice"; shopNotice.style.flexShrink = 0; shopPanel.Add(shopNotice);
            shopList = new ScrollView(ScrollViewMode.Vertical) { name = "shop-catalog" };
            shopList.style.flexGrow = 1; shopList.style.minHeight = 0; shopList.style.marginTop = 4;
            shopList.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            shopPanel.Add(shopList);
        }

        bool StakeOpen => stakeOverlay != null && stakeOverlay.style.display == DisplayStyle.Flex;
        bool ShopOpen => shopOverlay != null && shopOverlay.style.display == DisplayStyle.Flex;
        void ShowStakes()
        {
            if (spinning || ModalOpen || session.RecoveryRequired || model.Complete) return;
            HideToast(); main.SetEnabled(false); stakeOverlay.style.display = DisplayStyle.Flex; stakeOverlay.BringToFront();
            RefreshV09Controls();
            input.Open(stakeOverlay, stakeChoices[model.SelectedStake - 1], HideStakes, stakeButton);
        }
        void HideStakes()
        {
            stakeOverlay.style.display = DisplayStyle.None; main.SetEnabled(!session.RecoveryRequired); input?.Close(stakeOverlay);
        }
        void SelectStake(int value)
        {
            if (!StakeOpen || spinning) return;
            if (!session.TrySelectStake(value)) { UpdateSaveStatus(); stakeBalance.text = "Could not save this stake. Please retry."; return; }
            UpdateSaveStatus(true); HideStakes(); Refresh();
            ShowToast(value + "x STAKE · " + (5 * value) + " COINS");
        }
        void ShowShop()
        {
            if (spinning || ModalOpen || session.RecoveryRequired) return;
            HideToast(); main.SetEnabled(false); shopOverlay.style.display = DisplayStyle.Flex; shopOverlay.BringToFront();
            RefreshShop();
            shopList.scrollOffset = Vector2.zero;
            shopList.schedule.Execute(() => { if(ShopOpen) shopList.ScrollTo(shopList.Q<VisualElement>("shop-item-" + Math.Max(0,model.ActiveQuest))); }).StartingIn(60);
            input.Open(shopOverlay, shopOverlay.Q<Button>("close-shop"), HideShop, shopButton);
        }
        void HideShop()
        {
            shopOverlay.style.display = DisplayStyle.None; main.SetEnabled(!session.RecoveryRequired); input?.Close(shopOverlay);
        }
        void BuyFromShop(int index)
        {
            if (!ShopOpen || spinning || !model.CanBuy(index)) return;
            int before = model.PurchasedCount;
            Buy(index);
            if (model.PurchasedCount > before) HideShop();
            else RefreshShop();
        }
        void RefreshShop()
        {
            if (!ShopOpen) return;
            var s = model.Snapshot; int active = model.ActiveQuest, nextIndex = model.NextQuest;
            shopSummary.text = "ROOM " + model.PurchasedCount + "/8 · " + s.cash + " coins\n" + (active < 0 ? "All eight upgrades installed" : "CURRENT: " + EnglishUi.QuestTitle(active));
            shopNotice.text = string.IsNullOrEmpty(session.Error) ? "" : "Could not save. Purchase was not applied.";
            shopNotice.style.display = string.IsNullOrEmpty(session.Error) ? DisplayStyle.None : DisplayStyle.Flex;
            var scroll = shopList.scrollOffset;
            shopList.Clear();
            for (int i = 0; i < Economy.SymbolCount; i++)
            {
                int index = i; var d = model.Definition(index); bool owned = s.purchased[index], current = index == active, ready = model.CanBuy(index);
                var card = Frame("shop-item-" + index, PixelUi.Finish.Cream); card.style.flexShrink = 0;
                card.style.paddingLeft = card.style.paddingRight = card.style.paddingTop = card.style.paddingBottom = 8;
                card.style.marginBottom = 8;
                var top = new VisualElement(); Row(top); top.style.marginBottom = 5;
                var image = Icon(reelTextures[index + 1], 48); image.name = "shop-icon-" + index; image.style.marginRight = 8; top.Add(image);
                var info = new VisualElement(); info.style.flexGrow = 1; info.style.flexBasis = 0;
                var title = Text(EnglishUi.QuestTitle(index), UiTypography.Title, Forest); title.name = "shop-title-" + index; info.Add(title);
                var status = Text(owned ? "INSTALLED" : current ? (ready ? "CURRENT · READY TO BUY" : "CURRENT · COLLECTING") : index == nextIndex ? "NEXT · LOCKED" : "LOCKED", UiTypography.Meta, Forest);
                status.name = "shop-state-" + index; info.Add(status); top.Add(info); card.Add(top);
                var requirements = Text(owned ? "Cost: " + d.CashPrice + " coins + " + d.FragmentGoal + " parts · installed" : "Coins " + s.cash + "/" + d.CashPrice + "  ·  Parts " + s.fragments[index] + "/" + d.FragmentGoal, UiTypography.Counter, Forest);
                requirements.name = "shop-requirements-" + index; card.Add(requirements);
                var benefit = Text("Room effect: " + d.Description, UiTypography.Body, Forest); benefit.name = "shop-benefit-" + index;
                UiTypography.Apply(benefit, UiTypography.Body, Forest, false); benefit.style.marginTop = 4; benefit.style.marginBottom = 6; card.Add(benefit);
                if (!owned && !current)
                {
                    var unlock = Text("Unlock after " + EnglishUi.QuestTitle(active) + ".", UiTypography.Meta, Forest); unlock.name = "shop-unlock-" + index; unlock.style.marginBottom = 4; card.Add(unlock);
                }
                string caption = owned ? "INSTALLED" : !current ? "LOCKED" : ready ? "BUY & INSTALL" : s.fragments[index] < d.FragmentGoal && s.cash < d.CashPrice ? "NEED COINS + PARTS" : s.fragments[index] < d.FragmentGoal ? "NEED PARTS" : "NEED COINS";
                var buy = Action(caption, () => BuyFromShop(index), ready ? Rust : Color.gray); buy.name = "shop-buy-" + index;
                buy.SetEnabled(!spinning && current && ready); card.Add(buy); shopList.Add(card);
            }
            shopList.scrollOffset = scroll;
        }
        void RefreshV09Controls()
        {
            if (stakeButton == null) return;
            var state = model.Snapshot;
            int choice = spinning && activeSpin != null ? activeSpin.SelectedStake : model.SelectedStake;
            stakeTitle.text = "STAKE " + choice + "x";
            stakeButton.SetEnabled(!spinning && !model.Complete && !session.RecoveryRequired);
            shopButton.SetEnabled(!spinning && !session.RecoveryRequired);
            int cost = spinning && activeSpin != null ? activeSpin.Cost : model.NextSpinCost;
            betLabel.text = model.Complete || model.CanRecover ? "0" : cost.ToString();
            if (spinning) freeLabel.text = activeSpin != null && activeSpin.Free ? "FREE · 1x" : "REWARD " + choice + "x";
            else if (!model.Complete && !model.CanRecover && !model.CanBuy(model.ActiveQuest))
                freeLabel.text = state.freeSpins > 0 ? "FREE " + state.freeSpins + " · 1x" : state.cash < cost ? "LOW COINS" : "COINS · " + choice + "x";
            if (StakeOpen)
            {
                stakeBalance.text = "Balance: " + state.cash + " coins.\n" + (state.freeSpins > 0 ? state.freeSpins + " free spins remain at 1x." : state.cash < cost ? "Need " + cost + " coins; choose a lower stake." : "Next paid spin: " + cost + " coins, " + choice + "x coin rewards.");
                for (int value = 1; value <= 3; value++) ButtonText(stakeChoices[value - 1], value + "x · " + (5 * value) + " coins" + (value == choice ? " · SELECTED" : ""), 15);
            }
            RefreshShop();
        }
    }
}
