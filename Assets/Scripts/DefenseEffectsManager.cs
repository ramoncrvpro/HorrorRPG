namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public class DefenseEffectsManager : MonoBehaviour
{
    public static DefenseEffectsManager Instance { get; private set; }

    [Header("Hand Effects")]
    [SerializeField] private DefenseEffectUI leftHandEffect;
    [SerializeField] private DefenseEffectUI middleHandEffect;
    [SerializeField] private DefenseEffectUI rightHandEffect;

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
    }

    public void TriggerEffect(DefensePosition position)
    {
        switch (position)
        {
            case DefensePosition.Left:
                leftHandEffect?.Trigger();
                break;
            case DefensePosition.Up:
                middleHandEffect?.Trigger();
                break;
            case DefensePosition.Right:
                rightHandEffect?.Trigger();
                break;
        }
    }

    public void HideAllEffects()
    {
        leftHandEffect?.ForceHide();
        middleHandEffect?.ForceHide();
        rightHandEffect?.ForceHide();
    }
}


}
