using narkdagas.inputcontrol;
using UnityEngine;
using UnityEngine.InputSystem;

namespace narkdagas.gameobjects.controllers {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour {
        //TODO... Implement Sprint duration and recovery (fatigue)
        
        [Header("Movement Configuration")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float sprintAcceleration = 1f;
        [SerializeField] private float turnSpeed = 1f;
        [SerializeField] private float rotationAlignThreshold = 10f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.8f;
        
        [Header("Aim Line")]
        [SerializeField] public GameObject aimLinePrefab;
        [SerializeField] public float aimLineLength = 10f;

        [Header("Animation")]
        private int _animatorMoveSpeed;
        
        [Header("Input and State - Internals")]
        [SerializeField] private float forwardMoveInput;
        [SerializeField] private float sideMoveInput;
        [SerializeField] private float moveSpeed;
        [SerializeField] private bool useLastForwardDirection;
        [SerializeField] private Vector3 lastForwardDirection;
        [SerializeField] private bool sprinting;
        [SerializeField] private bool jumpPressed;
        [SerializeField] private Vector3 currentMoveDirection;
        [SerializeField] private Vector2 currentMousePosition;
        [SerializeField] private Vector2 joystickDelta;
        [SerializeField] private bool isMoving;
        [SerializeField] private float verticalVelocity;
        
        [Header("Internals")]
        private CharacterController _characterController;
        private GameInputControls _gameInputControls;
        private Animator _animator;
        private Camera _mainCamera;
        private LineRenderer _aimLine;
        // private CommandBuilder _draw;

        private void Awake() {
            // _draw = Draw.editor;
            // _draw.WithDuration(1f);
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _mainCamera ??= Camera.main;
            _gameInputControls = new GameInputControls();
        }

        private void OnEnable() {
            _gameInputControls.GameControls.Enable();
            _gameInputControls.GameControls.PlayerMove.started += OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.performed += OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.canceled += OnPlayerMove;
            _gameInputControls.GameControls.PlayerJump.started += OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.performed += OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.canceled += OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.AimMouse.performed += OnAimMouse;
            _gameInputControls.GameControls.AimJoystick.started += OnAimJoystick;
            _gameInputControls.GameControls.AimJoystick.performed += OnAimJoystick;
            _gameInputControls.GameControls.AimJoystick.canceled += OnAimJoystick;
            _gameInputControls.GameControls.Sprint.performed += OnSprint;
            _gameInputControls.GameControls.Sprint.canceled += OnSprint;
        }

        private void OnDisable() {
            _gameInputControls.GameControls.PlayerMove.started -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.performed -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.canceled -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerJump.started -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.performed -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.canceled -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.AimMouse.performed -= OnAimMouse;
            _gameInputControls.GameControls.AimJoystick.started -= OnAimJoystick;
            _gameInputControls.GameControls.AimJoystick.performed -= OnAimJoystick;
            _gameInputControls.GameControls.AimJoystick.canceled -= OnAimJoystick;
            _gameInputControls.GameControls.Sprint.performed += OnSprint;
            _gameInputControls.GameControls.Sprint.canceled += OnSprint;
            _gameInputControls.GameControls.Disable();
        }

        public void OnPlayerMove(InputAction.CallbackContext context) {
            var moveDirection = context.phase == InputActionPhase.Canceled ? Vector2.zero : context.ReadValue<Vector2>();
            //We store the last forward direction when the player starts moving on Z axis (forward/backward)
            //Do this before assigning forwardMoveInput
            if (forwardMoveInput == 0 && Mathf.Abs(moveDirection.y) > 0.1f) {
                lastForwardDirection = transform.forward;
            }
            isMoving = moveDirection.sqrMagnitude > 0.01f;
            forwardMoveInput = moveDirection.y;
            sideMoveInput = moveDirection.x;
        }

        public void OnAimMouse(InputAction.CallbackContext context) {
            currentMousePosition = context.phase == InputActionPhase.Canceled ? Vector2.zero : context.ReadValue<Vector2>();
            
        }

        public void OnAimJoystick(InputAction.CallbackContext context) {
            joystickDelta = context.phase == InputActionPhase.Canceled ? Vector2.zero : context.ReadValue<Vector2>();
        }
        
        private void OnPlayerJumpStateChanged(InputAction.CallbackContext context) {
            jumpPressed = context.phase switch {
                InputActionPhase.Started => true,
                InputActionPhase.Performed => false,
                InputActionPhase.Canceled => false,
                _ => jumpPressed
            };
        }
        
        private void OnSprint(InputAction.CallbackContext context) {
            sprinting = context.phase switch {
                InputActionPhase.Performed => true,
                InputActionPhase.Canceled => false,
                _ => sprinting
            };
        }

        private void Start() {
            SetupAnimationIds();
            GameObject lineObj = Instantiate(aimLinePrefab, transform.position, Quaternion.identity);
            _aimLine = lineObj.GetComponent<LineRenderer>();
            _aimLine.positionCount = 2;
            _aimLine.useWorldSpace = true;
            _aimLine.enabled = true;
        }

        private void SetupAnimationIds() {
            _animatorMoveSpeed = Animator.StringToHash("MoveSpeed");
        }

        private void Update() {
            Move();
        }

        private void Move() {
            UpdateSpeed();
            GroundedMove();
            Turn();
            SyncAnimation();
            UpdateAimLine();
        }
        
        private void UpdateSpeed() {
            moveSpeed = Mathf.Lerp(moveSpeed, sprinting ? sprintSpeed : walkSpeed, sprintAcceleration * Time.deltaTime);
        }

        private void GroundedMove() {
            if (!isMoving) return;
            //Lateral movement (left/right) relative to the transform.right
            //Forward/backward movement relative to the transform.forward
            var forward = useLastForwardDirection ? lastForwardDirection : transform.forward;
            currentMoveDirection = (forward * forwardMoveInput + transform.right * sideMoveInput).normalized;
            verticalVelocity = VerticalForceCalculation();
            //Vector3.ClampMagnitude is used to prevent diagonal movement from being faster
            var moveDirection = Vector3.ClampMagnitude(currentMoveDirection, 1f);
            _characterController.Move((moveDirection * moveSpeed + verticalVelocity * Vector3.up) * Time.deltaTime);
        }

        private float VerticalForceCalculation() {
            //We hit the ground
            if (_characterController.isGrounded) {
                if (jumpPressed) {
                    //Add jump force
                    verticalVelocity = Mathf.Sqrt(2f * -gravity * jumpHeight); // Jump height of 1.5 units
                } else {
                    verticalVelocity = -1f;
                }
            } else {
                verticalVelocity += gravity * Time.deltaTime;
            }

            return verticalVelocity;
        }

        private void Turn() {
            
            //TODO - Handle joystickDelta
            // if (TryGetRaycastHit(out var hitInfo, Mouse.current.position.ReadValue())) {
            // if (TryGetRaycastHit(out var hitInfo, currentMousePosition)) {
            //     Vector3 direction = hitInfo.point - transform.position;
            //     direction.y = 0f;
            if (TryGetRaycastPlaneHit(out var hitPosition, currentMousePosition, Vector3.up,  transform.position + Vector3.up * 0.5f)) {
                Vector3 direction = hitPosition - transform.position;
                direction.y = 0f;
                //TRY THIS TO AVOID SPINNING WITH THE ROTATING CAMERA
                //limit hitInfo.point to a forward-facing cone using Vector3.Dot(transform.forward, direction) > 0

                if (direction.sqrMagnitude > 1f) {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);
                    if (angleDiff > rotationAlignThreshold) {
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                    }
                }
            }
        }

        private void SyncAnimation() {
            //TODO - Refactor .. this only accounts for "positive value" (Abs) and also when side move
            //Need negative values for backward movement
            _animator.SetFloat(_animatorMoveSpeed, moveSpeed * Mathf.Max(Mathf.Abs(forwardMoveInput), Mathf.Abs(sideMoveInput)));
        }
        
        //TODO Refactor with the TURN method to re-use the raycast
        private void UpdateAimLine() {
            //TODOS: ---
            //Move start point to a weapon muzzle child object for realism
            //Add a glow or pulse effect on the line’s material
            //Use the direction * maxLength unless hitInfo.distance < maxLength (to clip at obstacles)
            //make the line turn red when aimed at an enemy or dynamically bend it based on projectile arcs later on?
            
            Vector3 start = transform.position + Vector3.up * 0.5f; // Slightly above the ground
            Vector3 end = start + transform.forward * aimLineLength; // Default direction
            
            // if (TryGetRaycastHit(out var hitInfo, currentMousePosition)) {
            //     Vector3 direction = (hitInfo.point - start).normalized;
            //     end = start + direction * aimLineLength;
            // }
            
            if (TryGetRaycastPlaneHit(out var hitPosition, currentMousePosition, Vector3.up, transform.position + Vector3.up * 0.5f)) {
                Vector3 direction = (hitPosition - start).normalized; 
                end = start + direction * aimLineLength;
            }

            _aimLine.SetPosition(0, start);
            _aimLine.SetPosition(1, end);
        }
        
        private bool TryGetRaycastHit(out RaycastHit hitInfo, Vector2 aimPosition) {
            // Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Ray ray = _mainCamera.ScreenPointToRay(aimPosition);
            return Physics.Raycast(ray, out hitInfo);
        }
        
        private bool TryGetRaycastPlaneHit(out Vector3 hitPosition, Vector2 aimPosition, Vector3 normal, Vector3 pointInPlane) {
            var aimPlane = new Plane(normal, pointInPlane);
            var ray = _mainCamera.ScreenPointToRay(aimPosition);
            if (aimPlane.Raycast(ray, out float hitDistance)) {
                hitPosition = ray.GetPoint(hitDistance);
                return true;
            }
            hitPosition = Vector3.zero;
            return false;
        }
    }
}


// THIS ALIGNS TO THE CAMERA FORWARD
// Vector3 debugCamForward = cameraTransform.forward;
// debugCamForward.y = 0f;
// debugCamForward.Normalize();
// draw.Line(transform.position, transform.position + debugCamForward * 20, Color.goldenRod);

// if (Mathf.Abs(moveVector.z) > 0.1f) {
//     Debug.Log("Turning");
//     Vector3 camForward = cameraTransform.forward;
//     camForward.y = 0f;
//     camForward.Normalize();
//     // transform.LookAt(transform.position + camForward, Vector3.up);
//     
//     // Calculate the target rotation
//     Quaternion targetRotation = Quaternion.LookRotation(camForward);
//
//     // Check angle difference before rotating
//     float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);
//     if (angleDiff > rotationAlignThreshold) // Threshold in degrees
//     {
//         // float degreesPerSecond = 360f;
//         // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, degreesPerSecond * Time.deltaTime);
//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             targetRotation,
//             turnSpeed * Time.deltaTime
//         );
//     } else {
//         // If the angle difference is small, snap to the target rotation
//         //transform.rotation = targetRotation;
//     }