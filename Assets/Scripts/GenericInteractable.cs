namespace HorrorRPG.Interaction
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Press E to interact";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private UnityEvent onInteract;

    public void Interact()
    {
        if (canInteract)
        {
            onInteract?.Invoke();
        }
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}


}
