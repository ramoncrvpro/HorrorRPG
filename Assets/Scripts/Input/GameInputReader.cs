using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorRPG.Input
{
    /// <summary>Translates Input System actions into typed gameplay events.</summary>
    public sealed class GameInputReader : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        private readonly List<(InputAction action, Action<InputAction.CallbackContext> handler)> subscriptions = new List<(InputAction, Action<InputAction.CallbackContext>)>();
        private InputAction moveAction;
        private InputAction lookAction;
        private InputContext? pendingContext;
        public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 Look => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public event Action InteractPerformed;
        public event Action OpenInventoryPerformed;
        public event Action<Vector2> NavigatePerformed;
        public event Action SubmitPerformed;
        public event Action CancelPerformed;
        public event Action AdvancePerformed;
        public event Action<Vector2> BattleNavigatePerformed;
        public event Action BattleSubmitPerformed;
        public event Action BattleCancelPerformed;
        public event Action TimingConfirmPerformed;
        public event Action DefendLeftPerformed;
        public event Action DefendUpPerformed;
        public event Action DefendRightPerformed;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        }
        private void OnEnable()
        {
            if (playerInput == null || playerInput.actions == null) { Debug.LogError($"{nameof(GameInputReader)} requires a configured PlayerInput on {name}.", this); enabled = false; return; }
            moveAction = playerInput.actions.FindAction("Gameplay/Move", true);
            lookAction = playerInput.actions.FindAction("Gameplay/Look", true);
            Bind("Gameplay/Interact", _ => InteractPerformed?.Invoke());
            Bind("Gameplay/OpenInventory", _ => OpenInventoryPerformed?.Invoke());
            Bind("UI/Navigate", context => NavigatePerformed?.Invoke(context.ReadValue<Vector2>()));
            Bind("UI/Submit", _ => SubmitPerformed?.Invoke());
            Bind("UI/Cancel", _ => CancelPerformed?.Invoke());
            Bind("Dialogue/Advance", _ => AdvancePerformed?.Invoke());
            Bind("Battle/Navigate", context => BattleNavigatePerformed?.Invoke(context.ReadValue<Vector2>()));
            Bind("Battle/Submit", _ => BattleSubmitPerformed?.Invoke());
            Bind("Battle/Cancel", _ => BattleCancelPerformed?.Invoke());
            Bind("Battle/TimingConfirm", _ =>
            {
                TimingConfirmPerformed?.Invoke();
                BattleSubmitPerformed?.Invoke();
            });
            Bind("Battle/DefendLeft", _ => DefendLeftPerformed?.Invoke());
            Bind("Battle/DefendUp", _ => DefendUpPerformed?.Invoke());
            Bind("Battle/DefendRight", _ => DefendRightPerformed?.Invoke());

            SetContext(InputContext.Gameplay);
        }

        private void LateUpdate()
        {
            EnforceCursorState();
            if (!pendingContext.HasValue || playerInput == null) return;

            InputContext context = pendingContext.Value;
            pendingContext = null;
            playerInput.SwitchCurrentActionMap(context.ToString());
            EnforceCursorState();
        }

        private static void EnforceCursorState()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) EnforceCursorState();
        }

        private void OnDisable()
        {
            foreach ((InputAction action, Action<InputAction.CallbackContext> handler) subscription in subscriptions) subscription.action.performed -= subscription.handler;
            subscriptions.Clear();
            if (playerInput != null && playerInput.actions != null) playerInput.actions.Disable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            moveAction = null;
            lookAction = null;
            pendingContext = null;
        }
        public void SetContext(InputContext context)
        {
            pendingContext = context;
        }
        private void Bind(string actionName, Action<InputAction.CallbackContext> handler)
        {
            InputAction action = playerInput.actions.FindAction(actionName, true);
            action.performed += handler;
            subscriptions.Add((action, handler));
        }
    }
}
