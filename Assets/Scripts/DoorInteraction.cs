namespace HorrorRPG.Interaction
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private ItemData requiredKey;

    [Header("Animation Settings")]
    [SerializeField] private SpriteSheetAnimator doorAnimator;
    [SerializeField] private int openStartFrame = 0;
    [SerializeField] private int openEndFrame = 15;

    [Header("Door Objects")]
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private float disableDelay = 0.5f;

    private bool isOpening = false;

    public void Interact()
    {
        if (isOpening) return;

        if (isLocked)
        {
            if (InventoryManager.Instance != null && requiredKey != null)
            {
                if (InventoryManager.Instance.HasItem(requiredKey, 1))
                {
                    UnlockDoor();
                    OpenDoor();
                }
            }
        }
        else
        {
            OpenDoor();
        }
    }

    public string GetInteractionPrompt()
    {
        if (isLocked)
        {
            if (requiredKey != null)
            {
                return $"Porta trancada - {requiredKey.itemName} necessária";
            }
            return "Porta trancada";
        }
        return "Pressione E para abrir";
    }

    public bool CanInteract()
    {
        return !isOpening;
    }

    private void UnlockDoor()
    {
        isLocked = false;
    }

    private void OpenDoor()
    {
        isOpening = true;

        if (doorAnimator != null)
        {
            doorAnimator.PlayAnimation(openStartFrame, openEndFrame, false);
        }

        if (objectToDisable != null)
        {
            StartCoroutine(DisableObjectAfterDelay(disableDelay));
        }
    }

    private IEnumerator DisableObjectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}


}
