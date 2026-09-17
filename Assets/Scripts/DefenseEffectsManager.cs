using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Displays the defense effect matching the selected region.</summary>
    public class DefenseEffectsManager : MonoBehaviour
    {
        [SerializeField] private DefenseEffectUI leftHandEffect;
        [SerializeField] private DefenseEffectUI middleHandEffect;
        [SerializeField] private DefenseEffectUI rightHandEffect;

        /// <summary>Triggers one positional defense effect.</summary>
        public void TriggerEffect(DefensePosition position)
        {
            switch (position)
            {
                case DefensePosition.Left: leftHandEffect?.Trigger(); break;
                case DefensePosition.Up: middleHandEffect?.Trigger(); break;
                case DefensePosition.Right: rightHandEffect?.Trigger(); break;
            }
        }

        /// <summary>Immediately hides every defense effect.</summary>
        public void HideAllEffects()
        {
            leftHandEffect?.ForceHide();
            middleHandEffect?.ForceHide();
            rightHandEffect?.ForceHide();
        }
    }
}
