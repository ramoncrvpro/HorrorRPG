namespace HorrorRPG.Player
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public class HandAnimationManager : MonoBehaviour
{
    public static HandAnimationManager Instance { get; private set; }

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        if (isWalking)
        {
            AnimateWalk();
        }
        else
        {
            ReturnToDefaultPosition();
        }
    }

    public void StartWalking()
    {
        isWalking = true;
        walkTimer = 0f;
    }

    public void StopWalking()
    {
        isWalking = false;
    }

    public void PlayReachAnimation()
    {
        if (reachAnimation == null)
        {
            Debug.LogWarning("HandAnimationManager: ReachAnimation not assigned!");
            return;
        }

        if (leftHandAnimator != null)
            leftHandAnimator.Play(reachAnimation);

        if (rightHandAnimator != null)
            rightHandAnimator.Play(reachAnimation);
    }

    public void PlayReachAnimationLeftHand()
    {
        if (reachAnimation == null)
        {
            Debug.LogWarning("HandAnimationManager: ReachAnimation not assigned!");
            return;
        }

        if (leftHandAnimator != null)
            leftHandAnimator.Play(reachAnimation);
    }

    public void PlayReachAnimationRightHand()
    {
        if (reachAnimation == null)
        {
            Debug.LogWarning("HandAnimationManager: ReachAnimation not assigned!");
            return;
        }

        if (rightHandAnimator != null)
            rightHandAnimator.Play(reachAnimation);
    }

    private void AnimateWalk()
    {
        walkTimer += Time.deltaTime * swingSpeed;

        float swingX = Mathf.Sin(walkTimer) * swingAmplitudeX;
        float swingY = Mathf.Abs(Mathf.Sin(walkTimer)) * swingAmplitudeY;

        if (leftHandRect != null)
        {
            Vector2 anchoredPos = leftHandRect.anchoredPosition;
            anchoredPos.x = leftHandDefaultPosition.x + swingX;
            anchoredPos.y = leftHandDefaultPosition.y + swingY;
            leftHandRect.anchoredPosition = anchoredPos;
        }

        if (rightHandRect != null)
        {
            Vector2 anchoredPos = rightHandRect.anchoredPosition;
            anchoredPos.x = rightHandDefaultPosition.x + swingX;
            anchoredPos.y = rightHandDefaultPosition.y + swingY;
            rightHandRect.anchoredPosition = anchoredPos;
        }
    }

    private void ReturnToDefaultPosition()
    {
        bool leftReturned = MoveTowardDefault(leftHandRect, leftHandDefaultPosition);
        bool rightReturned = MoveTowardDefault(rightHandRect, rightHandDefaultPosition);
    }

    private bool MoveTowardDefault(RectTransform rectTransform, Vector2 defaultPos)
    {
        if (rectTransform == null)
            return true;

        Vector2 anchoredPos = rectTransform.anchoredPosition;

        if (Vector2.Distance(anchoredPos, defaultPos) < 0.01f)
        {
            rectTransform.anchoredPosition = defaultPos;
            return true;
        }

        anchoredPos = Vector2.Lerp(anchoredPos, defaultPos, Time.deltaTime * returnToDefaultSpeed);
        rectTransform.anchoredPosition = anchoredPos;

        return false;
    }
}


}
