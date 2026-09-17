using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private string stableId;
        [SerializeField, FormerlySerializedAs("sentences"), TextArea(3, 10)] private string[] sentencesValue = Array.Empty<string>();
        [Header("Dialogue Behavior")]
        [SerializeField, FormerlySerializedAs("stuckDialogue")] private bool stuckDialogueValue;
        [SerializeField, FormerlySerializedAs("fastText")] private bool fastTextValue;
        [Header("Confirmation Settings")]
        [SerializeField, FormerlySerializedAs("requiresConfirmation")] private bool requiresConfirmationValue;

        public string Id => stableId;
        public string[] sentences => sentencesValue;
        public bool stuckDialogue => stuckDialogueValue;
        public bool fastText => fastTextValue;
        public bool requiresConfirmation => requiresConfirmationValue;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
            stableId = stableId.Trim();
            sentencesValue ??= Array.Empty<string>();
        }
    }
}
