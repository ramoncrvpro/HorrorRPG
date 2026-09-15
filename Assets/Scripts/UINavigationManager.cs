namespace HorrorRPG.Presentation
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using System;
using System.Collections.Generic;
using UnityEngine;

public class UINavigationManager : MonoBehaviour
{
    public static UINavigationManager Instance { get; private set; }

    [SerializeField] private GameInputReader inputReader;
    private readonly Stack<UIState> navigationStack = new Stack<UIState>();



    private void Awake() { if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>(); }
    private void OnEnable() { if (inputReader != null) inputReader.CancelPerformed += HandleBackNavigation; }
    private void OnDisable() { if (inputReader != null) inputReader.CancelPerformed -= HandleBackNavigation; }

    public void PushState(UIState state)
    {
        if (state == null)
        {
            Debug.LogWarning("Tentativa de adicionar UIState nulo à pilha de navegação");
            return;
        }

        navigationStack.Push(state);
    }

    public void PopState()
    {
        if (navigationStack.Count > 0)
        {
            navigationStack.Pop();
        }
    }

    public void ClearStack()
    {
        navigationStack.Clear();
    }

    private void HandleBackNavigation()
    {
        if (navigationStack.Count > 0)
        {
            UIState currentState = navigationStack.Peek();
            currentState?.onBackPressed?.Invoke();
        }
    }

    public int GetStackCount()
    {
        return navigationStack.Count;
    }
}

public class UIState
{
    public string stateName;
    public Action onBackPressed;

    public UIState(string name, Action backCallback)
    {
        stateName = name;
        onBackPressed = backCallback;
    }
}


}
