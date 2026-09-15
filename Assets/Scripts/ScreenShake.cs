namespace HorrorRPG.Presentation
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    private const float DefaultShakeDuration = 0.5f;
    private const float DefaultShakeIntensity = 0.3f;
    private const float DefaultShakeFrequency = 25f;
    private const float SmoothReturnSpeed = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine positionShakeCoroutine;
    private Coroutine rotationShakeCoroutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void Shake()
    {
        Shake(DefaultShakeDuration, DefaultShakeIntensity);
    }

    public void Shake(float duration)
    {
        Shake(duration, DefaultShakeIntensity);
    }

    public void Shake(float duration, float intensity)
    {
        if (positionShakeCoroutine != null)
        {
            StopCoroutine(positionShakeCoroutine);
        }
        positionShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity, DefaultShakeFrequency));
    }

    public void Shake(float duration, float intensity, float frequency)
    {
        if (positionShakeCoroutine != null)
        {
            StopCoroutine(positionShakeCoroutine);
        }
        positionShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity, frequency));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity, float frequency)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime * frequency;
            yield return null;
        }

        while (Vector3.Distance(transform.localPosition, originalPosition) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * SmoothReturnSpeed);
            yield return null;
        }

        transform.localPosition = originalPosition;
        positionShakeCoroutine = null;
    }

    public void ShakeRotation(float duration, float intensity)
    {
        if (rotationShakeCoroutine != null)
        {
            StopCoroutine(rotationShakeCoroutine);
        }
        rotationShakeCoroutine = StartCoroutine(ShakeRotationCoroutine(duration, intensity));
    }

    private IEnumerator ShakeRotationCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float z = Random.Range(-1f, 1f) * intensity;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (Quaternion.Angle(transform.localRotation, originalRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * SmoothReturnSpeed);
            yield return null;
        }

        transform.localRotation = originalRotation;
        rotationShakeCoroutine = null;
    }

    public void Stop()
    {
        if (positionShakeCoroutine != null)
        {
            StopCoroutine(positionShakeCoroutine);
            positionShakeCoroutine = null;
        }
        
        if (rotationShakeCoroutine != null)
        {
            StopCoroutine(rotationShakeCoroutine);
            rotationShakeCoroutine = null;
        }
        
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
}


}
