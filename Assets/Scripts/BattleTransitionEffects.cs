namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;


using UnityEngine;
using UnityEngine.UI;
using System;

public class BattleTransitionEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject effectsObject;
    [SerializeField] private Image fadeImage;
    [SerializeField] private UISpriteAnimator spriteAnimator;

    [Header("Settings")]
    [SerializeField] private float scaleTransitionDuration = 1f;
    [SerializeField] private float fadeTransitionDuration = 1f;
    [SerializeField] private float maxScale = 10f;
    [SerializeField] private LeanTweenType scaleEasing = LeanTweenType.easeOutQuad;



    public void PlayBattleStartEffects(Action onComplete = null)
    {
        ResetEffects();

        if (spriteAnimator != null)
        {
            spriteAnimator.Play();
        }

        ScaleEffect(0f, maxScale, scaleTransitionDuration, () =>
        {
            onComplete?.Invoke();
            FadeEffect(0f, fadeTransitionDuration, () =>
            {
                
            });
        });
    }

    public void PlayBattleEndEffects(Action onComplete = null)
    {
        FadeEffect(1f, fadeTransitionDuration/2, () =>
        {
            onComplete?.Invoke();
            LeanTween.delayedCall(fadeTransitionDuration / 2, () =>
            {
                FadeEffect(0f, fadeTransitionDuration, () =>
                {
                    
                });
            });
        });
    }

    private void ScaleEffect(float fromScale, float toScale, float duration, Action onComplete = null)
    {
        if (effectsObject == null)
        {
            onComplete?.Invoke();
            return;
        }

        effectsObject.transform.localScale = Vector3.one * fromScale;

        LeanTween.scale(effectsObject, Vector3.one * toScale, duration)
            .setEase(scaleEasing)
            .setOnComplete(() => onComplete?.Invoke());
    }

    private void FadeEffect(float toAlpha, float duration, Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            return;
        }

        LeanTween.alpha(fadeImage.rectTransform, toAlpha, duration)
            .setOnComplete(() => onComplete?.Invoke());
    }

    private void ResetEffects()
    {
        if (effectsObject != null)
        {
            effectsObject.transform.localScale = Vector3.zero;
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
    }
}


}
