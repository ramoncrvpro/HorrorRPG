namespace HorrorRPG.Presentation
{

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BaseTab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tabText;
    [SerializeField] private Image tabImage;

    private Color selectedTextColor = Color.black;
    private Color deselectedTextColor = Color.white;

    protected void SetTabText(string displayText)
    {
        if (tabText != null)
        {
            tabText.text = displayText;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (tabText != null)
        {
            tabText.color = isSelected ? selectedTextColor : deselectedTextColor;
        }

        if (tabImage != null)
        {
            tabImage.enabled = !isSelected;
        }
    }
}


}
