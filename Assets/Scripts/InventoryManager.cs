using HorrorRPG.Battle;
using HorrorRPG.Core;
using HorrorRPG.Input;
using UnityEngine;

namespace HorrorRPG.Inventory
{
    /// <summary>Scene controller connecting inventory input, domain state and presentation.</summary>
    public class InventoryManager : MonoBehaviour, IGameContextReceiver
    {


        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private WeaponData defaultWeapon;

        private GameContext gameContext;
        private InventoryService inventoryService;
        private GameInputReader inputReader;
        private InputContextLease inputLease;
        private UINavigationHandle navigationHandle;
        private UINavigationHandle confirmationHandle;
        private bool initialized;
        private bool isOpen;



        private void Awake()
        {
            if (inventoryView == null) Debug.LogError($"{nameof(InventoryManager)} requires an {nameof(InventoryView)} on {name}.", this);
        }

        /// <summary>Injects services and begins observing inventory and UI events.</summary>
        public void Initialize(GameContext context)
        {
            if (initialized) return;
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            inventoryService = context.Inventory;
            inputReader = context.InputReader;
            inventoryService.Changed += HandleInventoryChanged;
            inputReader.OpenInventoryPerformed += ToggleInventory;
            inputReader.NavigatePerformed += HandleNavigation;
            inputReader.SubmitPerformed += HandleSubmit;
            if (inventoryView != null)
            {
                inventoryView.CloseRequested += CloseInventory;
                inventoryView.DiscardRequested += HandleDiscardRequested;
                inventoryView.CategoryChanged += HandleCategoryChanged;
                inventoryView.ConfirmationOpened += HandleConfirmationOpened;
                inventoryView.ConfirmationClosed += HandleConfirmationClosed;
            }
            initialized = true;
            if (defaultWeapon != null && !inventoryService.HasItem(defaultWeapon, 1)) inventoryService.AddItem(defaultWeapon, 1);
        }

        /// <summary>Releases all input, navigation and inventory subscriptions.</summary>
        public void Deinitialize()
        {
            if (!initialized) return;
            CloseInventory();
            inventoryService.Changed -= HandleInventoryChanged;
            inputReader.OpenInventoryPerformed -= ToggleInventory;
            inputReader.NavigatePerformed -= HandleNavigation;
            inputReader.SubmitPerformed -= HandleSubmit;
            if (inventoryView != null)
            {
                inventoryView.CloseRequested -= CloseInventory;
                inventoryView.DiscardRequested -= HandleDiscardRequested;
                inventoryView.CategoryChanged -= HandleCategoryChanged;
                inventoryView.ConfirmationOpened -= HandleConfirmationOpened;
                inventoryView.ConfirmationClosed -= HandleConfirmationClosed;
            }
            gameContext = null;
            inventoryService = null;
            inputReader = null;
            initialized = false;
        }

        /// <summary>Opens or closes the inventory outside active battle phases.</summary>
        public void ToggleInventory()
        {
            if (!initialized) return;
            if (isOpen)
            {
                CloseInventory();
                return;
            }
            if (gameContext.Battle.State.Phase != BattlePhase.Idle) return;
            isOpen = true;
            inputLease = gameContext.Input.Acquire(InputContext.UI, InputBlockReason.Menu);
            navigationHandle = gameContext.Navigation.Push(UIScreenId.Inventory, CloseInventory);
            inventoryView?.Open();
            RefreshView();
        }




        private void HandleNavigation(Vector2 navigation)
        {
            if (isOpen) inventoryView?.Navigate(navigation);
        }

        private void HandleSubmit()
        {
            if (isOpen) inventoryView?.Submit();
        }

        private void HandleInventoryChanged(InventoryChangedEvent change)
        {
            if (isOpen) RefreshView();
        }

        private void HandleCategoryChanged(ItemCategory category) => RefreshView();

        private void HandleDiscardRequested(InventoryEntry entry)
        {
            inventoryService.RemoveItem(entry.Item, entry.Quantity);
            RefreshView();
        }

        private void HandleConfirmationOpened()
        {
            confirmationHandle = gameContext.Navigation.Push(UIScreenId.Confirmation, inventoryView.Cancel);
        }

        private void HandleConfirmationClosed()
        {
            gameContext.Navigation.Pop(confirmationHandle);
            confirmationHandle = default;
        }

        private void RefreshView()
        {
            if (inventoryView != null) inventoryView.Render(inventoryService.GetItems(inventoryView.CurrentCategory));
        }

        private void CloseInventory()
        {
            if (!isOpen) return;
            isOpen = false;
            inventoryView?.Close();
            gameContext?.Navigation.Pop(confirmationHandle);
            gameContext?.Navigation.Pop(navigationHandle);
            confirmationHandle = default;
            navigationHandle = default;
            inputLease?.Dispose();
            inputLease = null;
        }

        private void OnDestroy() => Deinitialize();
    }
}
