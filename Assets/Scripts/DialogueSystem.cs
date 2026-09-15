namespace HorrorRPG.Dialogue
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;



using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [SerializeField] private bool fastText = false;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Dialogue Settings")]
    [SerializeField] private bool stuckDialogue = false;
    // Advance is provided by GameInputReader.

    [Header("Confirmation Settings")]
    [SerializeField] private GameObject confirmationBackground;
    [SerializeField] private ConfirmationMenu confirmationMenu;
    [SerializeField] private GameInputReader inputReader;


    private const string CONTROL_LOCK_ID = "DialogueSystem";
    private const string CONFIRMATION_LOCK_ID = "DialogueConfirmation";

    private string[] currentSentences;
    private int currentSentenceIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private Coroutine typingCoroutine;
    
    private bool currentFastText;
    private bool currentStuckDialogue;
    private bool currentRequiresConfirmation;
    private bool isConfirmationMenuOpen;

    private Action onConfirmCallback;
    private Action onCancelCallback;

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

    private void OnEnable()
    {
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        if (inputReader == null) return;
        inputReader.AdvancePerformed += HandleAdvance;
        inputReader.NavigatePerformed += HandleNavigation;
        inputReader.SubmitPerformed += HandleSubmit;
        inputReader.CancelPerformed += HandleCancel;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;
        inputReader.AdvancePerformed -= HandleAdvance;
        inputReader.NavigatePerformed -= HandleNavigation;
        inputReader.SubmitPerformed -= HandleSubmit;
        inputReader.CancelPerformed -= HandleCancel;
    }



    private void Start()
    {
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    private void HandleAdvance()
    {
        if (!isDialogueActive || isConfirmationMenuOpen) return;
        if (isTyping)
        {
            StopTyping();
            dialogueText.text = currentSentences[currentSentenceIndex];
            isTyping = false;
        }
        else DisplayNextSentence();
    }

    private void HandleNavigation(Vector2 navigation)
    {
        if (isConfirmationMenuOpen) confirmationMenu?.HandleNavigation(navigation);
    }

    private void HandleSubmit()
    {
        if (isConfirmationMenuOpen) confirmationMenu?.ExecuteCurrentSelection();
    }

    private void HandleCancel()
    {
        if (isConfirmationMenuOpen) onCancelCallback?.Invoke();
        else if (isDialogueActive) EndDialogue();
    }


    public void StartDialogue(DialogueData dialogueData, bool? stuckOverride = null, bool? fastTextOverride = null)
    {
        if (dialogueData == null || dialogueData.sentences.Length == 0)
        {
            return;
        }

        currentFastText = fastTextOverride.HasValue ? fastTextOverride.Value : dialogueData.fastText;
        currentStuckDialogue = stuckOverride.HasValue ? stuckOverride.Value : dialogueData.stuckDialogue;
        currentRequiresConfirmation = dialogueData.requiresConfirmation;

        currentSentences = dialogueData.sentences;
        currentSentenceIndex = 0;
        isDialogueActive = true;

        onConfirmCallback = null;
        onCancelCallback = null;

        if (currentStuckDialogue && PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        DisplaySentence(currentSentences[currentSentenceIndex]);
    }

    public void StartDialogueWithConfirmation(string message, Action onConfirm, Action onCancel, DialogueData dialogueData)
    {
        currentFastText = dialogueData != null ? dialogueData.fastText : fastText;
        currentStuckDialogue = dialogueData != null ? dialogueData.stuckDialogue : stuckDialogue;
        currentRequiresConfirmation = true;

        currentSentences = new string[] { message };
        currentSentenceIndex = 0;
        isDialogueActive = true;

        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;

        if (currentStuckDialogue && PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        DisplaySentence(currentSentences[currentSentenceIndex]);
    }

    private void DisplaySentence(string sentence)
    {
        StopTyping();

        if (currentFastText)
        {
            dialogueText.text = sentence;
            isTyping = false;
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeSentence(sentence));
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void DisplayNextSentence()
    {
        currentSentenceIndex++;

        if (currentSentenceIndex < currentSentences.Length)
        {
            DisplaySentence(currentSentences[currentSentenceIndex]);
        }
        else
        {
            if (currentRequiresConfirmation)
            {
                ShowConfirmationMenu();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        StopTyping();
        isDialogueActive = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (currentStuckDialogue && PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);
        }

        currentSentences = null;
        currentSentenceIndex = 0;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void SetFastText(bool value)
    {
        fastText = value;
    }

    public void SetStuckDialogue(bool value)
    {
        stuckDialogue = value;
    }

    public void SetTypingSpeed(float speed)
    {
        typingSpeed = speed;
    }

    private void ShowConfirmationMenu()
    {
        isConfirmationMenuOpen = true;
        isDialogueActive = false;

        if (PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.LockControl(CONFIRMATION_LOCK_ID);
        }

        if (confirmationBackground != null)
        {
            confirmationBackground.SetActive(true);
        }

        if (confirmationMenu != null)
        {
            confirmationMenu.Show(OnConfirmationAccepted, OnConfirmationCancelled);
        }
    }

    private void OnConfirmationAccepted()
    {
        CloseConfirmationMenu();
        onConfirmCallback?.Invoke();
        EndDialogue();
        ResetDialogueState();
    }

    private void OnConfirmationCancelled()
    {
        CloseConfirmationMenu();
        onCancelCallback?.Invoke();
        EndDialogue();
        ResetDialogueState();
    }

    private void CloseConfirmationMenu()
    {
        isConfirmationMenuOpen = false;

        if (confirmationMenu != null)
        {
            confirmationMenu.Hide();
        }

        if (confirmationBackground != null)
        {
            confirmationBackground.SetActive(false);
        }

        if (PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.UnlockControl(CONFIRMATION_LOCK_ID);
        }
    }

    private void ResetDialogueState()
    {
        currentSentences = null;
        currentSentenceIndex = 0;
        currentRequiresConfirmation = false;
        onConfirmCallback = null;
        onCancelCallback = null;
    }
}


}
