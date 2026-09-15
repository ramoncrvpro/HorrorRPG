namespace HorrorRPG.Interaction
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;


public class SceneLoadInteraction : MonoBehaviour, IInteractable
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string customPrompt = "Press E to enter";
    
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteract = true;

    public void Interact()
    {
        if (!CanInteract())
            return;

        LoadScene();
    }

    public string GetInteractionPrompt()
    {
        return customPrompt;
    }

    public bool CanInteract()
    {
        return canInteract && !string.IsNullOrEmpty(sceneToLoad);
    }

    private void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("SceneLoadInteraction: Nome da cena não definido!");
            return;
        }

        if (GameBootstrap.Instance == null || GameBootstrap.Instance.Context == null)
        {
            Debug.LogError($"SceneFlow is unavailable for destination '{sceneToLoad}'.", this);
            return;
        }
        GameBootstrap.Instance.Context.SceneFlow.Load(sceneToLoad);
    }
}


}
