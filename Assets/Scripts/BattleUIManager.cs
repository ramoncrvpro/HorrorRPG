using System;
using System.Collections.Generic;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Inventory;
using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Presents battle state and forwards player menu intent without applying rules.</summary>
    public class BattleUIManager : MonoBehaviour, IGameContextReceiver
    {
        private static readonly WeaponCategory[] WeaponCategories = { WeaponCategory.Used, WeaponCategory.Basic, WeaponCategory.Limited };
        private static readonly string[] WeaponCategoryLabels = { "USED", "BASIC", "LIMITED" };

        [Header("UI References")]
        [SerializeField] private GameObject battleMainMenuBackground;
        [SerializeField] private GameObject attackMenuBackground;
        [SerializeField] private GameObject itensMenuBackground;
        [SerializeField] private GameObject emptyMessage;
        [SerializeField] private GameObject emptyMessageItems;
        [Header("Main Menu Buttons")]
        [SerializeField] private SelectableButton attackButton;
        [SerializeField] private SelectableButton itemsButton;
        [SerializeField] private SelectableButton runButton;
        [Header("Attack Menu")]
        [SerializeField] private WeaponTab[] weaponTabs;
        [SerializeField] private WeaponSlotUI[] weaponSlots;
        [Header("Items Menu")]
        [SerializeField] private ItemSlotUI[] itemSlots;
        [Header("Health Bars")]
        [SerializeField] private HealthBar playerHealthBar;
        [SerializeField] private HealthBar enemyHealthBar;

        private readonly List<SelectableButton> mainMenuButtons = new List<SelectableButton>();
        private readonly List<WeaponData> visibleWeapons = new List<WeaponData>();
        private readonly List<InventoryEntry> visibleItems = new List<InventoryEntry>();
        private GameContext gameContext;
        private BattleService battleService;
        private InventoryService inventoryService;
        private GameInputReader inputReader;
        private UINavigationHandle submenuHandle;
        private int mainMenuIndex;
        private int weaponTabIndex;
        private int weaponIndex = -1;
        private int itemIndex = -1;
        private bool mainMenuOpen;
        private bool attackMenuOpen;
        private bool itemsMenuOpen;
        private bool initialized;

        public event Action<WeaponData> WeaponSelected;
        public event Action<ConsumableData> ConsumableSelected;
        public event Action RunRequested;

        private void Awake()
        {
            ConfigureMainMenu();
            ConfigureTabs();
            CloseBattleUI();
        }

        /// <summary>Injects battle, inventory, navigation and input services.</summary>
        public void Initialize(GameContext context)
        {
            if (initialized) return;
            gameContext = context ?? throw new ArgumentNullException(nameof(context));
            battleService = context.Battle;
            inventoryService = context.Inventory;
            inputReader = context.InputReader;
            battleService.PhaseChanged += HandlePhaseChanged;
            battleService.HealthChanged += HandleHealthChanged;
            battleService.CommandRejected += HandleCommandRejected;
            inventoryService.Changed += HandleInventoryChanged;
            inputReader.BattleNavigatePerformed += HandleBattleNavigation;
            inputReader.BattleSubmitPerformed += HandleBattleSubmit;
            inputReader.BattleCancelPerformed += HandleBattleCancel;
            initialized = true;
        }

        /// <summary>Releases battle presentation callbacks and closes all menus.</summary>
        public void Deinitialize()
        {
            if (!initialized) return;
            battleService.PhaseChanged -= HandlePhaseChanged;
            battleService.HealthChanged -= HandleHealthChanged;
            battleService.CommandRejected -= HandleCommandRejected;
            inventoryService.Changed -= HandleInventoryChanged;
            inputReader.BattleNavigatePerformed -= HandleBattleNavigation;
            inputReader.BattleSubmitPerformed -= HandleBattleSubmit;
            inputReader.BattleCancelPerformed -= HandleBattleCancel;
            CloseBattleUI();
            gameContext = null;
            battleService = null;
            inventoryService = null;
            inputReader = null;
            initialized = false;
        }

        /// <summary>Initializes health presentation for the active battle.</summary>
        public void InitializeBattle()
        {
            if (battleService?.State.Enemy == null) return;
            playerHealthBar?.gameObject.SetActive(true);
            enemyHealthBar?.gameObject.SetActive(true);
            enemyHealthBar?.Initialize(battleService.State.Enemy.enemyName, battleService.State.CurrentEnemyHealth, battleService.State.Enemy.maxHealth);
            playerHealthBar?.InitializeAsPlayerHealthBar(gameContext.Session.Player.CurrentHealth, gameContext.Session.Player.MaxHealth);
        }

        /// <summary>Refreshes both health bars from authoritative runtime state.</summary>
        public void UpdateHealthBars()
        {
            if (battleService?.State.Enemy != null)
                enemyHealthBar?.SetHealth(battleService.State.CurrentEnemyHealth, battleService.State.Enemy.maxHealth);
            if (gameContext != null)
                playerHealthBar?.SetHealth(gameContext.Session.Player.CurrentHealth, gameContext.Session.Player.MaxHealth);
        }

        /// <summary>Closes all battle menus while preserving health bars.</summary>
        public void HideAllMenus()
        {
            CloseSubmenu(false);
            if (battleMainMenuBackground != null) battleMainMenuBackground.SetActive(false);
            mainMenuOpen = false;
        }

        /// <summary>Opens the main battle choices.</summary>
        public void OpenMainMenu()
        {
            CloseSubmenu(false);
            mainMenuOpen = true;
            if (battleMainMenuBackground != null) battleMainMenuBackground.SetActive(true);
            SelectMainMenu(0);
        }

        /// <summary>Closes battle menus and health presentation.</summary>
        public void CloseBattleUI()
        {
            HideAllMenus();
            playerHealthBar?.gameObject.SetActive(false);
            enemyHealthBar?.gameObject.SetActive(false);
        }

        private void ConfigureMainMenu()
        {
            mainMenuButtons.Clear();
            if (attackButton != null) { attackButton.SetText("Attack"); mainMenuButtons.Add(attackButton); }
            if (itemsButton != null) { itemsButton.SetText("Items"); mainMenuButtons.Add(itemsButton); }
            if (runButton != null) { runButton.SetText("Run"); mainMenuButtons.Add(runButton); }
        }

        private void ConfigureTabs()
        {
            if (weaponTabs == null) return;
            for (int index = 0; index < weaponTabs.Length && index < WeaponCategories.Length; index++)
                weaponTabs[index]?.Initialize(WeaponCategories[index], WeaponCategoryLabels[index]);
        }

        private void HandlePhaseChanged(BattlePhase phase)
        {
            switch (phase)
            {
                case BattlePhase.Entering:
                    InitializeBattle();
                    HideAllMenus();
                    break;
                case BattlePhase.PlayerChoice:
                    UpdateHealthBars();
                    OpenMainMenu();
                    break;
                case BattlePhase.Idle:
                case BattlePhase.Won:
                case BattlePhase.Lost:
                case BattlePhase.Exiting:
                    CloseBattleUI();
                    break;
                default:
                    HideAllMenus();
                    break;
            }
        }

        private void HandleHealthChanged(BattleHealthChangedEvent change)
        {
            if (change.Target == BattleHealthTarget.Player) playerHealthBar?.SetHealth(change.Current, change.Maximum);
            else enemyHealthBar?.SetHealth(change.Current, change.Maximum);
        }

        private void HandleInventoryChanged(InventoryChangedEvent change)
        {
            if (attackMenuOpen) RefreshWeapons();
            if (itemsMenuOpen) RefreshItems();
        }

        private void HandleCommandRejected(BattleCommandResult result)
        {
            if (!result.Accepted) Debug.LogWarning($"Battle command rejected: {result.Reason}", this);
        }

        private void HandleBattleNavigation(Vector2 navigation)
        {
            if (navigation.sqrMagnitude < 0.25f) return;
            if (mainMenuOpen && !attackMenuOpen && !itemsMenuOpen) SelectMainMenuWrapped(navigation.y < 0f ? 1 : -1);
            else if (attackMenuOpen)
            {
                if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y)) ChangeWeaponTab(navigation.x > 0f ? 1 : -1);
                else SelectWeaponWrapped(navigation.y < 0f ? 1 : -1);
            }
            else if (itemsMenuOpen) SelectItemWrapped(navigation.y < 0f ? 1 : -1);
        }

        private void HandleBattleSubmit()
        {
            if (mainMenuOpen && !attackMenuOpen && !itemsMenuOpen) ExecuteMainMenu();
            else if (attackMenuOpen && weaponIndex >= 0 && weaponIndex < visibleWeapons.Count)
            {
                WeaponData weapon = visibleWeapons[weaponIndex];
                CloseSubmenu(true);
                WeaponSelected?.Invoke(weapon);
            }
            else if (itemsMenuOpen && itemIndex >= 0 && itemIndex < visibleItems.Count && visibleItems[itemIndex].Item is ConsumableData consumable)
            {
                CloseSubmenu(true);
                ConsumableSelected?.Invoke(consumable);
            }
        }

        private void HandleBattleCancel()
        {
            if (attackMenuOpen || itemsMenuOpen)
            {
                CloseSubmenu(true);
                OpenMainMenu();
            }
        }

        private void ExecuteMainMenu()
        {
            if (mainMenuIndex == 0) OpenAttackMenu();
            else if (mainMenuIndex == 1) OpenItemsMenu();
            else if (mainMenuIndex == 2) RunRequested?.Invoke();
        }

        private void OpenAttackMenu()
        {
            mainMenuOpen = false;
            battleMainMenuBackground?.SetActive(false);
            attackMenuOpen = true;
            attackMenuBackground?.SetActive(true);
            weaponTabIndex = 0;
            submenuHandle = gameContext.Navigation.Push(UIScreenId.Battle, HandleBattleCancel);
            RefreshWeapons();
        }

        private void OpenItemsMenu()
        {
            mainMenuOpen = false;
            battleMainMenuBackground?.SetActive(false);
            itemsMenuOpen = true;
            itensMenuBackground?.SetActive(true);
            submenuHandle = gameContext.Navigation.Push(UIScreenId.Battle, HandleBattleCancel);
            RefreshItems();
        }

        private void CloseSubmenu(bool popNavigation)
        {
            attackMenuOpen = false;
            itemsMenuOpen = false;
            attackMenuBackground?.SetActive(false);
            itensMenuBackground?.SetActive(false);
            SelectWeapon(-1);
            SelectItem(-1);
            if (popNavigation && gameContext != null) gameContext.Navigation.Pop(submenuHandle);
            submenuHandle = default;
        }

        private void RefreshWeapons()
        {
            visibleWeapons.Clear();
            var allWeapons = new List<WeaponData>();
            foreach (InventoryEntry entry in inventoryService.GetItems())
                if (entry.Item is WeaponData weapon) allWeapons.Add(weapon);

            WeaponCategory category = WeaponCategories[Mathf.Clamp(weaponTabIndex, 0, WeaponCategories.Length - 1)];
            if (category == WeaponCategory.Used)
            {
                foreach (string id in gameContext.Session.RecentWeapons.WeaponIds)
                {
                    WeaponData weapon = allWeapons.Find(candidate => GetStableItemId(candidate) == id);
                    if (weapon != null) visibleWeapons.Add(weapon);
                }
            }
            else
            {
                foreach (WeaponData weapon in allWeapons)
                {
                    if ((category == WeaponCategory.Basic && !weapon.requiresAmmo) || (category == WeaponCategory.Limited && weapon.requiresAmmo))
                        visibleWeapons.Add(weapon);
                }
            }

            int slotCount = weaponSlots?.Length ?? 0;
            for (int index = 0; index < slotCount; index++)
            {
                if (index < visibleWeapons.Count)
                {
                    WeaponData weapon = visibleWeapons[index];
                    int ammo = weapon.requiresAmmo && weapon.ammoType != null ? inventoryService.GetQuantity(weapon.ammoType) : -1;
                    weaponSlots[index]?.Setup(weapon, ammo);
                }
                else weaponSlots[index]?.Setup(null);
            }
            emptyMessage?.SetActive(visibleWeapons.Count == 0);
            UpdateWeaponTabs();
            SelectWeapon(visibleWeapons.Count > 0 ? 0 : -1);
        }

        private void RefreshItems()
        {
            visibleItems.Clear();
            foreach (InventoryEntry entry in inventoryService.GetItems(ItemCategory.Consumable))
                if (entry.Item is ConsumableData) visibleItems.Add(entry);
            int slotCount = itemSlots?.Length ?? 0;
            for (int index = 0; index < slotCount; index++)
            {
                if (index < visibleItems.Count) itemSlots[index]?.Setup(visibleItems[index]);
                else itemSlots[index]?.Clear();
            }
            emptyMessageItems?.SetActive(visibleItems.Count == 0);
            SelectItem(visibleItems.Count > 0 ? 0 : -1);
        }

        private void ChangeWeaponTab(int direction)
        {
            weaponTabIndex = (weaponTabIndex + direction + WeaponCategories.Length) % WeaponCategories.Length;
            RefreshWeapons();
        }

        private void UpdateWeaponTabs()
        {
            if (weaponTabs == null) return;
            for (int index = 0; index < weaponTabs.Length; index++) weaponTabs[index]?.SetSelected(index == weaponTabIndex);
        }

        private void SelectMainMenuWrapped(int direction)
        {
            if (mainMenuButtons.Count == 0) return;
            SelectMainMenu((mainMenuIndex + direction + mainMenuButtons.Count) % mainMenuButtons.Count);
        }

        private void SelectMainMenu(int index)
        {
            if (mainMenuButtons.Count == 0) return;
            for (int buttonIndex = 0; buttonIndex < mainMenuButtons.Count; buttonIndex++) mainMenuButtons[buttonIndex].SetSelected(buttonIndex == index);
            mainMenuIndex = index;
        }

        private void SelectWeaponWrapped(int direction)
        {
            if (visibleWeapons.Count == 0) return;
            SelectWeapon((weaponIndex + direction + visibleWeapons.Count) % visibleWeapons.Count);
        }

        private void SelectWeapon(int index)
        {
            if (weaponSlots != null)
                for (int slotIndex = 0; slotIndex < weaponSlots.Length; slotIndex++) weaponSlots[slotIndex]?.SetSelected(slotIndex == index);
            weaponIndex = index;
        }

        private void SelectItemWrapped(int direction)
        {
            if (visibleItems.Count == 0) return;
            SelectItem((itemIndex + direction + visibleItems.Count) % visibleItems.Count);
        }

        private void SelectItem(int index)
        {
            if (itemSlots != null)
                for (int slotIndex = 0; slotIndex < itemSlots.Length; slotIndex++) itemSlots[slotIndex]?.SetSelected(slotIndex == index);
            itemIndex = index;
        }

        private static string GetStableItemId(ItemData item) => item.Id;
        private void OnDestroy() => Deinitialize();
    }
}
