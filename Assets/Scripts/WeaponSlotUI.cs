using HorrorRPG.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorRPG.Inventory
{
    /// <summary>Renders one weapon selection entry without manager coupling.</summary>
    public class WeaponSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemAmountText;
        [SerializeField] private Image backgroundImage;

        private readonly Color selectedNameColor = Color.black;
        private readonly Color selectedAmountColor = Color.black;
        private Color defaultNameColor = Color.white;
        private Color defaultAmountColor = Color.white;
        private WeaponData currentWeapon;

        private void Awake()
        {
            if (itemNameText != null) defaultNameColor = itemNameText.color;
            if (itemAmountText != null) defaultAmountColor = itemAmountText.color;
            SetSelected(false);
        }

        /// <summary>Renders a weapon and its current level.</summary>
        public void Setup(WeaponData weapon, int level)
        {
            currentWeapon = weapon;
            SetSelected(false);
            if (weapon == null)
            {
                gameObject.SetActive(false);
                return;
            }
            int visibleLevel = Mathf.Clamp(level, 1, weapon.maxLevel);
            if (itemNameText != null) itemNameText.text = weapon.itemName;
            if (itemAmountText != null) itemAmountText.text = $"LVL{visibleLevel}";
            gameObject.SetActive(true);
        }

        /// <summary>Updates selection colors and background visibility.</summary>
        public void SetSelected(bool selected)
        {
            if (backgroundImage != null) backgroundImage.enabled = selected;
            if (itemNameText != null) itemNameText.color = selected ? selectedNameColor : defaultNameColor;
            if (itemAmountText != null) itemAmountText.color = selected ? selectedAmountColor : defaultAmountColor;
        }

        /// <summary>Returns the weapon currently rendered.</summary>
        public WeaponData GetWeapon() => currentWeapon;
    }
}
