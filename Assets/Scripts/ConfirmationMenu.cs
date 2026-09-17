using System;
using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Dialogue
{
    /// <summary>Reusable confirmation view driven by an external controller.</summary>
    public class ConfirmationMenu : MonoBehaviour
    {
        [SerializeField] private SelectableButton confirmButton;
        [SerializeField] private SelectableButton cancelButton;

        private SelectableButton[] buttons;
        private SelectableButton currentlySelectedButton;
        private int selectedIndex;

        public event Action Confirmed;
        public event Action Cancelled;

        private void Awake()
        {
            buttons = new[] { cancelButton, confirmButton };
        }

        /// <summary>Shows the menu with cancel selected by default.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            selectedIndex = 0;
            SelectButton(selectedIndex);
        }

        /// <summary>Hides the menu and clears its selection.</summary>
        public void Hide()
        {
            if (currentlySelectedButton != null) currentlySelectedButton.SetSelected(false);
            currentlySelectedButton = null;
            gameObject.SetActive(false);
        }

        /// <summary>Moves selection vertically when a navigation threshold is crossed.</summary>
        public void HandleNavigation(Vector2 navigation)
        {
            if (buttons == null || buttons.Length == 0 || navigation.sqrMagnitude < 0.25f) return;
            if (Mathf.Abs(navigation.y) <= Mathf.Abs(navigation.x)) return;
            selectedIndex = navigation.y < 0f
                ? (selectedIndex + 1) % buttons.Length
                : (selectedIndex - 1 + buttons.Length) % buttons.Length;
            SelectButton(selectedIndex);
        }

        /// <summary>Invokes the event represented by the active selection.</summary>
        public void ExecuteCurrentSelection()
        {
            if (currentlySelectedButton == confirmButton) Confirmed?.Invoke();
            else if (currentlySelectedButton == cancelButton) Cancelled?.Invoke();
        }

        private void SelectButton(int index)
        {
            currentlySelectedButton?.SetSelected(false);
            if (buttons == null || index < 0 || index >= buttons.Length || buttons[index] == null) return;
            currentlySelectedButton = buttons[index];
            currentlySelectedButton.SetSelected(true);
        }
    }
}
