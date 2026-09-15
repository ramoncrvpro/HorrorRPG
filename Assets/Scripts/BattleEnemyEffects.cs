namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System.Collections;

public class BattleEnemyEffects : MonoBehaviour
{
    [Header("Hit Flash Settings")]
    [SerializeField] private MeshRenderer enemyRenderer;
    [SerializeField] private float hitFlashDuration = 0.3f;
    [SerializeField] private float hitFlashIntensity = 1f;

    [Header("Squeeze Settings")]
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private float squeezeDuration = 0.2f;
    [SerializeField] private float squeezeScaleX = 0.7f;
    [SerializeField] private float squeezeScaleY = 1.3f;

    private static readonly int HitEffectProperty = Shader.PropertyToID("_HitEffect");
    private MaterialPropertyBlock propertyBlock;
    private Vector3 originalScale;
    private Coroutine hitFlashCoroutine;
    private Coroutine squeezeCoroutine;

    private void Awake()
    {
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponent<MeshRenderer>();
        }

        if (enemyTransform == null)
        {
            enemyTransform = transform;
        }

        if (enemyRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        else
        {
            Debug.LogError("Enemy Renderer não encontrado!");
        }

        originalScale = enemyTransform.localScale;
    }

    public void PlayHitEffects()
    {
        PlayHitFlash();
        PlaySqueezeEffect();
    }

    public void PlayHitFlash()
    {
        if (enemyRenderer == null || propertyBlock == null)
        {
            Debug.LogError("PlayHitFlash: enemyRenderer ou propertyBlock é null!");
            return;
        }

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }

        hitFlashCoroutine = StartCoroutine(HitFlashCoroutine());
    }

    public void PlaySqueezeEffect()
    {
        if (enemyTransform == null)
            return;

        if (squeezeCoroutine != null)
        {
            StopCoroutine(squeezeCoroutine);
        }

        squeezeCoroutine = StartCoroutine(SqueezeCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < hitFlashDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / hitFlashDuration;
            float flashValue = Mathf.Lerp(hitFlashIntensity, 0f, normalizedTime);
            
            enemyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(HitEffectProperty, flashValue);
            enemyRenderer.SetPropertyBlock(propertyBlock);
            
            yield return null;
        }

        enemyRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(HitEffectProperty, 0f);
        enemyRenderer.SetPropertyBlock(propertyBlock);
        
        hitFlashCoroutine = null;
    }

    private IEnumerator SqueezeCoroutine()
    {
        float halfDuration = squeezeDuration * 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / halfDuration;
            float scaleX = Mathf.Lerp(1f, squeezeScaleX, normalizedTime);
            float scaleY = Mathf.Lerp(1f, squeezeScaleY, normalizedTime);
            
            enemyTransform.localScale = new Vector3(
                originalScale.x * scaleX,
                originalScale.y * scaleY,
                originalScale.z
            );
            
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / halfDuration;
            float scaleX = Mathf.Lerp(squeezeScaleX, 1f, normalizedTime);
            float scaleY = Mathf.Lerp(squeezeScaleY, 1f, normalizedTime);
            
            enemyTransform.localScale = new Vector3(
                originalScale.x * scaleX,
                originalScale.y * scaleY,
                originalScale.z
            );
            
            yield return null;
        }

        enemyTransform.localScale = originalScale;
        squeezeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (enemyRenderer != null && propertyBlock != null)
        {
            enemyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(HitEffectProperty, 0f);
            enemyRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}


}
