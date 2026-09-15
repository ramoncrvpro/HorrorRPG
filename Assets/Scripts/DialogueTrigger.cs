namespace HorrorRPG.Dialogue
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Interaction;



using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Trigger Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool oneTimeOnly = false;

    private bool hasInteracted = false;

    public void Interact()
    {
        if (!canInteract || (oneTimeOnly && hasInteracted))
        {
            return;
        }

        if (DialogueSystem.Instance != null && dialogueData != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueData);
            
            if (oneTimeOnly)
            {
                hasInteracted = true;
            }
        }
    }

    public string GetInteractionPrompt()
    {
        return "Pressione E para interagir";
    }

    public bool CanInteract()
    {
        if (!canInteract)
        {
            return false;
        }

        if (oneTimeOnly && hasInteracted)
        {
            return false;
        }

        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            return false;
        }

        return true;
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}


}
