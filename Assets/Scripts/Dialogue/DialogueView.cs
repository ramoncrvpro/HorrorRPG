using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace HorrorRPG.Dialogue
{
    /// <summary>Displays dialogue text, typing and confirmation without owning dialogue state.</summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject confirmationBackground;
        [SerializeField] private ConfirmationMenu confirmationMenu;

        private Coroutine typingCoroutine;
        private string completeLine = string.Empty;
        private bool typing;

        public event Action TypingCompleted;
        public event Action Confirmed;
        public event Action Cancelled;

        public bool IsTyping => typing;

        private void Awake()
        {
            if (confirmationMenu != null)
            {
                confirmationMenu.Confirmed += HandleConfirmed;
                confirmationMenu.Cancelled += HandleCancelled;
            }
            Hide();
        }

        private void OnDestroy()
        {
            if (confirmationMenu == null) return;
            confirmationMenu.Confirmed -= HandleConfirmed;
            confirmationMenu.Cancelled -= HandleCancelled;
        }

        /// <summary>Shows the dialogue box.</summary>
        public void Show()
        {
            if (dialogueBox != null) dialogueBox.SetActive(true);
        }

        /// <summary>Hides dialogue and confirmation while cancelling visual typing.</summary>
        public void Hide()
        {
            StopTyping();
            HideConfirmation();
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (dialogueText != null) dialogueText.text = string.Empty;
        }

        /// <summary>Displays a line immediately or with a typing animation.</summary>
        public void SetLine(string line, float typingSpeed, bool immediate)
        {
            StopTyping();
            completeLine = line ?? string.Empty;
            Show();
            if (dialogueText == null) return;
            if (immediate || typingSpeed <= 0f)
            {
                dialogueText.text = completeLine;
                TypingCompleted?.Invoke();
                return;
            }
            typingCoroutine = StartCoroutine(TypeLine(typingSpeed));
        }

        /// <summary>Completes the current typing animation exactly once.</summary>
        public void CompleteTyping()
        {
            if (!typing) return;
            StopTyping();
            if (dialogueText != null) dialogueText.text = completeLine;
            TypingCompleted?.Invoke();
        }

        /// <summary>Shows the shared confirmation view.</summary>
        public void ShowConfirmation()
        {
            if (confirmationBackground != null) confirmationBackground.SetActive(true);
            confirmationMenu?.Show();
        }

        /// <summary>Hides the shared confirmation view.</summary>
        public void HideConfirmation()
        {
            confirmationMenu?.Hide();
            if (confirmationBackground != null) confirmationBackground.SetActive(false);
        }

        /// <summary>Moves the current confirmation selection.</summary>
        public void NavigateConfirmation(Vector2 navigation) => confirmationMenu?.HandleNavigation(navigation);

        /// <summary>Executes the current confirmation selection.</summary>
        public void SubmitConfirmation() => confirmationMenu?.ExecuteCurrentSelection();

        private IEnumerator TypeLine(float typingSpeed)
        {
            typing = true;
            dialogueText.text = string.Empty;
            foreach (char character in completeLine)
            {
                dialogueText.text += character;
                yield return new WaitForSeconds(typingSpeed);
            }
            typing = false;
            typingCoroutine = null;
            TypingCompleted?.Invoke();
        }

        private void StopTyping()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            typing = false;
        }

        private void HandleConfirmed() => Confirmed?.Invoke();
        private void HandleCancelled() => Cancelled?.Invoke();
    }
}
