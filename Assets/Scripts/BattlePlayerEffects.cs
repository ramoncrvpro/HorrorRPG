namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;



using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DefenseType
{
    None,
    Partial,
    Perfect
}

public class BattlePlayerEffects : MonoBehaviour
{
    public static BattlePlayerEffects Instance { get; private set; }

    [Header("Damage Flash Settings")]
    [SerializeField] private Image damageFlashImage;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float flashAlpha = 0.5f;

    [Header("Flash Colors")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color partialDefenseColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private Color perfectDefenseColor = Color.white;

    [Header("Camera Shake Settings")]
    [SerializeField] private bool enableCameraShake = false;
    [SerializeField] private ScreenShake screenShake;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.15f;

    [Header("Item Use Effects")]
    [SerializeField] private ParticleSystem itemParticle;

    private Coroutine damageFlashCoroutine;

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

        if (damageFlashImage != null)
        {
            Color color = damageFlashImage.color;
            color.a = 0f;
            damageFlashImage.color = color;
        }
    }

    public void PlayDamageEffects(DefenseType defenseType = DefenseType.None)
    {
        PlayDamageFlash(defenseType);
        
        if (enableCameraShake)
        {
            PlayCameraShake();
        }
    }

    public void PlayDamageFlash(DefenseType defenseType = DefenseType.None)
    {
        if (damageFlashImage == null)
            return;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        Color flashColor = defenseType switch
        {
            DefenseType.Partial => partialDefenseColor,
            DefenseType.Perfect => perfectDefenseColor,
            _ => damageColor
        };

        damageFlashCoroutine = StartCoroutine(DamageFlashCoroutine(flashColor));
    }

    public void PlayCameraShake()
    {
        if (screenShake != null)
        {
            screenShake.Shake(shakeDuration, shakeIntensity);
            screenShake.ShakeRotation(shakeDuration, shakeIntensity/2);
        }
    }

    public void PlayItemUseEffects()
    {
        if (itemParticle != null)
        {
            itemParticle.Play();
        }
        
        PlayDamageFlash(DefenseType.Partial);
    }

    private IEnumerator DamageFlashCoroutine(Color flashColor)
    {
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / flashDuration;
            float alpha = Mathf.Lerp(flashAlpha, 0f, normalizedTime);
            
            Color color = flashColor;
            color.a = alpha;
            damageFlashImage.color = color;
            
            yield return null;
        }

        Color finalColor = flashColor;
        finalColor.a = 0f;
        damageFlashImage.color = finalColor;
        damageFlashCoroutine = null;
    }
}


}
