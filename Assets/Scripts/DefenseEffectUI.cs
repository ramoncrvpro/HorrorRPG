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

public class DefenseEffectUI : MonoBehaviour
{
    private const float DEFAULT_ACTIVATION_DELAY = 0.2f;
    private const float DEFAULT_ACTIVE_DURATION = 0.6f;

    [Header("Timing Settings")]
    [SerializeField] private float activationDelay = DEFAULT_ACTIVATION_DELAY;
    [SerializeField] private float activeDuration = DEFAULT_ACTIVE_DURATION;
    [SerializeField] private GameObject effectObject;

    private Coroutine activeEffectCoroutine;

    private void Awake()
    {
        effectObject.SetActive(false);
    }

    public void Trigger()
    {
        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
        }

        activeEffectCoroutine = StartCoroutine(ShowEffectRoutine());
    }

    private IEnumerator ShowEffectRoutine()
    {
        yield return new WaitForSeconds(activationDelay);

        effectObject.SetActive(true);
        
        yield return new WaitForSeconds(activeDuration);

        effectObject.SetActive(false);
        activeEffectCoroutine = null;
    }

    public void ForceHide()
    {
        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
            activeEffectCoroutine = null;
        }
        
        gameObject.SetActive(false);
    }
}


}
