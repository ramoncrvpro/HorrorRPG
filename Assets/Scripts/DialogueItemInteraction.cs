namespace HorrorRPG.Dialogue
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Interaction;



using System.Collections;
using UnityEngine;

public class DialogueItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Item Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction Settings")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private string customPrompt = "";
    [SerializeField] private bool oneTimeOnly = false;

    private bool hasInteracted = false;
    private bool isWaitingForDialogue = false;

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (DialogueSystem.Instance != null && dialogueData != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueData);
            isWaitingForDialogue = true;
            StartCoroutine(WaitForDialogueEnd());

            if (oneTimeOnly)
            {
                hasInteracted = true;
            }
        }
    }

    private IEnumerator WaitForDialogueEnd()
    {
        yield return null;

        while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            yield return null;
        }

        isWaitingForDialogue = false;

        if (canBePickedUp && itemData != null)
        {
            ShowItemConfirmation();
        }
    }

    private void ShowItemConfirmation()
    {
        string confirmationMessage = $"Você quer adicionar {quantity}x {itemData.itemName} ao seu inventário?";
        
        DialogueSystem.Instance.StartDialogueWithConfirmation(
            confirmationMessage,
            OnItemConfirmed,
            OnItemCancelled,
            dialogueData
        );
    }

    private void OnItemConfirmed()
    {
        CollectItems();
    }

    private void OnItemCancelled()
    {
        if (oneTimeOnly)
        {
            hasInteracted = true;
        }
    }

    private void CollectItems()
    {
        if (!canBePickedUp || itemData == null)
            return;

        StartCoroutine(CollectItemsCoroutine());
    }

    private IEnumerator CollectItemsCoroutine()
    {
        if (InventoryManager.Instance != null)
        {
            yield return InventoryManager.Instance.AddItemCoroutine(itemData, quantity);

            int addedQuantity = InventoryManager.Instance.GetLastAddedQuantity();

            if (addedQuantity > 0)
            {
                quantity -= addedQuantity;

                if (quantity <= 0)
                {
                    canBePickedUp = false;
                }
            }
        }
    }

    public string GetInteractionPrompt()
    {
        if (!string.IsNullOrEmpty(customPrompt))
        {
            return customPrompt;
        }

        if (itemData != null)
        {
            return "Pressione E para interagir";
        }

        return "Pressione E para interagir";
    }

    public bool CanInteract()
    {
        if (!canBePickedUp)
        {
            return false;
        }

        if (oneTimeOnly && hasInteracted)
        {
            return false;
        }

        if (isWaitingForDialogue)
        {
            return false;
        }

        return dialogueData != null && itemData != null;
    }

    public void SetCanInteract(bool value)
    {
        canBePickedUp = value;
    }

    public void ResetInteraction()
    {
        hasInteracted = false;
    }
}


}
