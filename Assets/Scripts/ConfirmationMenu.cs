namespace HorrorRPG.Dialogue
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using System;
using UnityEngine;

public class ConfirmationMenu : MonoBehaviour
{
    [SerializeField] private SelectableButton confirmButton;
    [SerializeField] private SelectableButton cancelButton;

    private SelectableButton currentlySelectedButton;
    private int currentSelectedIndex = 0;
    private SelectableButton[] buttons;
    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        buttons = new SelectableButton[] { cancelButton, confirmButton };
        //Hide();
    }

    public void Show(Action confirmCallback, Action cancelCallback)
    {
        onConfirm = confirmCallback;
        onCancel = cancelCallback;
        
        currentSelectedIndex = 0;
        SelectButton(currentSelectedIndex);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.SetSelected(false);
            currentlySelectedButton = null;
        }
    }

    public void HandleNavigation(Vector2 navigation)
    {
        if (buttons == null || buttons.Length == 0 || navigation.sqrMagnitude < 0.25f) return;
        if (Mathf.Abs(navigation.y) <= Mathf.Abs(navigation.x)) return;
        if (navigation.y < 0f) currentSelectedIndex = (currentSelectedIndex + 1) % buttons.Length;
        else
        {
            currentSelectedIndex--;
            if (currentSelectedIndex < 0) currentSelectedIndex = buttons.Length - 1;
        }
        SelectButton(currentSelectedIndex);
    }

    private void SelectButton(int index)
    {
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.SetSelected(false);
        }
        if (buttons == null ||  buttons.Length <= 0)
        {
            buttons = new SelectableButton[] { cancelButton, confirmButton };
        }
        currentlySelectedButton = buttons[index];
        currentlySelectedButton.SetSelected(true);
    }

    public void ExecuteCurrentSelection() => ExecuteCurrentButton();

    private void ExecuteCurrentButton()
    {
        if (currentlySelectedButton == confirmButton) onConfirm?.Invoke();
        else if (currentlySelectedButton == cancelButton) onCancel?.Invoke();
    }
}

}
