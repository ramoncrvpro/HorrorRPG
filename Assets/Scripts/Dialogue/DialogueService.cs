using System;

namespace HorrorRPG.Dialogue
{
    public enum DialogueState { Inactive, Typing, AwaitingAdvance, AwaitingConfirmation, Completed }
    public enum DialogueEndReason { Completed, Cancelled, Aborted, SceneChanged }
    public readonly struct DialogueHandle { internal DialogueHandle(Guid id) { Id = id; } internal Guid Id { get; } }
    public readonly struct DialogueRequest { public DialogueRequest(string[] lines) { Lines = lines; } public string[] Lines { get; } }
    /// <summary>Single authority for dialogue progression and completion.</summary>
    public sealed class DialogueService
    {
        private Guid activeId;
        private string[] lines;
        private int lineIndex;
        public DialogueState State { get; private set; }
        public event Action<DialogueHandle> Started;
        public event Action<DialogueHandle, string> LineChanged;
        public event Action<DialogueHandle> Ended;
        public DialogueHandle Start(DialogueRequest request)
        {
            if (lines != null) throw new InvalidOperationException("A dialogue is already active.");
            if (request.Lines == null || request.Lines.Length == 0) throw new ArgumentException("Dialogue requires at least one line.");
            foreach (string line in request.Lines) if (string.IsNullOrWhiteSpace(line)) throw new ArgumentException("Dialogue lines cannot be empty.");
            activeId = Guid.NewGuid(); lines = request.Lines; lineIndex = 0; State = DialogueState.Typing;
            var handle = new DialogueHandle(activeId); Started?.Invoke(handle); LineChanged?.Invoke(handle, lines[0]); return handle;
        }
        public void Advance(DialogueHandle handle) { Validate(handle); if (State == DialogueState.Typing) State = DialogueState.AwaitingAdvance; else if (State == DialogueState.AwaitingAdvance) { lineIndex++; if (lineIndex >= lines.Length) End(handle); else { State = DialogueState.Typing; LineChanged?.Invoke(handle, lines[lineIndex]); } } }
        public void Confirm(DialogueHandle handle) => Advance(handle);
        public void Cancel(DialogueHandle handle) => End(handle);
        public void Abort(DialogueHandle handle, DialogueEndReason reason) => End(handle);
        private void Validate(DialogueHandle handle) { if (lines == null || handle.Id != activeId) throw new InvalidOperationException("Dialogue handle is no longer active."); }
        private void End(DialogueHandle handle) { Validate(handle); State = DialogueState.Completed; lines = null; Ended?.Invoke(handle); activeId = Guid.Empty; }
    }
}
