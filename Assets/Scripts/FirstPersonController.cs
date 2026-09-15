namespace HorrorRPG.Player
{
using HorrorRPG.Input;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    private const float GroundingVelocity = -2f;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;
    [Header("UI")]
    [SerializeField] private HandAnimationManager handAnimationManager;
    [SerializeField] private GameInputReader inputReader;
    private CharacterController characterController;
    private float verticalVelocity;
    private bool controlEnabled = true;
    private bool isWalking;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (inputReader == null) inputReader = FindFirstObjectByType<GameInputReader>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (cameraTransform == null) Debug.LogError($"{nameof(FirstPersonController)} requires a camera on {name}.", this);
    }

    private void Update()
    {
        if (!controlEnabled || inputReader == null) { StopWalking(); return; }
        Vector2 moveInput = inputReader.Move;
        Vector2 lookInput = inputReader.Look;
        HandleMovement(moveInput);
        HandleRotation(lookInput);
    }

    private void HandleMovement(Vector2 input)
    {
        Vector3 moveDirection = Vector3.ClampMagnitude(transform.right * input.x + transform.forward * input.y, 1f);
        Vector3 movement = moveDirection * moveSpeed;
        if (characterController.isGrounded && verticalVelocity < 0f) verticalVelocity = GroundingVelocity; else verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
        UpdateWalkingState(input.sqrMagnitude > 0.0001f);
    }

    private void HandleRotation(Vector2 input) { transform.Rotate(Vector3.up * (input.x * lookSensitivity)); }
    private void UpdateWalkingState(bool moving)
    {
        if (handAnimationManager == null || moving == isWalking) return;
        isWalking = moving;
        if (isWalking) handAnimationManager.StartWalking(); else handAnimationManager.StopWalking();
    }
    private void StopWalking() { if (!isWalking) return; isWalking = false; handAnimationManager?.StopWalking(); }
    public void Initialize(GameInputReader reader) { inputReader = reader; }
    public void SetMovementEnabled(bool enabled) { controlEnabled = enabled; if (!enabled) StopWalking(); }
    public void SetControlEnabled(bool enabled) => SetMovementEnabled(enabled);
}
}
