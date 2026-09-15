namespace HorrorRPG.Player
{
using HorrorRPG.Input;
using HorrorRPG.Interaction;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameInputReader inputReader;
    [SerializeField] private InteractionPromptUI promptUI;
    private IInteractable currentInteractable;
    private string currentPrompt;
    private bool interactionEnabled = true;

    private void Awake()
    {
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        if (promptUI == null) promptUI = FindFirstObjectByType<InteractionPromptUI>();
        if (raycastOrigin == null) { Debug.LogError($"{nameof(PlayerInteraction)} requires a raycast origin on {name}.", this); enabled = false; }
    }

    private void OnEnable() { if (inputReader != null) inputReader.InteractPerformed += HandleInteraction; }
    private void OnDisable() { if (inputReader != null) inputReader.InteractPerformed -= HandleInteraction; HideInteractionPrompt(); }
    private void Update() { if (interactionEnabled) CheckForInteractable(); else SetCurrentInteractable(null); }

    private void CheckForInteractable()
    {
        if (raycastOrigin == null) { SetCurrentInteractable(null); return; }
        IInteractable found = null;
        if (Physics.Raycast(new Ray(raycastOrigin.position, raycastOrigin.forward), out RaycastHit hit, interactionRange, interactableLayer)) found = hit.collider.GetComponentInParent<IInteractable>();
        if (found != null && !found.CanInteract()) found = null;
        SetCurrentInteractable(found);
    }

    private void HandleInteraction() { if (interactionEnabled) currentInteractable?.Interact(); }
    private void SetCurrentInteractable(IInteractable interactable)
    {
        string prompt = interactable?.GetInteractionPrompt();
        if (ReferenceEquals(currentInteractable, interactable) && currentPrompt == prompt) return;
        currentInteractable = interactable;
        currentPrompt = prompt;
        if (currentInteractable == null) HideInteractionPrompt(); else promptUI?.ShowPrompt(currentPrompt);
    }
    private void HideInteractionPrompt() { promptUI?.HidePrompt(); currentPrompt = null; }
    public IInteractable GetCurrentInteractable() => currentInteractable;
    public string GetInteractionPrompt() => currentPrompt;
    public void SetInteractionEnabled(bool enabled) { interactionEnabled = enabled; if (!enabled) SetCurrentInteractable(null); }
    private void OnDrawGizmosSelected() { if (raycastOrigin != null) Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange); }
}
}
