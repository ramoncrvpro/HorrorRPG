using HorrorRPG.Core;
using HorrorRPG.Input;
using UnityEngine;

namespace HorrorRPG.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour, IGameContextReceiver
    {
        private const float GroundingVelocity = -2f;
        private const float MovementInputThreshold = 0.0001f;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;
        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private Transform cameraTransform;
        [Header("UI")]
        [SerializeField] private HandAnimationManager handAnimationManager;

        private CharacterController characterController;
        private GameInputReader inputReader;
        private float verticalVelocity;
        private bool movementEnabled = true;
        private bool isWalking;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError($"{nameof(FirstPersonController)} requires a camera on {name}.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!movementEnabled || inputReader == null)
            {
                StopWalking();
                return;
            }
            HandleMovement(inputReader.Move);
            HandleRotation(inputReader.Look);
        }

        /// <summary>Initializes movement with the shared input reader.</summary>
        public void Initialize(GameInputReader reader)
        {
            inputReader = reader != null ? reader : throw new System.ArgumentNullException(nameof(reader));
        }

        void IGameContextReceiver.Initialize(GameContext context) => Initialize(context.InputReader);

        /// <summary>Releases the input reader when the scene is deinitialized.</summary>
        public void Deinitialize()
        {
            inputReader = null;
            StopWalking();
        }

        /// <summary>Enables or disables movement and walking presentation.</summary>
        public void SetMovementEnabled(bool isEnabled)
        {
            movementEnabled = isEnabled;
            if (!isEnabled) StopWalking();
        }

        /// <summary>Compatibility alias for movement enablement.</summary>
        public void SetControlEnabled(bool isEnabled) => SetMovementEnabled(isEnabled);

        private void HandleMovement(Vector2 input)
        {
            Vector3 direction = Vector3.ClampMagnitude(transform.right * input.x + transform.forward * input.y, 1f);
            Vector3 movement = direction * moveSpeed;
            verticalVelocity = characterController.isGrounded && verticalVelocity < 0f
                ? GroundingVelocity
                : verticalVelocity + gravity * Time.deltaTime;
            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
            UpdateWalkingState(input.sqrMagnitude > MovementInputThreshold);
        }

        private void HandleRotation(Vector2 input) => transform.Rotate(Vector3.up * (input.x * lookSensitivity));

        private void UpdateWalkingState(bool moving)
        {
            if (handAnimationManager == null || moving == isWalking) return;
            isWalking = moving;
            if (isWalking) handAnimationManager.StartWalking();
            else handAnimationManager.StopWalking();
        }

        private void StopWalking()
        {
            if (!isWalking) return;
            isWalking = false;
            handAnimationManager?.StopWalking();
        }
    }
}
