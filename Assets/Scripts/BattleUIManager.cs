namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

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
    [SerializeField] private Transform weaponTabsParent;
    [SerializeField] private Transform weaponSlotsParent;
    [SerializeField] private GameObject weaponTabPrefab;
    [SerializeField] private GameObject weaponSlotPrefab;

    [Header("Items Menu")]
    [SerializeField] private Transform itemSlotsParent;

    [Header("Health Bars")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar enemyHealthBar;


    [Header("Settings")]
    [SerializeField] private int maxWeaponSlots = 9;

    private List<SelectableButton> mainMenuButtons = new List<SelectableButton>();
    private int currentMainMenuIndex = 0;
    private bool isMainMenuOpen = false;

    private List<WeaponTab> weaponTabs = new List<WeaponTab>();
    private int currentTabIndex = 0;
    private WeaponCategory currentCategory = WeaponCategory.Used;

    private List<WeaponSlotUI> weaponSlotUIList = new List<WeaponSlotUI>();
    private WeaponSlotUI currentlySelectedWeaponSlot;
    private int currentSelectedWeaponIndex = -1;

    private bool isAttackMenuOpen = false;

    private UIState attackMenuState;

    private List<ItemSlotUI> itemSlotUIList = new List<ItemSlotUI>();
    private ItemSlotUI currentlySelectedItemSlot;
    private int currentSelectedItemIndex = -1;
    private bool isItemsMenuOpen = false;
    private UIState itemsMenuState;
    private GameInputReader inputReader;

    private void OnEnable()
    {
        inputReader = FindFirstObjectByType<GameInputReader>();
        if (inputReader == null)
        {
            return;
        }

        inputReader.BattleNavigatePerformed += HandleBattleNavigation;
        inputReader.BattleSubmitPerformed += HandleBattleSubmit;
        inputReader.BattleCancelPerformed += HandleBattleCancel;
    }

    private void OnDisable()
    {
        if (inputReader == null)
        {
            return;
        }

        inputReader.BattleNavigatePerformed -= HandleBattleNavigation;
        inputReader.BattleSubmitPerformed -= HandleBattleSubmit;
        inputReader.BattleCancelPerformed -= HandleBattleCancel;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (battleMainMenuBackground != null)
        {
            battleMainMenuBackground.SetActive(false);
        }

        if (attackMenuBackground != null)
        {
            attackMenuBackground.SetActive(false);
        }

        if (itensMenuBackground != null)
        {
            itensMenuBackground.SetActive(false);
        }

        InitializeMainMenuButtons();
        InitializeWeaponTabs();
        FindOrCreateWeaponSlots();
        FindOrCreateItemSlots();
        DisableHealthBars();
    }

    private void InitializeMainMenuButtons()
    {
        if (attackButton != null)
        {
            attackButton.SetText("Attack");
            mainMenuButtons.Add(attackButton);
        }

        if (itemsButton != null)
        {
            itemsButton.SetText("Items");
            mainMenuButtons.Add(itemsButton);
        }

        if (runButton != null)
        {
            runButton.SetText("Run");
            mainMenuButtons.Add(runButton);
        }
    }

    private void InitializeWeaponTabs()
    {
        if (weaponTabsParent == null || weaponTabPrefab == null)
        {
            return;
        }

        WeaponCategory[] categories = { WeaponCategory.Used, WeaponCategory.Basic, WeaponCategory.Limited };
        string[] tabNames = { "USED", "BASIC", "LIMITED" };

        for (int i = 0; i < categories.Length; i++)
        {
            GameObject tabObject = Instantiate(weaponTabPrefab, weaponTabsParent);
            WeaponTab tab = tabObject.GetComponent<WeaponTab>();
            
            if (tab == null)
            {
                tab = tabObject.AddComponent<WeaponTab>();
            }

            tab.Initialize(categories[i], tabNames[i]);
            weaponTabs.Add(tab);
        }

        currentTabIndex = 0;
        currentCategory = WeaponCategory.Used;
        UpdateWeaponTabsVisuals();
    }

    private void FindOrCreateWeaponSlots()
    {
        if (weaponSlotsParent != null)
        {
            WeaponSlotUI[] existingSlots = weaponSlotsParent.GetComponentsInChildren<WeaponSlotUI>(true);
            
            foreach (WeaponSlotUI slot in existingSlots)
            {
                weaponSlotUIList.Add(slot);
                slot.gameObject.SetActive(false);
            }

            int slotsToCreate = maxWeaponSlots - weaponSlotUIList.Count;
            for (int i = 0; i < slotsToCreate; i++)
            {
                if (weaponSlotPrefab != null)
                {
                    GameObject newSlot = Instantiate(weaponSlotPrefab, weaponSlotsParent);
                    WeaponSlotUI slotUI = newSlot.GetComponent<WeaponSlotUI>();
                    if (slotUI != null)
                    {
                        weaponSlotUIList.Add(slotUI);
                        slotUI.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void HandleBattleNavigation(Vector2 navigation)
    {
        if (navigation.sqrMagnitude < 0.25f)
        {
            return;
        }

        if (isMainMenuOpen && !isAttackMenuOpen && !isItemsMenuOpen)
        {
            HandleMainMenuNavigation(navigation.y < 0f);
        }
        else if (isAttackMenuOpen)
        {
            if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y))
            {
                HandleWeaponTabNavigation(navigation.x > 0f);
            }
            else
            {
                HandleWeaponListNavigation(navigation.y < 0f);
            }
        }
        else if (isItemsMenuOpen)
        {
            HandleItemsListNavigation(navigation.y < 0f);
        }
    }

    private void HandleBattleSubmit()
    {
        if (isMainMenuOpen && !isAttackMenuOpen && !isItemsMenuOpen)
        {
            ExecuteMainMenuAction();
        }
        else if (isAttackMenuOpen)
        {
            OnWeaponSelected();
        }
        else if (isItemsMenuOpen)
        {
            OnItemSelected();
        }
    }

    private void HandleBattleCancel()
    {
        if (isAttackMenuOpen)
        {
            OnAttackMenuBack();
        }
        else if (isItemsMenuOpen)
        {
            OnItemsMenuBack();
        }
    }

    public void InitializeBattle()
    {
        EnableHealthBars();
        UpdateHealthBars();
    }

    public void UpdateHealthBars()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.CurrentEnemyData != null)
        {
            EnemyData enemyData = BattleManager.Instance.CurrentEnemyData;
            int enemyCurrentHealth = BattleManager.Instance.CurrentEnemyHealth;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.SetName(enemyData.enemyName);
                enemyHealthBar.SetHealth(enemyCurrentHealth, enemyData.maxHealth);
            }
        }

        if (PlayerStats.Instance != null)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.SetName("Player");
                playerHealthBar.SetHealth(PlayerStats.Instance.CurrentHealth, PlayerStats.Instance.MaxHealth);
            }
        }
    }

    private void EnableHealthBars()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.gameObject.SetActive(true);
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.gameObject.SetActive(true);
        }
    }

    private void DisableHealthBars()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.gameObject.SetActive(false);
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.gameObject.SetActive(false);
        }
    }

    public void HideAllMenus()
    {
        if (battleMainMenuBackground != null)
        {
            battleMainMenuBackground.SetActive(false);
        }

        if (attackMenuBackground != null)
        {
            attackMenuBackground.SetActive(false);
        }

        isMainMenuOpen = false;
        isAttackMenuOpen = false;
    }

    public void OpenMainMenu()
    {
        if (battleMainMenuBackground != null)
        {
            battleMainMenuBackground.SetActive(true);
            isMainMenuOpen = true;
            SelectMainMenuButton(0);
        }
    }

    public void CloseMainMenu()
    {
        if (battleMainMenuBackground != null)
        {
            battleMainMenuBackground.SetActive(false);
            isMainMenuOpen = false;
            
            if (currentMainMenuIndex >= 0 && currentMainMenuIndex < mainMenuButtons.Count)
            {
                mainMenuButtons[currentMainMenuIndex].SetSelected(false);
            }
        }
    }

    public void CloseBattleUI()
    {
        DisableHealthBars();
        CloseMainMenu();
    }

    private void HandleMainMenuNavigation(bool moveDown)
    {
        if (moveDown)
        {
            int nextIndex = currentMainMenuIndex + 1;
            if (nextIndex >= mainMenuButtons.Count)
            {
                nextIndex = 0;
            }
            SelectMainMenuButton(nextIndex);
        }
        else
        {
            int previousIndex = currentMainMenuIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = mainMenuButtons.Count - 1;
            }
            SelectMainMenuButton(previousIndex);
        }
    }

    private void SelectMainMenuButton(int index)
    {
        if (index < 0 || index >= mainMenuButtons.Count)
            return;

        if (currentMainMenuIndex >= 0 && currentMainMenuIndex < mainMenuButtons.Count)
        {
            mainMenuButtons[currentMainMenuIndex].SetSelected(false);
        }

        currentMainMenuIndex = index;
        mainMenuButtons[currentMainMenuIndex].SetSelected(true);
    }

    private void ExecuteMainMenuAction()
    {
        if (currentMainMenuIndex == 0)
        {
            OnAttackButtonClicked();
        }
        else if (currentMainMenuIndex == 1)
        {
            OnItemsButtonClicked();
        }
        else if (currentMainMenuIndex == 2)
        {
            OnRunButtonClicked();
        }
    }

    private void OnAttackButtonClicked()
    {
        CloseMainMenu();
        OpenAttackMenu();
    }

    private void OnItemsButtonClicked()
    {
        CloseMainMenu();
        OpenItemsMenu();
    }

    private void OnRunButtonClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EndBattle();
        }
        CloseMainMenu();
    }

    private void OpenAttackMenu()
    {
        if (attackMenuBackground != null)
        {
            attackMenuBackground.SetActive(true);
            isAttackMenuOpen = true;
            currentTabIndex = 0;
            currentCategory = WeaponCategory.Used;
            UpdateWeaponTabsVisuals();
            RefreshWeaponList();

            attackMenuState = new UIState("AttackMenu", OnAttackMenuBack);
            if (UINavigationManager.Instance != null)
            {
                UINavigationManager.Instance.PushState(attackMenuState);
            }
        }
    }

    private void OnAttackMenuBack()
    {
        CloseAttackMenu();
        OpenMainMenu();
    }

    private void CloseAttackMenu()
    {
        if (attackMenuBackground != null)
        {
            attackMenuBackground.SetActive(false);
            isAttackMenuOpen = false;
            
            if (currentlySelectedWeaponSlot != null)
            {
                currentlySelectedWeaponSlot.SetSelected(false);
                currentlySelectedWeaponSlot = null;
            }
        }

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.PopState();
        }
    }

    private void HandleWeaponTabNavigation(bool moveRight)
    {
        if (moveRight)
        {
            currentTabIndex++;
            if (currentTabIndex >= weaponTabs.Count)
            {
                currentTabIndex = 0;
            }
            
            currentCategory = weaponTabs[currentTabIndex].GetCategory();
            UpdateWeaponTabsVisuals();
            RefreshWeaponList();
        }
        else
        {
            currentTabIndex--;
            if (currentTabIndex < 0)
            {
                currentTabIndex = weaponTabs.Count - 1;
            }
            
            currentCategory = weaponTabs[currentTabIndex].GetCategory();
            UpdateWeaponTabsVisuals();
            RefreshWeaponList();
        }
    }

    private void UpdateWeaponTabsVisuals()
    {
        for (int i = 0; i < weaponTabs.Count; i++)
        {
            weaponTabs[i].SetSelected(i == currentTabIndex);
        }
    }

    private void RefreshWeaponList()
    {
        List<WeaponData> filteredWeapons = GetFilteredWeapons();

        for (int i = 0; i < weaponSlotUIList.Count; i++)
        {
            if (i < filteredWeapons.Count)
            {
                int ammoCount = -1;
                
                if (currentCategory == WeaponCategory.Limited && filteredWeapons[i].requiresAmmo)
                {
                    if (InventoryManager.Instance != null && filteredWeapons[i].ammoType != null)
                    {
                        ammoCount = GetAmmoCount(filteredWeapons[i].ammoType);
                    }
                }
                
                weaponSlotUIList[i].Setup(filteredWeapons[i], this, ammoCount);
            }
            else
            {
                weaponSlotUIList[i].Setup(null, this, -1);
            }
        }

        if (currentlySelectedWeaponSlot != null)
        {
            currentlySelectedWeaponSlot.SetSelected(false);
            currentlySelectedWeaponSlot = null;
        }

        if (filteredWeapons.Count == 0)
        {
            if (emptyMessage != null)
            {
                emptyMessage.SetActive(true);
            }
        }
        else
        {
            if (emptyMessage != null)
            {
                emptyMessage.SetActive(false);
            }

            SelectWeaponSlotByIndex(0);
        }
    }

    private List<WeaponData> GetFilteredWeapons()
    {
        List<WeaponData> filtered = new List<WeaponData>();

        if (InventoryManager.Instance == null)
            return filtered;

        List<WeaponData> allWeapons = GetAllWeaponsFromInventory();

        switch (currentCategory)
        {
            case WeaponCategory.Used:
                if (RecentWeaponsManager.Instance != null)
                {
                    filtered = RecentWeaponsManager.Instance.GetRecentWeapons();
                    filtered.RemoveAll(w => !allWeapons.Contains(w));
                }
                break;

            case WeaponCategory.Basic:
                foreach (WeaponData weapon in allWeapons)
                {
                    if (!weapon.requiresAmmo)
                    {
                        filtered.Add(weapon);
                    }
                }
                break;

            case WeaponCategory.Limited:
                foreach (WeaponData weapon in allWeapons)
                {
                    if (weapon.requiresAmmo)
                    {
                        filtered.Add(weapon);
                    }
                }
                break;
        }

        return filtered;
    }

    private List<WeaponData> GetAllWeaponsFromInventory()
    {
        List<WeaponData> weapons = new List<WeaponData>();
        
        if (InventoryManager.Instance == null)
            return weapons;

        List<ItemData> allItems = InventoryManager.Instance.GetAllItems();
        
        foreach (ItemData item in allItems)
        {
            WeaponData weapon = item as WeaponData;
            if (weapon != null && !weapons.Contains(weapon))
            {
                weapons.Add(weapon);
            }
        }

        return weapons;
    }

    private int GetAmmoCount(ItemData ammoType)
    {
        if (InventoryManager.Instance == null)
            return 0;

        return InventoryManager.Instance.GetItemQuantity(ammoType);
    }

    private void HandleWeaponListNavigation(bool moveDown)
    {
        List<WeaponData> filteredWeapons = GetFilteredWeapons();
        
        if (filteredWeapons.Count == 0)
        {
            return;
        }

        if (moveDown)
        {
            int nextIndex = currentSelectedWeaponIndex + 1;
            if (nextIndex >= filteredWeapons.Count)
            {
                nextIndex = 0;
            }
            SelectWeaponSlotByIndex(nextIndex);
        }
        else
        {
            int previousIndex = currentSelectedWeaponIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = filteredWeapons.Count - 1;
            }
            SelectWeaponSlotByIndex(previousIndex);
        }
    }

    private void SelectWeaponSlotByIndex(int index)
    {
        List<WeaponData> filteredWeapons = GetFilteredWeapons();
        
        if (index < 0 || index >= filteredWeapons.Count)
        {
            return;
        }

        if (currentlySelectedWeaponSlot != null)
        {
            currentlySelectedWeaponSlot.SetSelected(false);
        }

        currentSelectedWeaponIndex = index;
        currentlySelectedWeaponSlot = weaponSlotUIList[index];
        currentlySelectedWeaponSlot.SetSelected(true);
    }

    private void OnWeaponSelected()
    {
        if (currentlySelectedWeaponSlot != null && currentlySelectedWeaponSlot.GetWeapon() != null)
        {
            WeaponData selectedWeapon = currentlySelectedWeaponSlot.GetWeapon();
            
            if (InventoryManager.Instance != null && !selectedWeapon.CanUse(InventoryManager.Instance))
            {
                Debug.Log("Sem munição para usar esta arma!");
                return;
            }
            
            if (RecentWeaponsManager.Instance != null)
            {
                RecentWeaponsManager.Instance.AddRecentWeapon(selectedWeapon);
            }

            if (selectedWeapon.requiresAmmo && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ConsumeItem(selectedWeapon.ammoType, 1);
            }

            CloseAttackMenu();

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.PlayerAttack(selectedWeapon);
            }
        }
    }

    private void FindOrCreateItemSlots()
    {
        if (itemSlotsParent != null)
        {
            ItemSlotUI[] existingSlots = itemSlotsParent.GetComponentsInChildren<ItemSlotUI>(true);
            
            foreach (ItemSlotUI slot in existingSlots)
            {
                itemSlotUIList.Add(slot);
                slot.gameObject.SetActive(false);
            }
        }
    }

    private void OpenItemsMenu()
    {
        if (itensMenuBackground != null)
        {
            itensMenuBackground.SetActive(true);
            isItemsMenuOpen = true;
            RefreshItemsList();

            itemsMenuState = new UIState("ItemsMenu", OnItemsMenuBack);
            if (UINavigationManager.Instance != null)
            {
                UINavigationManager.Instance.PushState(itemsMenuState);
            }
        }
    }

    private void OnItemsMenuBack()
    {
        CloseItemsMenu();
        OpenMainMenu();
    }

    private void CloseItemsMenu()
    {
        if (itensMenuBackground != null)
        {
            itensMenuBackground.SetActive(false);
            isItemsMenuOpen = false;
            
            if (currentlySelectedItemSlot != null)
            {
                currentlySelectedItemSlot.SetSelected(false);
                currentlySelectedItemSlot = null;
            }
        }

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.PopState();
        }
    }

    private void RefreshItemsList()
    {
        List<ItemData> consumableItems = new List<ItemData>();
        
        if (InventoryManager.Instance != null)
        {
            consumableItems = InventoryManager.Instance.GetAllItemsOfCategory(ItemCategory.Consumable);
        }

        for (int i = 0; i < itemSlotUIList.Count; i++)
        {
            if (i < consumableItems.Count)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(consumableItems[i]);
                itemSlotUIList[i].Setup(consumableItems[i], quantity);
            }
            else
            {
                itemSlotUIList[i].Setup(null, 0);
            }
        }

        if (currentlySelectedItemSlot != null)
        {
            currentlySelectedItemSlot.SetSelected(false);
            currentlySelectedItemSlot = null;
        }

        if (consumableItems.Count == 0)
        {
            if (emptyMessageItems != null)
            {
                emptyMessageItems.SetActive(true);
            }
        }
        else
        {
            if (emptyMessageItems != null)
            {
                emptyMessageItems.SetActive(false);
            }

            SelectItemSlotByIndex(0);
        }
    }

    private void HandleItemsListNavigation(bool moveDown)
    {
        List<ItemData> consumableItems = new List<ItemData>();
        
        if (InventoryManager.Instance != null)
        {
            consumableItems = InventoryManager.Instance.GetAllItemsOfCategory(ItemCategory.Consumable);
        }
        
        if (consumableItems.Count == 0)
        {
            return;
        }

        if (moveDown)
        {
            int nextIndex = currentSelectedItemIndex + 1;
            if (nextIndex >= consumableItems.Count)
            {
                nextIndex = 0;
            }
            SelectItemSlotByIndex(nextIndex);
        }
        else
        {
            int previousIndex = currentSelectedItemIndex - 1;
            if (previousIndex < 0)
            {
                previousIndex = consumableItems.Count - 1;
            }
            SelectItemSlotByIndex(previousIndex);
        }
    }

    private void SelectItemSlotByIndex(int index)
    {
        List<ItemData> consumableItems = new List<ItemData>();
        
        if (InventoryManager.Instance != null)
        {
            consumableItems = InventoryManager.Instance.GetAllItemsOfCategory(ItemCategory.Consumable);
        }
        
        if (index < 0 || index >= consumableItems.Count)
        {
            return;
        }

        if (currentlySelectedItemSlot != null)
        {
            currentlySelectedItemSlot.SetSelected(false);
        }

        currentSelectedItemIndex = index;
        currentlySelectedItemSlot = itemSlotUIList[index];
        currentlySelectedItemSlot.SetSelected(true);
    }

    private void OnItemSelected()
    {
        if (currentlySelectedItemSlot != null && currentlySelectedItemSlot.GetItemData() != null)
        {
            ItemData selectedItem = currentlySelectedItemSlot.GetItemData();
            
            if (InventoryManager.Instance == null)
                return;

            int availableQuantity = InventoryManager.Instance.GetItemQuantity(selectedItem);
            if (availableQuantity <= 0)
            {
                Debug.Log("Você não possui este item!");
                return;
            }

            ConsumableData consumable = selectedItem as ConsumableData;
            if (consumable == null)
            {
                Debug.Log("Este item não é consumível!");
                return;
            }

            InventoryManager.Instance.ConsumeItem(selectedItem, 1);

            CloseItemsMenu();

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.PlayerUseItem(consumable);
            }
        }
    }
}


}
