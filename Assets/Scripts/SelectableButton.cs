namespace HorrorRPG.Presentation
{



using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image backgroundImage;

    private Color defaultTextColor = Color.white;
    private Color selectedTextColor = Color.black;

    private void Awake()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (buttonText != null)
            defaultTextColor = buttonText.color;

        if (backgroundImage != null)
            backgroundImage.enabled = false;
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.enabled = selected;
        }

        if (buttonText != null)
        {
            buttonText.color = selected ? selectedTextColor : defaultTextColor;
        }
    }

    public void SetText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    public string GetText()
    {
        return buttonText != null ? buttonText.text : "";
    }
}


}
