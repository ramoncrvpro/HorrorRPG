using System;

namespace HorrorRPG.Dialogue
{
    public enum DialogueState { Inactive, Typing, AwaitingAdvance, AwaitingConfirmation, Completed }
    public enum DialogueEndReason { Completed, Cancelled, Aborted, SceneChanged }

    public readonly struct DialogueHandle
    {
        internal DialogueHandle(Guid id) { Id = id; }
        internal Guid Id { get; }
    }

    public readonly struct DialogueRequest
    {
        public DialogueRequest(string[] lines, bool requiresConfirmation = false, bool fastText = false)
        {
            Lines = lines;
            RequiresConfirmation = requiresConfirmation;
            FastText = fastText;
        }

        public string[] Lines { get; }
        public bool RequiresConfirmation { get; }
        public bool FastText { get; }
    }

    /// <summary>Immutable result emitted when a dialogue ends.</summary>
    public readonly struct DialogueEndedEvent
    {
        public DialogueHandle Handle { get; }
        public DialogueEndReason Reason { get; }

        public DialogueEndedEvent(DialogueHandle handle, DialogueEndReason reason)
        {
            Handle = handle;
            Reason = reason;
        }
    }

    /// <summary>Single authority for dialogue progression, confirmation and completion.</summary>
    public sealed class DialogueService
    {
        private Guid activeId;
        private string[] lines;
        private int lineIndex;
        private bool requiresConfirmation;

        public DialogueState State { get; private set; } = DialogueState.Inactive;
        public bool CurrentFastText { get; private set; }
        public event Action<DialogueHandle> Started;
        public event Action<DialogueHandle, string> LineChanged;
        public event Action<DialogueHandle> ConfirmationRequested;
        public event Action<DialogueEndedEvent> Ended;

        public DialogueHandle Start(DialogueRequest request)
        {
            if (lines != null) throw new InvalidOperationException("A dialogue is already active.");
            if (request.Lines == null || request.Lines.Length == 0) throw new ArgumentException("Dialogue requires at least one line.", nameof(request));

            var copiedLines = new string[request.Lines.Length];
            for (int index = 0; index < request.Lines.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(request.Lines[index])) throw new ArgumentException("Dialogue lines cannot be empty.", nameof(request));
                copiedLines[index] = request.Lines[index];
            }

            activeId = Guid.NewGuid();
            lines = copiedLines;
            lineIndex = 0;
            requiresConfirmation = request.RequiresConfirmation;
            CurrentFastText = request.FastText;
            State = DialogueState.Typing;

            var handle = new DialogueHandle(activeId);
            Started?.Invoke(handle);
            LineChanged?.Invoke(handle, lines[0]);
            return handle;
        }

        public void Advance(DialogueHandle handle)
        {
            Validate(handle);
            if (State == DialogueState.Typing)
            {
                State = DialogueState.AwaitingAdvance;
                return;
            }

            if (State != DialogueState.AwaitingAdvance) return;
            lineIndex++;
            if (lineIndex < lines.Length)
            {
                State = DialogueState.Typing;
                LineChanged?.Invoke(handle, lines[lineIndex]);
                return;
            }

            if (requiresConfirmation)
            {
                State = DialogueState.AwaitingConfirmation;
                ConfirmationRequested?.Invoke(handle);
            }
            else
            {
                End(handle, DialogueEndReason.Completed);
            }
        }

        public void Confirm(DialogueHandle handle)
        {
            Validate(handle);
            if (State != DialogueState.AwaitingConfirmation) return;
            End(handle, DialogueEndReason.Completed);
        }

        public void Cancel(DialogueHandle handle)
        {
            Validate(handle);
            End(handle, DialogueEndReason.Cancelled);
        }

        public void Abort(DialogueHandle handle, DialogueEndReason reason)
        {
            Validate(handle);
            if (reason == DialogueEndReason.Completed || reason == DialogueEndReason.Cancelled) reason = DialogueEndReason.Aborted;
            End(handle, reason);
        }

        /// <summary>Aborts the active dialogue when its scene is being unloaded.</summary>
        public void AbortActive(DialogueEndReason reason)
        {
            if (lines == null) return;
            if (reason == DialogueEndReason.Completed || reason == DialogueEndReason.Cancelled) reason = DialogueEndReason.SceneChanged;
            End(new DialogueHandle(activeId), reason);
        }

        private void Validate(DialogueHandle handle)
        {
            if (lines == null || handle.Id != activeId) throw new InvalidOperationException("Dialogue handle is no longer active.");
        }

        private void End(DialogueHandle handle, DialogueEndReason reason)
        {
            Validate(handle);
            State = reason == DialogueEndReason.Completed ? DialogueState.Completed : DialogueState.Inactive;
            lines = null;
            lineIndex = 0;
            requiresConfirmation = false;
            CurrentFastText = false;
            activeId = Guid.Empty;
            Ended?.Invoke(new DialogueEndedEvent(handle, reason));
        }
    }
}
