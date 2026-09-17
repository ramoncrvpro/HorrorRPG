using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Interaction;
using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Player
{
    public class PlayerInteraction : MonoBehaviour, IGameContextReceiver
    {
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private InteractionPromptUI promptUI;
        [SerializeField] private HandAnimationManager handAnimationManager;

        private GameContext gameContext;
        private GameInputReader inputReader;
        private IInteractable currentInteractable;
        private string currentPrompt;
        private bool interactionEnabled = true;
        private bool inputSubscribed;

        private void Awake()
        {
            if (handAnimationManager == null) handAnimationManager = FindFirstObjectByType<HandAnimationManager>();
            if (raycastOrigin == null && playerCamera != null) raycastOrigin = playerCamera.transform;
            if (raycastOrigin == null)
            {
                Debug.LogError($"{nameof(PlayerInteraction)} requires a raycast origin or player camera on {name}.", this);
                enabled = false;
            }
            if (promptUI == null) Debug.LogError($"{nameof(PlayerInteraction)} requires an interaction prompt view on {name}.", this);
        }

        private void OnEnable() => SubscribeInput();

        private void OnDisable()
        {
            UnsubscribeInput();
            HideInteractionPrompt();
        }

        private void Update()
        {
            if (interactionEnabled && gameContext != null) CheckForInteractable();
            else SetCurrentInteractable(null);
        }

        /// <summary>Injects runtime services and subscribes to interaction input.</summary>
        public void Initialize(GameContext context)
        {
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            inputReader = context.InputReader;
            SubscribeInput();
        }

        /// <summary>Releases input subscriptions and the active target.</summary>
        public void Deinitialize()
        {
            UnsubscribeInput();
            gameContext = null;
            inputReader = null;
            SetCurrentInteractable(null);
        }

        /// <summary>Returns the target currently visible to the player.</summary>
        public IInteractable GetCurrentInteractable() => currentInteractable;

        /// <summary>Returns the prompt currently visible to the player.</summary>
        public string GetInteractionPrompt() => currentPrompt;

        /// <summary>Enables or disables interaction checks.</summary>
        public void SetInteractionEnabled(bool isEnabled)
        {
            interactionEnabled = isEnabled;
            if (!isEnabled) SetCurrentInteractable(null);
        }

        private void CheckForInteractable()
        {
            InteractionContext context = CreateInteractionContext();
            IInteractable found = null;
            var ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
            }
            if (found != null && !found.CanInteract(in context)) found = null;
            SetCurrentInteractable(found);
        }

        private void HandleInteraction()
        {
            if (!interactionEnabled || currentInteractable == null || gameContext == null) return;
            InteractionContext context = CreateInteractionContext();
            if (currentInteractable.CanInteract(in context))
            {
                handAnimationManager?.PlayReachAnimation();
                currentInteractable.Interact(in context);
            }
        }

        private InteractionContext CreateInteractionContext() => new InteractionContext(gameContext, gameObject);

        private void SetCurrentInteractable(IInteractable interactable)
        {
            string prompt = null;
            if (interactable != null && gameContext != null)
            {
                InteractionContext context = CreateInteractionContext();
                prompt = interactable.GetInteractionPrompt(in context);
            }
            if (ReferenceEquals(currentInteractable, interactable) && currentPrompt == prompt) return;
            currentInteractable = interactable;
            currentPrompt = prompt;
            if (currentInteractable == null) HideInteractionPrompt();
            else promptUI?.ShowPrompt(currentPrompt);
        }

        private void HideInteractionPrompt()
        {
            promptUI?.HidePrompt();
            currentPrompt = null;
        }

        private void SubscribeInput()
        {
            if (!isActiveAndEnabled || inputReader == null || inputSubscribed) return;
            inputReader.InteractPerformed += HandleInteraction;
            inputSubscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!inputSubscribed || inputReader == null) return;
            inputReader.InteractPerformed -= HandleInteraction;
            inputSubscribed = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (raycastOrigin != null) Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
        }
    }
}
