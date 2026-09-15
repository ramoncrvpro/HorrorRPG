namespace HorrorRPG.Player
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Settings")]
    [SerializeField] private string defaultPromptMessage = "Pressione E para interagir";

    private bool isPromptActive = false;
    private Coroutine autoHideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (promptBox != null)
        {
            promptBox.SetActive(false);
        }
    }

    public void ShowPrompt(string message = null)
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (promptBox != null && promptText != null)
        {
            promptText.text = string.IsNullOrEmpty(message) ? defaultPromptMessage : message;
            promptBox.SetActive(true);
            isPromptActive = true;
        }
    }

    public void ShowPromptWithDuration(string message, float duration)
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }

        ShowPrompt(message);
        autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(duration));
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt(true);
        autoHideCoroutine = null;
    }

    public void HidePrompt(bool force = false)
    {
        if (!force && autoHideCoroutine != null)
        {
            return;
        }

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (promptBox != null)
        {
            promptBox.SetActive(false);
            isPromptActive = false;
        }
    }

    public bool IsPromptActive()
    {
        return isPromptActive;
    }

    public bool HasActiveAutoHide()
    {
        return autoHideCoroutine != null;
    }
}


}
