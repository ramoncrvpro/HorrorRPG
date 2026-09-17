using System.Collections;
using HorrorRPG.Core;
using HorrorRPG.Inventory;
using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Interaction
{
    public class DoorInteraction : MonoBehaviour, IInteractable, IGameContextReceiver
    {
        private const string LockedPrompt = "Porta trancada";
        private const string OpenPrompt = "Pressione E para abrir";

        [Header("Lock Settings")]
        [SerializeField] private bool isLocked;
        [SerializeField] private ItemData requiredKey;
        [SerializeField] private bool consumeRequiredKey;
        [Header("Animation Settings")]
        [SerializeField] private SpriteRendererAnimator doorAnimator;
        [Header("Door Objects")]
        [SerializeField] private GameObject objectToDisable;
        [SerializeField] private float disableDelay = 0.5f;
        [Header("Persistence")]
        [SerializeField] private WorldObjectId worldObjectId;

        private bool isOpening;

        /// <summary>Applies an opened door state restored from the runtime session.</summary>
        public void Initialize(GameContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            if (worldObjectId == null) worldObjectId = GetComponent<WorldObjectId>();
            if (worldObjectId == null || !context.Session.World.IsCompleted(worldObjectId.Value)) return;
            isLocked = false;
            isOpening = true;
            if (objectToDisable != null) objectToDisable.SetActive(false);
        }

        /// <summary>Stops pending door presentation when the scene is released.</summary>
        public void Deinitialize() => StopAllCoroutines();

        /// <summary>Unlocks with the configured key and starts the door sequence once.</summary>
        public void Interact(in InteractionContext context)
        {
            if (!CanInteract(in context)) return;
            if (isLocked)
            {
                if (requiredKey == null || !context.Inventory.HasItem(requiredKey, 1)) return;
                if (consumeRequiredKey && !context.Inventory.RemoveItem(requiredKey, 1).IsComplete) return;
                isLocked = false;
            }
            isOpening = true;
            if (worldObjectId != null) context.Game.Session.World.MarkCompleted(worldObjectId.Value);
            if (doorAnimator != null) doorAnimator.Play();
            if (objectToDisable != null) StartCoroutine(DisableObjectAfterDelay());
        }

        /// <summary>Returns a prompt describing the door's current lock state.</summary>
        public string GetInteractionPrompt(in InteractionContext context)
        {
            if (!isLocked) return OpenPrompt;
            return requiredKey != null ? $"{LockedPrompt} - {requiredKey.itemName} necessária" : LockedPrompt;
        }

        /// <summary>Prevents re-entry after the opening sequence starts.</summary>
        public bool CanInteract(in InteractionContext context) => !isOpening;

        private IEnumerator DisableObjectAfterDelay()
        {
            yield return new WaitForSeconds(disableDelay);
            if (objectToDisable != null) objectToDisable.SetActive(false);
        }

        private void OnValidate()
        {
            disableDelay = Mathf.Max(0f, disableDelay);
        }
    }
}
