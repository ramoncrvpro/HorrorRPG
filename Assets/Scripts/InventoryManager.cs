namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Image itemIconDisplay;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private ConfirmationMenu discardConfirmationMenu;
    [SerializeField] private GameObject emptyMessageObject;
    [SerializeField] private GameObject descriptionBackground;
    [SerializeField] private GameObject iconBackground;

    [Header("Tabs")]
    [SerializeField] private Transform tabsParent;
    [SerializeField] private GameObject tabPrefab;

    [Header("Prefab")]
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Default Items")]
    [SerializeField] private WeaponData defaultWeapon;

    [Header("Settings")]
    [SerializeField] private int maxSlots = 9;
    [SerializeField] private float itemAddDelay = 0.25f;

    [Header("Prompt Messages")]
    private const string inventoryFullMessage = "Inventario de consumiveis cheio";
    private const string partialPickupMessage = "Alguns itens ficaram para trás";
    [SerializeField] private float promptDisplayDuration = 2f;

    private const string CONTROL_LOCK_ID = "InventorySystem";
    private const string DISCARD_CONFIRMATION_MESSAGE = "Tem certeza que deseja descartar este item?";

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();
    private ItemSlotUI currentlySelectedSlot;
    private bool isInventoryOpen = false;
    private int currentSelectedIndex = -1;
    private bool isDiscardMenuOpen = false;
    private string originalDescriptionText = "";

    private List<InventoryTab> tabs = new List<InventoryTab>();
    private int currentTabIndex = 0;
    private ItemCategory currentCategory = ItemCategory.Consumable;

    private UIState inventoryState;
    private UIState discardMenuState;
    private int lastAddedQuantity;
    [SerializeField] private GameInputReader inputReader;


    private void Awake()
    {
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        Instance = this;
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(false);
        }

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.gameObject.SetActive(false);
        }

        InitializeTabs();
        FindOrCreateSlots();
    }
    private void OnEnable()
    {
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        if (inputReader == null) return;
        inputReader.OpenInventoryPerformed += ToggleInventory;
        inputReader.NavigatePerformed += HandleNavigation;
        inputReader.SubmitPerformed += HandleSubmit;
        inputReader.CancelPerformed += HandleCancel;
    }
    private void OnDisable()
    {
        if (inputReader == null) return;
        inputReader.OpenInventoryPerformed -= ToggleInventory;
        inputReader.NavigatePerformed -= HandleNavigation;
        inputReader.SubmitPerformed -= HandleSubmit;
        inputReader.CancelPerformed -= HandleCancel;
    }



    private void Start()
    {
        if (defaultWeapon != null && !HasItem(defaultWeapon, 1))
        {
            AddItem(defaultWeapon, 1);
        }
    }

    private void FindOrCreateSlots()
    {
        if (itemsGridParent != null)
        {
            ItemSlotUI[] existingSlots = itemsGridParent.GetComponentsInChildren<ItemSlotUI>(true);
            
            foreach (ItemSlotUI slot in existingSlots)
            {
                slotUIList.Add(slot);
                slot.gameObject.SetActive(false);
            }

            int slotsToCreate = maxSlots - slotUIList.Count;
            for (int i = 0; i < slotsToCreate; i++)
            {
                if (itemSlotPrefab != null)
                {
                    GameObject newSlot = Instantiate(itemSlotPrefab, itemsGridParent);
                    ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();
                    if (slotUI != null)
                    {
                        slotUIList.Add(slotUI);
                        slotUI.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void InitializeTabs()
    {
        if (tabsParent == null || tabPrefab == null)
        {
            return;
        }

        ItemCategory[] categories = { ItemCategory.Consumable, ItemCategory.Equipable, ItemCategory.Key };
        string[] tabNames = { "CONS", "EQUIP", "KEY" };

        for (int i = 0; i < categories.Length; i++)
        {
            GameObject tabObject = Instantiate(tabPrefab, tabsParent);
            InventoryTab tab = tabObject.GetComponent<InventoryTab>();
            
            if (tab == null)
            {
                tab = tabObject.AddComponent<InventoryTab>();
            }

            tab.Initialize(categories[i], tabNames[i]);
            tabs.Add(tab);
        }

        currentTabIndex = 0;
        currentCategory = ItemCategory.Consumable;
        UpdateTabsVisuals();
    }

    private void HandleNavigation(Vector2 navigation)
    {
        if (!isInventoryOpen) return;
        if (isDiscardMenuOpen)
        {
            discardConfirmationMenu?.HandleNavigation(navigation);
            return;
        }
        if (navigation.sqrMagnitude < 0.25f) return;
        if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y)) HandleTabNavigation(navigation.x > 0f);
        else HandleInventoryNavigation(navigation.y < 0f);
    }

    private void HandleSubmit()
    {
        if (!isInventoryOpen) return;
        if (isDiscardMenuOpen) discardConfirmationMenu?.ExecuteCurrentSelection();
        else OpenDiscardMenu();
    }



    private void HandleCancel()
    {
        if (!isInventoryOpen) return;
        if (isDiscardMenuOpen) OnDiscardCancelled();
        else CloseInventory();
    }


    public void ToggleInventory()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsInBattle())
        {
            return;
        }

        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            inputReader?.SetContext(InputContext.UI);
            currentTabIndex = 0;
            currentCategory = ItemCategory.Consumable;
            UpdateTabsVisuals();
            RefreshInventoryUI();
            
            if (PlayerControlManager.Instance != null)
            {
                PlayerControlManager.Instance.LockControl(CONTROL_LOCK_ID);
            }

            inventoryState = new UIState("Inventory", CloseInventory);
            if (UINavigationManager.Instance != null)
            {
                UINavigationManager.Instance.PushState(inventoryState);
            }
        }
        else
        {
            CloseInventory();
        }
    }

    private void CloseInventory()
    {
        isInventoryOpen = false;

        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(false);
        }

        if (PlayerControlManager.Instance != null)
        {
            PlayerControlManager.Instance.UnlockControl(CONTROL_LOCK_ID);
        }

        inputReader?.SetContext(InputContext.Gameplay);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.PopState();
        }
    }

    public int AddItem(ItemData itemData, int quantity = 1)
    {
        StartCoroutine(AddItemWithDelay(itemData, quantity));
        return quantity;
    }

    public IEnumerator AddItemCoroutine(ItemData itemData, int quantity = 1)
    {
        yield return StartCoroutine(AddItemWithDelay(itemData, quantity));
    }

    public int GetLastAddedQuantity()
    {
        return lastAddedQuantity;
    }

    private IEnumerator AddItemWithDelay(ItemData itemData, int quantity)
    {
        PlayItemPickupAnimation();
        
        yield return new WaitForSeconds(itemAddDelay);
        
        int remainingQuantity = quantity;

        while (remainingQuantity > 0)
        {
            InventorySlot existingSlot = inventorySlots.Find(slot => slot.CanStack(itemData));

            if (existingSlot != null)
            {
                remainingQuantity = existingSlot.AddQuantity(remainingQuantity);
            }
            else
            {
                if (itemData.category == ItemCategory.Consumable && GetUsedConsumableSlots() >= maxSlots)
                {
                    break;
                }

                int quantityToAdd = Mathf.Min(remainingQuantity, itemData.maxStackSize);
                inventorySlots.Add(new InventorySlot(itemData, quantityToAdd));
                remainingQuantity -= quantityToAdd;
            }
        }

        if (isInventoryOpen)
        {
            RefreshInventoryUI();
        }

        int addedQuantity = quantity - remainingQuantity;
        lastAddedQuantity = addedQuantity;

        if (addedQuantity == 0)
        {
            string categoryName = GetCategoryDisplayName(itemData.category);
            string message = itemData.category == ItemCategory.Consumable 
                ? inventoryFullMessage 
                : $"Não foi possível adicionar {itemData.itemName}";
            ShowInventoryMessage(message);
        }
        else if (addedQuantity < quantity)
        {
            string message = $"Você pegou {addedQuantity} de {itemData.itemName}\n{partialPickupMessage}";
            ShowInventoryMessage(message);
        }
        else
        {
            string message = $"Você pegou {addedQuantity} de {itemData.itemName}";
            ShowInventoryMessage(message);
        }
    }

    private void PlayItemPickupAnimation()
    {
        if (HandAnimationManager.Instance != null)
        {
            HandAnimationManager.Instance.PlayReachAnimationRightHand();
        }
    }

    private int GetUsedConsumableSlots()
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.category == ItemCategory.Consumable)
            {
                count++;
            }
        }
        return count;
    }

    private string GetCategoryDisplayName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Consumable:
                return "Consumível";
            case ItemCategory.Equipable:
                return "Equipamento";
            case ItemCategory.Key:
                return "Chave";
            default:
                return "Item";
        }
    }

    private void ShowInventoryMessage(string message)
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.ShowPromptWithDuration(message, promptDisplayDuration);
        }
    }

    private void RefreshInventoryUI()
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < filteredSlots.Count)
            {
                slotUIList[i].Setup(filteredSlots[i], this);
            }
            else
            {
                slotUIList[i].Setup(null, this);
            }
        }

        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.SetSelected(false);
            currentlySelectedSlot = null;
        }

        ClearItemDisplay();

        if (filteredSlots.Count == 0)
        {
            if (emptyMessageObject != null)
            {
                emptyMessageObject.SetActive(true);
            }

            if (descriptionBackground != null)
            {
                descriptionBackground.SetActive(false);
            }

            if (iconBackground != null)
            {
                iconBackground.SetActive(false);
            }
        }
        else
        {
            if (emptyMessageObject != null)
            {
                emptyMessageObject.SetActive(false);
            }

            if (descriptionBackground != null)
            {
                descriptionBackground.SetActive(true);
            }

            if (iconBackground != null)
            {
                iconBackground.SetActive(true);
            }

            if (isInventoryOpen)
            {
                SelectSlotByIndex(0);
            }
        }
    }

    private List<InventorySlot> GetFilteredSlots()
    {
        List<InventorySlot> filtered = new List<InventorySlot>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.category == currentCategory)
            {
                filtered.Add(slot);
            }
        }
        
        return filtered;
    }

    private void UpdateTabsVisuals()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].SetSelected(i == currentTabIndex);
        }
    }

    private void HandleTabNavigation(bool moveRight)
    {
        if (tabs.Count == 0) return;
        if (moveRight)
        {
            currentTabIndex = (currentTabIndex + 1) % tabs.Count;
        }
        else
        {
            currentTabIndex--;
            if (currentTabIndex < 0) currentTabIndex = tabs.Count - 1;
        }
        currentCategory = tabs[currentTabIndex].GetCategory();
        UpdateTabsVisuals();
        RefreshInventoryUI();
    }

    public void SelectSlot(ItemSlotUI selectedSlot)
    {
        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.SetSelected(false);
        }

        currentlySelectedSlot = selectedSlot;
        currentlySelectedSlot.SetSelected(true);

        List<InventorySlot> filteredSlots = GetFilteredSlots();
        currentSelectedIndex = slotUIList.IndexOf(selectedSlot);

        DisplayItemDetails(selectedSlot.GetSlot());
    }

    private void SelectSlotByIndex(int index)
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();
        
        if (index < 0 || index >= filteredSlots.Count)
        {
            return;
        }

        if (index < slotUIList.Count && slotUIList[index].GetSlot() != null)
        {
            SelectSlot(slotUIList[index]);
        }
    }

    private void HandleInventoryNavigation(bool moveDown)
    {
        List<InventorySlot> filteredSlots = GetFilteredSlots();
        if (filteredSlots.Count == 0) return;
        int nextIndex = moveDown ? currentSelectedIndex + 1 : currentSelectedIndex - 1;
        if (nextIndex >= filteredSlots.Count) nextIndex = 0;
        if (nextIndex < 0) nextIndex = filteredSlots.Count - 1;
        SelectSlotByIndex(nextIndex);
    }

    private void HandleDiscardMenuNavigation() { }


    private void OpenDiscardMenu()
    {
        if (currentlySelectedSlot == null || currentlySelectedSlot.GetSlot() == null)
        {
            return;
        }

        if (!currentlySelectedSlot.GetSlot().itemData.disposable)
        {
            return;
        }

        isDiscardMenuOpen = true;
        originalDescriptionText = itemDescriptionText != null ? itemDescriptionText.text : "";

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = DISCARD_CONFIRMATION_MESSAGE;
        }

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.gameObject.SetActive(true);
            discardConfirmationMenu.Show(OnDiscardConfirmed, OnDiscardCancelled);

            discardMenuState = new UIState("DiscardMenu", OnDiscardCancelled);
            if (UINavigationManager.Instance != null)
            {
                UINavigationManager.Instance.PushState(discardMenuState);
            }
        }
    }

    private void OnDiscardConfirmed()
    {
        if (currentlySelectedSlot != null)
        {
            InventorySlot slotToRemove = currentlySelectedSlot.GetSlot();
            if (slotToRemove != null && inventorySlots.Contains(slotToRemove))
            {
                inventorySlots.Remove(slotToRemove);
            }
        }

        CloseDiscardMenu();
        RefreshInventoryUI();
    }

    private void OnDiscardCancelled()
    {
        CloseDiscardMenu();
        
        if (currentlySelectedSlot != null)
        {
            DisplayItemDetails(currentlySelectedSlot.GetSlot());
        }
    }

    private void CloseDiscardMenu()
    {
        isDiscardMenuOpen = false;

        if (discardConfirmationMenu != null)
        {
            discardConfirmationMenu.Hide();
        }

        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.PopState();
        }
    }

    public int GetSlotIndex(ItemSlotUI slotUI)
    {
        return slotUIList.IndexOf(slotUI);
    }

    private void DisplayItemDetails(InventorySlot slot)
    {
        if (slot == null || slot.itemData == null)
        {
            ClearItemDisplay();
            return;
        }

        if (itemIconDisplay != null)
        {
            itemIconDisplay.sprite = slot.itemData.icon;
            itemIconDisplay.enabled = slot.itemData.icon != null;
        }

        if (itemDescriptionText != null)
        {
            string description = slot.itemData.description;
            
            if (!slot.itemData.disposable)
            {
                description += ". Cannot be discarted";
            }
            
            itemDescriptionText.text = description;
        }
    }

    private void ClearItemDisplay()
    {
        if (itemIconDisplay != null)
        {
            itemIconDisplay.sprite = null;
            itemIconDisplay.enabled = false;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
    }

    public bool HasItem(ItemData itemData, int quantity)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                if (slot.quantity >= quantity)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool ConsumeItem(ItemData itemData, int quantity)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                if (slot.quantity >= quantity)
                {
                    slot.quantity -= quantity;
                    if (slot.quantity <= 0)
                    {
                        inventorySlots.Remove(slot);
                    }
                    RefreshInventoryUI();
                    return true;
                }
            }
        }
        return false;
    }

    public List<ItemData> GetAllItemsOfCategory(ItemCategory category)
    {
        List<ItemData> items = new List<ItemData>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData != null && slot.itemData.category == category)
            {
                items.Add(slot.itemData);
            }
        }

        return items;
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> items = new List<ItemData>();
        
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData != null)
            {
                items.Add(slot.itemData);
            }
        }

        return items;
    }

    public int GetItemQuantity(ItemData itemData)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.itemData == itemData)
            {
                return slot.quantity;
            }
        }

        return 0;
    }
}


}
