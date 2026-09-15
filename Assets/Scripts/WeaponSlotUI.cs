namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private Image backgroundImage;

    private WeaponData currentWeapon;
    private BattleUIManager battleUIManager;
    private Color defaultNameColor = Color.white;
    private Color defaultAmountColor = Color.white;
    private Color selectedNameColor = Color.black;
    private Color selectedAmountColor = Color.black;

    private void Awake()
    {
        if (itemNameText == null)
            itemNameText = transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
        
        if (itemAmountText == null)
            itemAmountText = transform.Find("ItemAmountText").GetComponent<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (itemNameText != null)
            defaultNameColor = itemNameText.color;
        
        if (itemAmountText != null)
            defaultAmountColor = itemAmountText.color;
    }

    public void Setup(WeaponData weapon, BattleUIManager manager, int ammoCount = -1)
    {
        currentWeapon = weapon;
        battleUIManager = manager;

        if (weapon != null)
        {
            itemNameText.text = weapon.itemName;
            
            if (ammoCount >= 0)
            {
                itemAmountText.text = "x" + ammoCount.ToString();
            }
            else
            {
                itemAmountText.text = "";
            }
            
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.enabled = selected;
        }

        if (itemNameText != null)
        {
            itemNameText.color = selected ? selectedNameColor : defaultNameColor;
        }

        if (itemAmountText != null)
        {
            itemAmountText.color = selected ? selectedAmountColor : defaultAmountColor;
        }
    }

    public WeaponData GetWeapon()
    {
        return currentWeapon;
    }
}


}
