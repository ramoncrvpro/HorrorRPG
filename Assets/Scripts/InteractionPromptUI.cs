using System.Collections;
using TMPro;
using UnityEngine;

namespace HorrorRPG.Player
{
    /// <summary>Displays the scene-local interaction prompt.</summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject promptBox;
        [SerializeField] private TextMeshProUGUI promptText;
        [Header("Prompt Settings")]
        [SerializeField] private string defaultPromptMessage = "Pressione E para interagir";

        private Coroutine autoHideCoroutine;
        public bool IsPromptActive { get; private set; }
        public bool HasActiveAutoHide => autoHideCoroutine != null;

        private void Start() => HidePrompt(true);

        /// <summary>Shows a prompt until explicitly hidden.</summary>
        public void ShowPrompt(string message = null)
        {
            StopAutoHide();
            if (promptBox == null || promptText == null) return;
            promptText.text = string.IsNullOrEmpty(message) ? defaultPromptMessage : message;
            promptBox.SetActive(true);
            IsPromptActive = true;
        }

        /// <summary>Shows a prompt for a fixed non-negative duration.</summary>
        public void ShowPromptWithDuration(string message, float duration)
        {
            ShowPrompt(message);
            autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(Mathf.Max(0f, duration)));
        }

        /// <summary>Hides the prompt unless a timed prompt still owns it.</summary>
        public void HidePrompt(bool force = false)
        {
            if (!force && autoHideCoroutine != null) return;
            StopAutoHide();
            promptBox?.SetActive(false);
            IsPromptActive = false;
        }

        /// <summary>Returns whether the prompt is visible.</summary>
        public bool GetIsPromptActive() => IsPromptActive;

        private IEnumerator AutoHideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            autoHideCoroutine = null;
            HidePrompt(true);
        }

        private void StopAutoHide()
        {
            if (autoHideCoroutine == null) return;
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }
}
