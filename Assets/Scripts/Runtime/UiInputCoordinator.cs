using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    // One input boundary per document. Buttons retain their native Clickable action.
    public sealed class UiInputCoordinator : IDisposable
    {
        static readonly ConditionalWeakTable<VisualElement, UiInputCoordinator> Owners = new ConditionalWeakTable<VisualElement, UiInputCoordinator>();
        readonly VisualElement root;
        readonly HashSet<KeyCode> heldKeys = new HashSet<KeyCode>();
        VisualElement modal, restoreTarget;
        Action cancel;
        bool submitConsumed, disposed;
        Button pressedButton;

        public UiInputCoordinator(VisualElement root)
        {
            this.root = root;
            Owners.Add(root, this);
            root.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            root.RegisterCallback<KeyUpEvent>(KeyUp, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(Submit, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
        }

        public static bool IsShown(VisualElement element)
        {
            if (element == null || element.panel == null) return false;
            for (var item = element; item != null; item = item.parent)
                if (item.style.display == DisplayStyle.None || item.resolvedStyle.display == DisplayStyle.None || item.resolvedStyle.visibility == Visibility.Hidden) return false;
            return true;
        }
        public static bool CanActivate(Button button) => button != null && button.enabledInHierarchy && IsShown(button);
        public static void NoteAction(Button button)
        {
            for (VisualElement item = button; item != null; item = item.parent)
                if (Owners.TryGetValue(item, out var owner)) { if (owner.heldKeys.Count != 0) owner.submitConsumed = true; break; }
        }

        public void Open(VisualElement window, Button first, Action cancelAction, VisualElement restore)
        {
            modal = window; cancel = cancelAction; restoreTarget = restore;
            ReleasePressed();
            window.schedule.Execute(() => { if (!disposed && modal == window && CanActivate(first)) first.Focus(); });
        }
        public void Close(VisualElement window, bool restore = true)
        {
            if (modal != window) return;
            var target = restoreTarget;
            modal = restoreTarget = null; cancel = null;
            ReleasePressed();
            if (restore && target != null)
                root.schedule.Execute(() => { if (!disposed && modal == null && target.enabledInHierarchy && IsShown(target)) target.Focus(); });
        }
        bool InScope(VisualElement target) => modal == null || target == modal || modal.Contains(target);
        static Button ButtonFor(VisualElement target)
        {
            for (var item = target; item != null; item = item.parent) if (item is Button button) return button;
            return null;
        }
        static bool ActivationKey(KeyCode key) => key == KeyCode.Return || key == KeyCode.KeypadEnter || key == KeyCode.Space;
        void KeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape && modal != null)
            {
                evt.StopImmediatePropagation();
                cancel?.Invoke();
                return;
            }
            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopImmediatePropagation();
                var candidates = (modal ?? root).Query<Button>().ToList();
                candidates.RemoveAll(b => !b.focusable || b.tabIndex < 0 || !CanActivate(b));
                if (candidates.Count == 0) return;
                var focused = root.panel.focusController.focusedElement as VisualElement;
                int index = candidates.IndexOf(ButtonFor(focused));
                index = index < 0 ? (evt.shiftKey ? candidates.Count - 1 : 0) : (index + (evt.shiftKey ? -1 : 1) + candidates.Count) % candidates.Count;
                candidates[index].Focus();
                return;
            }
            if (!ActivationKey(evt.keyCode)) return;
            if (!heldKeys.Add(evt.keyCode)) { evt.StopImmediatePropagation(); return; }
            if (heldKeys.Count == 1) submitConsumed = false;
            pressedButton = ButtonFor(root.panel.focusController.focusedElement as VisualElement);
            if (CanActivate(pressedButton) && InScope(pressedButton)) PixelUi.SetPressed(pressedButton, true);
        }
        void KeyUp(KeyUpEvent evt)
        {
            if (!ActivationKey(evt.keyCode)) return;
            heldKeys.Remove(evt.keyCode);
            if (heldKeys.Count == 0) { submitConsumed = false; ReleasePressed(); }
        }
        void Submit(NavigationSubmitEvent evt)
        {
            var button = ButtonFor(evt.target as VisualElement);
            if (!CanActivate(button) || !InScope(button) || (heldKeys.Count != 0 && submitConsumed)) evt.StopImmediatePropagation();
        }
        void PointerDown(PointerDownEvent evt)
        {
            if (!InScope(evt.target as VisualElement)) evt.StopImmediatePropagation();
        }
        void ReleasePressed() { if (pressedButton != null) PixelUi.SetPressed(pressedButton, false); pressedButton = null; }
        public void ReleaseKeys() { heldKeys.Clear(); submitConsumed = false; ReleasePressed(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; ReleaseKeys(); Owners.Remove(root);
            root.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
            root.UnregisterCallback<KeyUpEvent>(KeyUp, TrickleDown.TrickleDown);
            root.UnregisterCallback<NavigationSubmitEvent>(Submit, TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
        }
    }
}
