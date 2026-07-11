using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Header("Interaction Settings")]
        public float InteractionRange = 5f;
        public LayerMask InteractionLayer;
        private IInteractable currentInteractable;

        public bool isInteractable = false;
        public Image Crosshair;
        public Image InteractableCrosshair;

        [SerializeField] private Transform objectGrabPointTransform;
        private InteractableOutline currentInteractableOutline;
        private ObjectGrabbable currentObject;
        private ObjectGrabbable targetObject;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine & Clamping")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        [Tooltip("How restricted the up/down look angle is when holding a heavy object like the mirror")]
        public float HeldObjectTopClamp = 30.0f;
        [Tooltip("How restricted the up/down look angle is when holding a heavy object like the mirror")]
        public float HeldObjectBottomClamp = -30.0f;

        private float _cinemachineTargetPitch;
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();

            DetectInteractable();
            UpdateCrosshair();
            Interact();
        }

        private void LateUpdate()
        {
            if(PauseMenuUI.IsPaused) return;
            CameraRotation();
        }

        public void DetectInteractable()
        {
            if (currentObject != null) return;

            ClearTargetObject();

            if (currentInteractableOutline != null)
            {
                currentInteractableOutline.SetOutline(false);
                currentInteractableOutline = null;
            }

            currentInteractable = null;
            isInteractable = false;

            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * InteractionRange);

            if (Physics.Raycast(ray, out RaycastHit hit, InteractionRange, InteractionLayer))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    currentInteractable = interactable;
                    isInteractable = true;

                    if (hit.collider.TryGetComponent<InteractableOutline>(out var interactableOutline))
                    {
                        currentInteractableOutline = interactableOutline;
                        currentInteractableOutline.SetOutline(true);
                    }
                }

                ObjectGrabbable grabbable = hit.collider.GetComponentInParent<ObjectGrabbable>();
                if (grabbable != null)
                {
                    SetTargetObject(grabbable);
                    isInteractable = true;
                }
            }
            else
            {
                isInteractable = false;
            }
        }

        public void Interact()
        {
            if (_input.interact)
            {
                _input.interact = false;

                if (currentObject != null)
                {
                    DropObject();
                    return;
                }

                if (currentInteractable != null)
                {
                    if (currentInteractableOutline != null)
                    {
                        currentInteractableOutline.SetOutline(false);
                    }
                    currentInteractable.Interact();
                }

                if (targetObject != null && currentObject == null)
                {
                    TryGrabObject();
                }
            }
        }

        public void TryGrabObject()
        {
            if (targetObject == null) return;

            currentObject = targetObject;
            currentObject.SetOutline(false);
            currentObject.Grab(objectGrabPointTransform);

            targetObject = null;
        }

        private void SetTargetObject(ObjectGrabbable newTarget)
        {
            if (targetObject == newTarget) return;

            targetObject = newTarget;
            targetObject.SetOutline(true);
        }

        private void ClearTargetObject()
        {
            if (targetObject == null) return;
            targetObject.SetOutline(false);
            targetObject = null;
        }

        private void DropObject()
        {
            if (currentObject == null) return;
            currentObject.SetOutline(false);
            currentObject.Drop();
            currentObject = null;
        }

        public void UpdateCrosshair()
        {
            if (Crosshair != null && InteractableCrosshair != null)
            {
                if (isInteractable)
                {
                    Crosshair.gameObject.SetActive(false);
                    InteractableCrosshair.gameObject.SetActive(true);
                }
                else
                {
                    Crosshair.gameObject.SetActive(true);
                    InteractableCrosshair.gameObject.SetActive(false);
                }
            }
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            // Read the current hold state of the input system directly to bypass sticking toggle bugs
            bool isGrabInputHeld = false;
#if ENABLE_INPUT_SYSTEM
            if (_playerInput != null)
            {
                // Finds the action called "Grab" (make sure your Input Actions asset map matches this name)
                var grabAction = _playerInput.actions.FindAction("Grab");
                if (grabAction != null)
                {
                    isGrabInputHeld = grabAction.IsPressed();
                }
            }
#else
			// Legacy Input system fallbacks if needed
			isGrabInputHeld = Input.GetKey(KeyCode.Mouse1); 
#endif

            // Core Interaction Loop: If holding an object AND holding down the grab input
            if (currentObject != null && isGrabInputHeld)
            {
                if (_input.look.sqrMagnitude >= _threshold)
                {
                    float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                    float pivotSensitivity = IsCurrentDeviceMouse ? 0.5f : 50f;

                    // Rotate the mirror instead of the player camera
                    objectGrabPointTransform.Rotate(Vector3.up * _input.look.x * RotationSpeed * pivotSensitivity * deltaTimeMultiplier);
                }
                return; // Intercept: Stop camera execution loop here while actively turning the object
            }

            // Standard Look and Carrying Loop
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // Dynamic Clamping: Strict limits if carrying an object, standard limits if hands are empty
                if (currentObject != null)
                {
                    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, HeldObjectBottomClamp, HeldObjectTopClamp);
                }
                else
                {
                    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                }

                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}