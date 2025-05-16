using Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace narkdagas.gameobjects.controllers {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController3 : MonoBehaviour {
        [Header("Input and State - Internals")]
        [SerializeField] private float forwardMoveInput;

        [SerializeField] private float sideMoveInput;
        [SerializeField] private bool forwardPressed;
        [SerializeField] private bool forwardReleased;
        [SerializeField] private bool forwardHeld;
        [SerializeField] private bool jumpPressed;
        [SerializeField] private Vector3 currentMoveDirection;
        [SerializeField] private Vector3 lastForwardDirection;
        [SerializeField] private bool isMoving;
        [SerializeField] private bool isJumping;

        [SerializeField] private float verticalVelocity;

        [Header("Movement Configuration")]
        [SerializeField] private float walkSpeed = 5f;

        [SerializeField] private float turnSpeed = 1f;
        [SerializeField] private float rotationAlignThreshold = 10f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.8f;

        [Header("Aim Line")]
        [SerializeField] public LineRenderer aimLine;

        [SerializeField] public float aimLineLength = 10f;

        [Header("Internals")]
        private CharacterController _characterController;

        private Camera _mainCamera;
        private CommandBuilder _draw;


        private void Awake() {
            _draw = Draw.editor;
            _draw.WithDuration(1f);
            _characterController = GetComponent<CharacterController>();
            _mainCamera ??= Camera.main;
        }

        private void Update() {
            _draw.Line(transform.position, transform.position + transform.forward * 20, Color.blue);
            CaptureInput();
            Move();
        }

        //TODO - Switch to new input system
        private void CaptureInput() {
            forwardMoveInput = Input.GetAxis("Vertical");
            sideMoveInput = Input.GetAxis("Horizontal");
            //currentMoveDirection = new Vector3(sideMoveInput, 0, forwardMoveInput);

            forwardPressed = Input.GetKeyDown("w") || Input.GetKeyDown("s");
            forwardReleased = Input.GetKeyUp("w") || Input.GetKeyUp("s");

            if (forwardPressed) {
                lastForwardDirection = transform.forward;
                forwardHeld = true;
            } else if (forwardReleased) {
                forwardHeld = false;
            }
            currentMoveDirection = ((forwardHeld ? lastForwardDirection : transform.forward) * forwardMoveInput + transform.right * sideMoveInput).normalized;

            Debug.Log($"{currentMoveDirection.sqrMagnitude}");
            isMoving = currentMoveDirection.sqrMagnitude > 0.01f;
            // if (isMoving) {
            //     lastMoveDirection = (transform.forward * forwardMoveInput + transform.right * sideMoveInput).normalized;
            // }

            jumpPressed = Input.GetButtonDown("Jump");
            isJumping = !_characterController.isGrounded || (jumpPressed && _characterController.isGrounded);
        }

        private void Move() {
            GroundedMove();
            Turn();
            //UpdateAimLine();
        }

        private void GroundedMove() {
            verticalVelocity = VerticalForceCalculation();
            //Lateral movement (left/right) relative to the transform.right (Vertical Input?)
            //Forward/backward movement relative to the transform.forward (Horizontal Input?)
            //Vector3 moveDirection = (transform.forward * currentMoveDirection.z + transform.right * currentMoveDirection.x).normalized;
            //Using the last move direction, this is reset when the keys are released
            //Vector3.ClampMagnitude is used to prevent diagonal movement from being faster
            Vector3 moveDirection = Vector3.ClampMagnitude(isMoving ? currentMoveDirection : Vector3.zero, 1f);

            _characterController.Move(((moveDirection * walkSpeed) + (verticalVelocity * Vector3.up)) * Time.deltaTime);
        }

        private float VerticalForceCalculation() {
            //We hit the ground
            if (_characterController.isGrounded && verticalVelocity < 0) {
                verticalVelocity = 0f;
            }

            //Jump
            if (jumpPressed && _characterController.isGrounded) {
                //Add jump force
                verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight); // Jump height of 1.5 units
            }

            //Apply gravity
            verticalVelocity += gravity * Time.deltaTime;

            return verticalVelocity;
        }

        private void Turn() {
            //Turn should only account on forward/backward movement in relation to the camera forward direction
            // Turning logic: only if there's forward/back input
            // THIS ALIGNS TO THE MOUSE POSITION
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            //USEFUL TIP TO RAYCAST OVER A PLANE
            //to prevent accidental ray hits on walls, enemies, or slopes
            // Plane aimPlane = new Plane(Vector3.up, transform.position);
            // Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // if (aimPlane.Raycast(ray, out float enter)) {
            //     Vector3 hitPoint = ray.GetPoint(enter);
            //     ...
            // }


            if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                Vector3 direction = hitInfo.point - transform.position;
                direction.y = 0f;

                //TRY THIS TO AVOID SPINNING WITH THE ROTATING CAMERA
                //limit hitInfo.point to a forward-facing cone using Vector3.Dot(transform.forward, direction) > 0

                _draw.Line(transform.position, transform.position + direction * 20, Color.goldenRod);
                _draw.Line(transform.position, hitInfo.point, Color.green);

                if (direction.sqrMagnitude > 1f) {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);
                    if (angleDiff > rotationAlignThreshold) {
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                    }
                }
            }
        }

        //TODO Refactor with the TURN method to re-use the raycast
        private void UpdateAimLine() {
            //Move start point to a weapon muzzle child object for realism
            //Use a layer mask in Physics.Raycast to ignore enemies or props
            //Add a glow or pulse effect on the line’s material
            //Use the direction * maxLength unless hitInfo.distance < maxLength (to clip at obstacles)

            //Would you like to make the line turn red when aimed at an enemy or dynamically bend it based on projectile arcs later on?
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                Vector3 direction = hitInfo.point - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.1f) {
                    direction.Normalize();
                    Vector3 start = transform.position + Vector3.up * 0.5f; // Slightly above ground
                    //Vector3 end = start + direction.normalized * aimLineLength;
                    //Clip on ray hit
                    Vector3 end = hitInfo.point;
                    if ((end - start).magnitude > aimLineLength)
                        end = start + direction * aimLineLength;

                    aimLine.SetPosition(0, start);
                    aimLine.SetPosition(1, end);
                    aimLine.enabled = true;
                }
            } else {
                aimLine.enabled = false;
            }
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