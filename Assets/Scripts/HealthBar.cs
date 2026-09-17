namespace HorrorRPG.Presentation
{



using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarImage;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 2f;

    private const string DEFAULT_PLAYER_NAME = "player";

    private int currentHealthValue;
    private int maxHealthValue;
    private float targetFillAmount;
    private Coroutine animationCoroutine;

    public void SetName(string characterName)
    {
        if (nameText != null)
        {
            nameText.text = characterName;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        bool shouldAnimate = currentHealth < currentHealthValue;
        
        currentHealthValue = currentHealth;
        maxHealthValue = maxHealth;

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if (healthBarImage != null)
        {
            targetFillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
            if (shouldAnimate)
            {
                animationCoroutine = StartCoroutine(AnimateHealthBar());
            }
            else
            {
                healthBarImage.fillAmount = targetFillAmount;
            }
        }
    }

    private IEnumerator AnimateHealthBar()
    {
        float currentFillAmount = healthBarImage.fillAmount;
        
        while (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * animationSpeed);
            healthBarImage.fillAmount = currentFillAmount;
            yield return null;
        }
        
        healthBarImage.fillAmount = targetFillAmount;
        animationCoroutine = null;
    }

    public void Initialize(string characterName, int currentHealth, int maxHealth)
    {
        SetName(characterName);
        
        currentHealthValue = currentHealth;
        maxHealthValue = maxHealth;
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
        
        if (healthBarImage != null)
        {
            targetFillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            healthBarImage.fillAmount = targetFillAmount;
        }
    }

    public void InitializeAsPlayerHealthBar(int currentHealth, int maxHealth)
    {
        Initialize(DEFAULT_PLAYER_NAME, currentHealth, maxHealth);
    }
}


}
