using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Keybinds")] 
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode menuKey = KeyCode.Escape;

    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;

    public Transform orientation;

    [Header("Crouching")] 
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    
    
    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    [Header("Slope Handling")] 
    public float maxSlopeAngel;
    public RaycastHit slopeHit;
    private bool exitingSlope;
    
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //sucht nach dem boden
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //bearbeitet den drag am boden
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // sendet den spieler wieder ins Main Menu
        if (Input.GetKeyDown(menuKey))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu#");
            return;
        }
        
        //wann darf man springen
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            
            Jump();
            
            Invoke(nameof(ResetJump),jumpCooldown);
        }
        
        //crouchen
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        //stop crouch
        if(Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }
    }

    private void StateHandler()
    {
        //Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        
        //Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Mode - Air
        else
        {
            state = MovementState.air;
        }
        
        //Mode - Crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
    }

    private void MovePlayer()
    {
        //berechnet die bewegenungs richtung
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //auf einer schregen
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        
        //auf dem boden
        if(grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        //in der luft
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
      
    }

    private void SpeedControl()
    {
        //limitiert die geschwindigkeit auf schrägen
        if(OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                return;
        } 
        
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        //setzt die y velocity zurück
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngel && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    
}
