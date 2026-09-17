using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Player
{
    /// <summary>Controls walking and reach presentation for both player hands.</summary>
    public class HandAnimationManager : MonoBehaviour
    {
        [Header("Hand References")]
        [SerializeField] private UISpriteAnimator leftHandAnimator;
        [SerializeField] private UISpriteAnimator rightHandAnimator;
        [Header("Animation")]
        [SerializeField] private UISpriteAnimation reachAnimation;
        [Header("Swing Animation Settings")]
        [SerializeField] private float swingAmplitudeX = 15f;
        [SerializeField] private float swingAmplitudeY = 10f;
        [SerializeField] private float swingSpeed = 2f;
        [SerializeField] private float returnToDefaultSpeed = 5f;

        private RectTransform leftHandRect;
        private RectTransform rightHandRect;
        private Vector2 leftHandDefaultPosition;
        private Vector2 rightHandDefaultPosition;
        private bool isWalking;
        private float walkTimer;

        private void Awake()
        {
            if (leftHandAnimator != null)
            {
                leftHandRect = leftHandAnimator.GetComponent<RectTransform>();
                leftHandDefaultPosition = leftHandRect.anchoredPosition;
            }
            if (rightHandAnimator != null)
            {
                rightHandRect = rightHandAnimator.GetComponent<RectTransform>();
                rightHandDefaultPosition = rightHandRect.anchoredPosition;
            }
        }

        private void Update()
        {
            if (isWalking) AnimateWalk();
            else ReturnToDefaultPosition();
        }

        /// <summary>Starts the hand walking loop.</summary>
        public void StartWalking()
        {
            isWalking = true;
            walkTimer = 0f;
        }

        /// <summary>Stops walking and returns hands to their default positions.</summary>
        public void StopWalking() => isWalking = false;

        /// <summary>Plays reach animation on both hands.</summary>
        public void PlayReachAnimation()
        {
            if (reachAnimation == null) return;
            leftHandAnimator?.Play(reachAnimation);
            rightHandAnimator?.Play(reachAnimation);
        }

        /// <summary>Plays reach animation on the left hand.</summary>
        public void PlayReachAnimationLeftHand()
        {
            if (reachAnimation != null) leftHandAnimator?.Play(reachAnimation);
        }

        /// <summary>Plays reach animation on the right hand.</summary>
        public void PlayReachAnimationRightHand()
        {
            if (reachAnimation != null) rightHandAnimator?.Play(reachAnimation);
        }

        private void AnimateWalk()
        {
            walkTimer += Time.deltaTime * swingSpeed;
            float swingX = Mathf.Sin(walkTimer) * swingAmplitudeX;
            float swingY = Mathf.Abs(Mathf.Sin(walkTimer)) * swingAmplitudeY;
            SetHandOffset(leftHandRect, leftHandDefaultPosition, swingX, swingY);
            SetHandOffset(rightHandRect, rightHandDefaultPosition, swingX, swingY);
        }

        private void ReturnToDefaultPosition()
        {
            MoveTowardDefault(leftHandRect, leftHandDefaultPosition);
            MoveTowardDefault(rightHandRect, rightHandDefaultPosition);
        }

        private void SetHandOffset(RectTransform hand, Vector2 defaultPosition, float x, float y)
        {
            if (hand == null) return;
            hand.anchoredPosition = defaultPosition + new Vector2(x, y);
        }

        private void MoveTowardDefault(RectTransform hand, Vector2 defaultPosition)
        {
            if (hand == null) return;
            hand.anchoredPosition = Vector2.Lerp(hand.anchoredPosition, defaultPosition, Time.deltaTime * returnToDefaultSpeed);
        }
    }
}
