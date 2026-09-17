using HorrorRPG.Core;
using HorrorRPG.Input;
using UnityEngine;

namespace HorrorRPG.Dialogue
{
    /// <summary>Scene controller connecting dialogue domain state, input and presentation.</summary>
    public class DialogueSystem : MonoBehaviour, IGameContextReceiver
    {
        private const float MinimumTypingSpeed = 0.001f;

        [SerializeField] private DialogueView dialogueView;
        [SerializeField] private bool fastText;
        [SerializeField] private float typingSpeed = 0.05f;

        private GameContext gameContext;
        private DialogueService dialogueService;
        private GameInputReader inputReader;
        private DialogueHandle activeHandle;
        private InputContextLease inputLease;
        private UINavigationHandle navigationHandle;
        private UINavigationHandle confirmationHandle;
        private bool initialized;
        private bool hasActiveDialogue;

        private void Awake()
        {
            if (dialogueView == null) Debug.LogError($"{nameof(DialogueSystem)} requires a {nameof(DialogueView)} on {name}.", this);
        }

        /// <summary>Injects services and subscribes to dialogue, input and view events.</summary>
        public void Initialize(GameContext context)
        {
            if (initialized) return;
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            dialogueService = context.Dialogue;
            inputReader = context.InputReader;
            dialogueService.Started += HandleStarted;
            dialogueService.LineChanged += HandleLineChanged;
            dialogueService.ConfirmationRequested += HandleConfirmationRequested;
            dialogueService.Ended += HandleEnded;
            inputReader.AdvancePerformed += HandleAdvance;
            inputReader.NavigatePerformed += HandleNavigation;
            inputReader.SubmitPerformed += HandleSubmit;
            if (dialogueView != null)
            {
                dialogueView.TypingCompleted += HandleTypingCompleted;
                dialogueView.Confirmed += HandleConfirmed;
                dialogueView.Cancelled += HandleCancelled;
            }
            initialized = true;
        }

        /// <summary>Releases scene-owned callbacks and presentation state.</summary>
        public void Deinitialize()
        {
            if (!initialized) return;
            dialogueService.Started -= HandleStarted;
            dialogueService.LineChanged -= HandleLineChanged;
            dialogueService.ConfirmationRequested -= HandleConfirmationRequested;
            dialogueService.Ended -= HandleEnded;
            inputReader.AdvancePerformed -= HandleAdvance;
            inputReader.NavigatePerformed -= HandleNavigation;
            inputReader.SubmitPerformed -= HandleSubmit;
            if (dialogueView != null)
            {
                dialogueView.TypingCompleted -= HandleTypingCompleted;
                dialogueView.Confirmed -= HandleConfirmed;
                dialogueView.Cancelled -= HandleCancelled;
                dialogueView.Hide();
            }
            ReleaseNavigationAndInput();
            hasActiveDialogue = false;
            gameContext = null;
            dialogueService = null;
            inputReader = null;
            initialized = false;
        }

        private void HandleStarted(DialogueHandle handle)
        {
            activeHandle = handle;
            hasActiveDialogue = true;
            inputLease = gameContext.Input.Acquire(InputContext.Dialogue, InputBlockReason.Dialogue);
            navigationHandle = gameContext.Navigation.Push(UIScreenId.Dialogue, CancelActiveDialogue);
            dialogueView?.Show();
        }

        private void HandleLineChanged(DialogueHandle handle, string line)
        {
            if (!hasActiveDialogue || !handle.Equals(activeHandle)) return;
            dialogueView?.SetLine(line, Mathf.Max(MinimumTypingSpeed, typingSpeed), fastText || dialogueService.CurrentFastText);
        }

        private void HandleTypingCompleted()
        {
            if (hasActiveDialogue && dialogueService.State == DialogueState.Typing) dialogueService.Advance(activeHandle);
        }

        private void HandleAdvance()
        {
            if (!hasActiveDialogue || dialogueService.State == DialogueState.AwaitingConfirmation) return;
            if (dialogueView != null && dialogueView.IsTyping) dialogueView.CompleteTyping();
            else dialogueService.Advance(activeHandle);
        }

        private void HandleConfirmationRequested(DialogueHandle handle)
        {
            if (!hasActiveDialogue || !handle.Equals(activeHandle)) return;
            dialogueView?.ShowConfirmation();
            confirmationHandle = gameContext.Navigation.Push(UIScreenId.Confirmation, HandleCancelled);
        }

        private void HandleNavigation(Vector2 navigation)
        {
            if (hasActiveDialogue && dialogueService.State == DialogueState.AwaitingConfirmation)
                dialogueView?.NavigateConfirmation(navigation);
        }

        private void HandleSubmit()
        {
            if (hasActiveDialogue && dialogueService.State == DialogueState.AwaitingConfirmation)
                dialogueView?.SubmitConfirmation();
        }

        private void HandleConfirmed()
        {
            if (hasActiveDialogue) dialogueService.Confirm(activeHandle);
        }

        private void HandleCancelled()
        {
            if (hasActiveDialogue) dialogueService.Cancel(activeHandle);
        }

        private void CancelActiveDialogue()
        {
            if (hasActiveDialogue) dialogueService.Cancel(activeHandle);
        }

        private void HandleEnded(DialogueEndedEvent result)
        {
            if (!hasActiveDialogue || !result.Handle.Equals(activeHandle)) return;
            hasActiveDialogue = false;
            dialogueView?.Hide();
            ReleaseNavigationAndInput();
            activeHandle = default;
        }

        private void ReleaseNavigationAndInput()
        {
            if (gameContext != null)
            {
                gameContext.Navigation.Pop(confirmationHandle);
                gameContext.Navigation.Pop(navigationHandle);
            }
            confirmationHandle = default;
            navigationHandle = default;
            inputLease?.Dispose();
            inputLease = null;
        }

        private void OnValidate()
        {
            typingSpeed = Mathf.Max(MinimumTypingSpeed, typingSpeed);
        }

        private void OnDestroy() => Deinitialize();
    }
}
