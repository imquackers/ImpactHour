using UnityEngine;

// Makes sure the object always has a CharacterController component
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // inspector settings
    [Header("Movement Settings")]
    public float walkSpeed = 5f;    
    public float sprintSpeed = 8f;  
    public float gravity = -9.81f;   
    public float jumpHeight = 2f;

    [Header("Jump Feel")]
    public float riseGravityMultiplier = 0.4f;  // Gravity scale while moving upward (< 1 = floaty)
    public float fallGravityMultiplier = 4f;     // Gravity scale while falling (> 1 = snappy)

    [Header("Ground Check")]
    public Transform groundCheck;    // Position used to check if on ground
    public float groundDistance = 0.4f; // Size of ground check sphere
    public LayerMask groundMask;     // What counts as ground

    // internal variables
    private CharacterController characterController; // Handles movement/collisions
    private Vector3 velocity;     // Stores vertical movement 
    private bool isGrounded;      // Is the player touching the ground?
    private Vector2 moveInput;    // Stores input (WASD)
    private bool isSprinting;     // Is player holding sprint?

    private void Awake()
    {
        // Get the CharacterController attached to this object
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GroundCheck();   // Check if player is on the ground
        Move();          // Handle movement input
        Jump();          // Handle jumping
        ApplyGravity();  // Apply gravity every frame
    }

    private void GroundCheck()
    {
        // Check if a small sphere at groundCheck is touching the ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // If grounded and falling, reset downward velocity slightly
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps player stuck to ground
        }
    }

    private void Move()
    {
        // Get WASD / arrow key input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Store input (used for other systems like camera bob)
        moveInput = new Vector2(horizontal, vertical);

        // Check if sprint key is held
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Choose speed based on sprinting
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Calculate movement direction relative to player rotation
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Move the player
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        // If jump button pressed AND player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Apply upward force using physics formula
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        // Use a reduced gravity multiplier while the player is rising (floaty ascent)
        // and a stronger multiplier while falling (snappy descent)
        float gravityMultiplier = velocity.y > 0f ? riseGravityMultiplier : fallGravityMultiplier;

        velocity.y += gravity * gravityMultiplier * Time.deltaTime;

        // Move player vertically
        characterController.Move(velocity * Time.deltaTime);
    }

    // === GETTERS (used by other scripts like camera bob) ===

    public Vector2 GetMoveInput()
    {
        return moveInput; // Returns movement input
    }

    public bool IsGrounded()
    {
        return isGrounded; // Returns if player is on ground
    }

    public bool IsSprinting()
    {
        return isSprinting; // Returns if player is sprinting
    }
}