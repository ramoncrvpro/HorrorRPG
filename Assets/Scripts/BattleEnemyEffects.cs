using System.Collections;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Applies hit feedback to the battle enemy sprite and transform.</summary>
    public class BattleEnemyEffects : MonoBehaviour
    {
        [Header("Hit Flash Settings")]
        [SerializeField] private SpriteRenderer enemyRenderer;
        [SerializeField] private float hitFlashDuration = 0.3f;
        [SerializeField] private float hitFlashIntensity = 1f;

        [Header("Squeeze Settings")]
        [SerializeField] private Transform enemyTransform;
        [SerializeField] private float squeezeDuration = 0.2f;
        [SerializeField] private float squeezeScaleX = 0.7f;
        [SerializeField] private float squeezeScaleY = 1.3f;

        private Vector3 originalScale;
        private Color originalColor = Color.white;
        private Coroutine hitFlashCoroutine;
        private Coroutine squeezeCoroutine;

        private void Awake()
        {
            if (enemyRenderer == null)
            {
                enemyRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (enemyTransform == null)
            {
                enemyTransform = transform;
            }

            if (enemyRenderer == null)
            {
                Debug.LogError($"{nameof(BattleEnemyEffects)} requires a SpriteRenderer on {name}.", this);
            }
            else
            {
                originalColor = enemyRenderer.color;
            }

            originalScale = enemyTransform.localScale;
        }

        /// <summary>Plays both hit-flash and squeeze feedback.</summary>
        public void PlayHitEffects()
        {
            PlayHitFlash();
            PlaySqueezeEffect();
        }

        /// <summary>Plays the temporary color flash on the enemy sprite.</summary>
        public void PlayHitFlash()
        {
            if (enemyRenderer == null)
            {
                return;
            }

            if (hitFlashCoroutine != null)
            {
                StopCoroutine(hitFlashCoroutine);
            }

            hitFlashCoroutine = StartCoroutine(HitFlashCoroutine());
        }

        /// <summary>Plays the temporary squash-and-stretch effect.</summary>
        public void PlaySqueezeEffect()
        {
            if (enemyTransform == null)
            {
                return;
            }

            if (squeezeCoroutine != null)
            {
                StopCoroutine(squeezeCoroutine);
            }

            squeezeCoroutine = StartCoroutine(SqueezeCoroutine());
        }

        private IEnumerator HitFlashCoroutine()
        {
            float duration = Mathf.Max(hitFlashDuration, 0.01f);
            float elapsedTime = 0f;
            Color flashColor = Color.Lerp(originalColor, Color.red, Mathf.Clamp01(hitFlashIntensity));

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                enemyRenderer.color = Color.Lerp(flashColor, originalColor, normalizedTime);
                yield return null;
            }

            enemyRenderer.color = originalColor;
            hitFlashCoroutine = null;
        }

        private IEnumerator SqueezeCoroutine()
        {
            float halfDuration = Mathf.Max(squeezeDuration * 0.5f, 0.01f);
            float elapsedTime = 0f;

            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / halfDuration);
                float scaleX = Mathf.Lerp(1f, squeezeScaleX, normalizedTime);
                float scaleY = Mathf.Lerp(1f, squeezeScaleY, normalizedTime);
                enemyTransform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / halfDuration);
                float scaleX = Mathf.Lerp(squeezeScaleX, 1f, normalizedTime);
                float scaleY = Mathf.Lerp(squeezeScaleY, 1f, normalizedTime);
                enemyTransform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
                yield return null;
            }

            enemyTransform.localScale = originalScale;
            squeezeCoroutine = null;
        }

        private void OnDestroy()
        {
            if (enemyRenderer != null)
            {
                enemyRenderer.color = originalColor;
            }
        }
    }
}
