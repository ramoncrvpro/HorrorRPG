using UnityEngine;
using UnityEngine.UI;

namespace HorrorRPG.Battle
{
    /// <summary>Visual-only timing bar presentation.</summary>
    public class AttackTimingUI : MonoBehaviour
    {
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
            Hide();
            SetupColors();
        }

        /// <summary>Shows the timing panel.</summary>
        public void Show() => timingBarPanel?.SetActive(true);

        /// <summary>Hides the timing panel.</summary>
        public void Hide() => timingBarPanel?.SetActive(false);

        /// <summary>Updates the marker along the normalized timing bar.</summary>
        public void UpdateMarkerPosition(float normalizedPosition)
        {
            if (marker == null || barBackground == null) return;
            float barWidth = barBackground.rect.width;
            Vector2 localPosition = marker.anchoredPosition;
            localPosition.x = Mathf.Clamp01(normalizedPosition) * barWidth - barWidth * 0.5f;
            marker.anchoredPosition = localPosition;
        }

        /// <summary>Sizes timing zones from the selected weapon definition.</summary>
        public void SetupZones(WeaponData weapon)
        {
            if (barBackground == null || weapon == null) return;
            float totalWidth = barBackground.rect.width;
            float criticalMinimum = weapon.criticalZoneCenter - weapon.criticalZoneWidth * 0.5f;
            float criticalMaximum = weapon.criticalZoneCenter + weapon.criticalZoneWidth * 0.5f;
            float hitLeftMinimum = Mathf.Max(0f, criticalMinimum - weapon.hitZoneWidth);
            float hitRightMaximum = Mathf.Min(1f, criticalMaximum + weapon.hitZoneWidth);
            SetZoneSize(missZoneLeft, 0f, hitLeftMinimum, totalWidth);
            SetZoneSize(hitZoneLeft, hitLeftMinimum, criticalMinimum, totalWidth);
            SetZoneSize(criticalZone, criticalMinimum, criticalMaximum, totalWidth);
            SetZoneSize(hitZoneRight, criticalMaximum, hitRightMaximum, totalWidth);
            SetZoneSize(missZoneRight, hitRightMaximum, 1f, totalWidth);
            LayoutRebuilder.ForceRebuildLayoutImmediate(barBackground);
        }

        private void SetupColors()
        {
            if (missZoneLeft != null) missZoneLeft.color = missColor;
            if (missZoneRight != null) missZoneRight.color = missColor;
            if (hitZoneLeft != null) hitZoneLeft.color = hitColor;
            if (hitZoneRight != null) hitZoneRight.color = hitColor;
            if (criticalZone != null) criticalZone.color = criticalColor;
            if (marker != null && marker.TryGetComponent(out Image markerImage)) markerImage.color = markerColor;
        }

        private static void SetZoneSize(Image zoneImage, float startNormalized, float endNormalized, float totalWidth)
        {
            if (zoneImage == null) return;
            zoneImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(0f, endNormalized - startNormalized) * totalWidth);
        }
    }
}
