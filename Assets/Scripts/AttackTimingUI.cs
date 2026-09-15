namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using UnityEngine.UI;

public class AttackTimingUI : MonoBehaviour
{
    public static AttackTimingUI Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject timingBarPanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private Image missZoneLeft;
    [SerializeField] private Image hitZoneLeft;
    [SerializeField] private Image criticalZone;
    [SerializeField] private Image hitZoneRight;
    [SerializeField] private Image missZoneRight;
    [SerializeField] private RectTransform marker;
    
    [Header("Colors")]
    [SerializeField] private Color missColor = Color.black;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color markerColor = new Color(1f, 0.3f, 0f);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (timingBarPanel != null)
        {
            timingBarPanel.SetActive(false);
        }
        
        SetupColors();
    }
    
    private void SetupColors()
    {
        if (missZoneLeft != null) missZoneLeft.color = missColor;
        if (missZoneRight != null) missZoneRight.color = missColor;
        if (hitZoneLeft != null) hitZoneLeft.color = hitColor;
        if (hitZoneRight != null) hitZoneRight.color = hitColor;
        if (criticalZone != null) criticalZone.color = criticalColor;
        
        if (marker != null)
        {
            Image markerImage = marker.GetComponent<Image>();
            if (markerImage != null)
            {
                markerImage.color = markerColor;
            }
        }
    }
    
    public void Show()
    {
        if (timingBarPanel != null)
        {
            timingBarPanel.SetActive(true);
        }
    }
    
    public void Hide()
    {
        if (timingBarPanel != null)
        {
            timingBarPanel.SetActive(false);
        }
    }
    
    public void UpdateMarkerPosition(float normalizedPosition)
    {
        if (marker == null || barBackground == null)
            return;
        
        float barWidth = barBackground.rect.width;
        Vector2 localPos = marker.anchoredPosition;
        localPos.x = (normalizedPosition * barWidth) - (barWidth / 2f);
        marker.anchoredPosition = localPos;
    }
    
    public void SetupZones(WeaponData weapon)
    {
        if (barBackground == null)
            return;
        
        float totalWidth = barBackground.rect.width;
        
        float critMin = weapon.criticalZoneCenter - (weapon.criticalZoneWidth / 2f);
        float critMax = weapon.criticalZoneCenter + (weapon.criticalZoneWidth / 2f);
        
        float hitLeftMin = Mathf.Max(0f, critMin - weapon.hitZoneWidth);
        float hitLeftMax = critMin;
        
        float hitRightMin = critMax;
        float hitRightMax = Mathf.Min(1f, critMax + weapon.hitZoneWidth);
        
        SetZoneSize(missZoneLeft, 0f, hitLeftMin, totalWidth);
        SetZoneSize(hitZoneLeft, hitLeftMin, hitLeftMax, totalWidth);
        SetZoneSize(criticalZone, critMin, critMax, totalWidth);
        SetZoneSize(hitZoneRight, hitRightMin, hitRightMax, totalWidth);
        SetZoneSize(missZoneRight, hitRightMax, 1f, totalWidth);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(barBackground);
    }
    
    private void SetZoneSize(Image zoneImage, float startNorm, float endNorm, float totalWidth)
    {
        if (zoneImage == null)
            return;
        
        RectTransform zoneRect = zoneImage.rectTransform;
        float width = (endNorm - startNorm) * totalWidth;
        
        zoneRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}


}
