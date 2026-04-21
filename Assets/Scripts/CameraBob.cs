using UnityEngine;

//this script creates a head bobbing effect for the player by adjusting the camera's Y position

public class CameraBob : MonoBehaviour
{
    [Header("Bob Settings")]
    //bobbing speed whiole walking
    public float walkBobSpeed = 14f;
    //vertical movement while walking
    public float walkBobAmount = 0.05f;
    // speed of the bobbing motion while sprinting
    public float sprintBobSpeed = 18f;

    // vertical movement amount while sprinting
    public float sprintBobAmount = 0.08f;

    [Header("References")]

    //Reference to PlayerMJovement script to check movement input
    public PlayerMovement playerMovement;
    
    private float defaultYPos; //stores original Y position 
    private float timer; //Tracks time progression for the sine wave for smooth motion
    
    private void Start()
    {
        defaultYPos = transform.localPosition.y; //stores camera's starting Y position
        
        if (playerMovement == null) //if no PlayerMovement reference is assigned, auto try to find it in parent ojbect
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
    }
    
    private void Update()
    {
        //if reference missing, do nothing
        if (playerMovement == null) return;
        
        // check if player is moving and grounded
        bool isMoving = playerMovement.GetMoveInput().magnitude > 0.1f;
        bool isGrounded = playerMovement.IsGrounded();
        bool isSprinting = playerMovement.IsSprinting();
        
        if (isMoving && isGrounded)
        {
            // etermine bob parameters based on sprint state
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;
            
            // Apply head bob
            timer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(timer) * bobAmount;
            
            Vector3 newPosition = transform.localPosition;
            newPosition.y = defaultYPos + bobOffset;
            transform.localPosition = newPosition;
        }
        else
        {
            // Smoothly return to default position when not moving
            timer = 0f;
            Vector3 newPosition = transform.localPosition;
            newPosition.y = Mathf.Lerp(newPosition.y, defaultYPos, Time.deltaTime * 10f);
            transform.localPosition = newPosition;
        }
    }
}
