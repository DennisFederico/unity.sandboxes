using Drawing;
using narkdagas.inputcontrol;
using UnityEngine;
using UnityEngine.InputSystem;

namespace narkdagas.gameobjects.controllers {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour {
        [Header("Input and State - Internals")]
        [SerializeField] private float forwardMoveInput;
        [SerializeField] private float sideMoveInput;
        [SerializeField] private bool useLastForwardDirection;
        [SerializeField] private Vector3 lastForwardDirection;
        [SerializeField] private bool jumpPressed;
        [SerializeField] private Vector3 currentMoveDirection;
        [SerializeField] private bool isMoving;
        [SerializeField] private float verticalVelocity;

        [Header("Movement Configuration")]
        [SerializeField] private float walkSpeed = 5f;

        //[SerializeField] private float sprintSpeed = 5f;
        [SerializeField] private float turnSpeed = 1f;
        [SerializeField] private float rotationAlignThreshold = 10f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.8f;

        [Header("Aim Line")]
        [SerializeField] public GameObject aimLinePrefab;

        [SerializeField] public float aimLineLength = 10f;
        private LineRenderer _aimLine;

        [Header("Internals")]
        private CharacterController _characterController;

        private GameInputControls _gameInputControls;

        private Camera _mainCamera;
        private CommandBuilder _draw;

        private void Awake() {
            _draw = Draw.editor;
            _draw.WithDuration(1f);
            _characterController = GetComponent<CharacterController>();
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
        }

        private void OnDisable() {
            _gameInputControls.GameControls.PlayerMove.started -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.performed -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerMove.canceled -= OnPlayerMove;
            _gameInputControls.GameControls.PlayerJump.started -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.performed -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.PlayerJump.canceled -= OnPlayerJumpStateChanged;
            _gameInputControls.GameControls.Disable();
        }

        public void OnPlayerMove(InputAction.CallbackContext context) {
            currentMoveDirection = context.phase == InputActionPhase.Canceled ? Vector3.zero : context.ReadValue<Vector3>();
            //We store the last forward direction when the player starts moving on Z axis (forward/backward)
            if (forwardMoveInput == 0 && Mathf.Abs(currentMoveDirection.z) > 0.1f) {
                lastForwardDirection = transform.forward;
            } 
            forwardMoveInput = currentMoveDirection.z;
            sideMoveInput = currentMoveDirection.x;
            isMoving = currentMoveDirection.sqrMagnitude > 0.01f;
        }

        private void OnPlayerJumpStateChanged(InputAction.CallbackContext context) {
            jumpPressed = context.phase switch {
                InputActionPhase.Started => true,
                InputActionPhase.Performed => false,
                InputActionPhase.Canceled => false,
                _ => jumpPressed
            };
        }

        private void Start() {
            GameObject lineObj = Instantiate(aimLinePrefab, transform.position, Quaternion.identity);
            _aimLine = lineObj.GetComponent<LineRenderer>();
            _aimLine.positionCount = 2;
            _aimLine.useWorldSpace = true;
            _aimLine.enabled = true;
        }

        private void Update() {
            Move();
        }

        private void Move() {
            GroundedMove();
            Turn();
            UpdateAimLine();
        }

        private void GroundedMove() {
            if (!isMoving) return;
            //Lateral movement (left/right) relative to the transform.right
            //Forward/backward movement relative to the transform.forward
            Vector3 forward = useLastForwardDirection ? lastForwardDirection : transform.forward;
            currentMoveDirection = (forward * forwardMoveInput + transform.right * sideMoveInput).normalized;
            verticalVelocity = VerticalForceCalculation();
            //Vector3.ClampMagnitude is used to prevent diagonal movement from being faster
            Vector3 moveDirection = Vector3.ClampMagnitude(currentMoveDirection, 1f);
            _characterController.Move((moveDirection * walkSpeed + verticalVelocity * Vector3.up) * Time.deltaTime);
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
            //USEFUL TIP TO RAYCAST OVER A PLANE
            //to prevent accidental ray hits on walls, enemies, or slopes
            // Plane aimPlane = new Plane(Vector3.up, transform.position);
            // Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // if (aimPlane.Raycast(ray, out float enter)) {
            //     Vector3 hitPoint = ray.GetPoint(enter);
            //     ...
            // }
            
            if (TryGetRaycastHit(out var hitInfo)) {
                Vector3 direction = hitInfo.point - transform.position;
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

        private bool TryGetRaycastHit(out RaycastHit hitInfo) {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hitInfo);
        }

        //TODO Refactor with the TURN method to re-use the raycast
        private void UpdateAimLine() {
            //Move start point to a weapon muzzle child object for realism
            //Use a layer mask in Physics.Raycast to ignore enemies or props
            //Add a glow or pulse effect on the line’s material
            //Use the direction * maxLength unless hitInfo.distance < maxLength (to clip at obstacles)

            //Would you like to make the line turn red when aimed at an enemy or dynamically bend it based on projectile arcs later on?
            Vector3 start = transform.position + Vector3.up * 0.5f; // Slightly above the ground
            Vector3 end = start + transform.forward * aimLineLength; // Default direction
            // Optional: aim towards the mouse cursor

            if (TryGetRaycastHit(out var hitInfo)) {
                Vector3 direction = (hitInfo.point - start).normalized;
                end = start + direction * aimLineLength;
            }

            _aimLine.SetPosition(0, start);
            _aimLine.SetPosition(1, end);
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