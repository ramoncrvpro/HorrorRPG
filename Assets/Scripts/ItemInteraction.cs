namespace HorrorRPG.Interaction
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;



using System.Collections;
using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Item Configuration")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction Settings")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private string customPrompt = "";

    public void Interact()
    {
        if (!CanInteract())
            return;

        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        if (PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.LockControl("ItemPickup");
        }
        
        if (InventoryManager.Instance != null && itemData != null)
        {
            yield return InventoryManager.Instance.AddItemCoroutine(itemData, quantity);

            int addedQuantity = InventoryManager.Instance.GetLastAddedQuantity();

            if (addedQuantity > 0)
            {
                quantity -= addedQuantity;

                if (quantity <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        if (PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.UnlockControl("ItemPickup");
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
            if (quantity > 1)
            {
                return $"Press E to pick up {itemData.itemName} x{quantity}";
            }
            return $"Press E to pick up {itemData.itemName}";
        }

        return "Press E to pick up item";
    }

    public bool CanInteract()
    {
        return canBePickedUp && itemData != null && InventoryManager.Instance != null;
    }
}


}
