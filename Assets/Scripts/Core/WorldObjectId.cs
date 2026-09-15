using UnityEngine;

namespace HorrorRPG.Core
{
    /// <summary>Stable identifier for scene objects whose state survives scene changes in the current session.</summary>
    public sealed class WorldObjectId : MonoBehaviour
    {
        [SerializeField] private string value;
        public string Value => value;
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(value)) Debug.LogError($"{nameof(WorldObjectId)} on '{name}' requires a non-empty ID.", this);
        }
    }
}
