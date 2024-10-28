using UnityEngine;
using System.Collections;

public class BirdMovementScript : MonoBehaviour
{
    #region vars
    public Rigidbody2D rb;
    public SpriteRenderer sprite; 

    public Vector2 lookingDirection = Vector2.right;

    #region run
    public float moveSpeed = 5;
    public Vector2 moveInput;
    public float accelAmount = 10;
    public float decelAmount = 3;
    public float velPower = 0.9f;
    #endregion

    #region jump
    public bool isGrounded;
    public bool isJumping;
    public float timeSinceJump; 
    public float jumpForce = 30;
    public float timeToThreeFourthsJumpHeight = 0.5f; // replace this with an equation

    public float jumpDownVel = -10.0f;
    public float groundCheckDistance = 0.1f;
    int groundLayer; // unused var

    // double jump
    public bool hasDoubleJumped = false; 
    public float doubleJumpForce = 30;

    // wall jump
    public bool isTouchingWall;
    public float wallJumpForceX;
    public float wallJumpForceY;
    public float timeSinceWallJump;
    public float wallJumpBufferX; 

    #endregion

    #region dash
    public bool isDashing; 
    public float dashMultiplier = 3;
    public float oldVelX;
    #endregion
    #endregion

    void Start()
    {
        groundLayer = LayerMask.GetMask("floor"); // unused var
    }

    void Update()
    {
        #region inputs
        if(isDashing)
        {
            return;
        }

        // moving left and right
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        #region jump
        timeSinceJump += Time.deltaTime;
        timeSinceWallJump += Time.deltaTime; 
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            BeginJump();
            timeSinceJump = 0; 
        }
        if(isJumping)
        {
            if (Input.GetKeyUp(KeyCode.Space) && !hasDoubleJumped && rb.linearVelocityY > 0 && timeSinceJump < timeToThreeFourthsJumpHeight)
            {
                ExertDownForce(); 
            }

            StartCoroutine(WallJump());
            StartCoroutine(DoubleJump());
        }
        #endregion

        // dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Dash());
        }
        #endregion

        #region isGrounded check
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, Color.red);
        #endregion

        UpdateDirection(); 
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return; 
        }
        UpdateSpeed();
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.CompareTag("floor"))
        {
            isJumping = false;
            hasDoubleJumped = false;
        }

        if(collision.gameObject.CompareTag("wall"))
        {
            isTouchingWall = true;
        } 
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            isTouchingWall = false; 
        }
    }

    private void UpdateDirection()
    {
        if(moveInput.x != 0)
        {
            lookingDirection = new Vector2(moveInput.x, moveInput.y);
        }

        if(lookingDirection.x == 1)
        {
            sprite.flipX = false;
        } 
        if(moveInput.x == -1)
        {
            sprite.flipX = true;
        }
    }

    private void UpdateSpeed()
    {
        if (timeSinceWallJump < wallJumpBufferX)
        {
            return;
        }

        float targetSpeed = moveInput.x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > .01) ? accelAmount : decelAmount;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);

        rb.AddForce(movement * Vector2.right);
    }

    #region jump
    private void BeginJump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        timeSinceJump = 0; 
    }

    private void ExertDownForce()
    {
        Debug.Log("down force");
        rb.linearVelocityY = jumpDownVel;
        // old code that exerted a down force instead of setting vel directly
        // rb.AddForce(Vector2.up * (-jumpForce / 2), ForceMode2D.Impulse);
    }

    private IEnumerator DoubleJump()
    {
        yield return new WaitForSeconds(0.05f);

        if (Input.GetKeyDown(KeyCode.Space) && !hasDoubleJumped && !isTouchingWall) // TODO: if too close to floor, don't double jump 
        {
            Debug.Log("double jump");

            rb.linearVelocityY = 0;
            rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
            hasDoubleJumped = true;
        }
    }

    private IEnumerator WallJump()
    {
        yield return new WaitForSeconds(0.05f);

        if (Input.GetKeyDown(KeyCode.Space) && isTouchingWall)
        {
            timeSinceWallJump = 0;
            Debug.Log("wall jump");
            rb.linearVelocityY = 0; 
            rb.AddForce(Vector2.up * wallJumpForceY, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * wallJumpForceX * -lookingDirection.x, ForceMode2D.Impulse); 
        }
    }
    #endregion

    private IEnumerator Dash()
    {
        isDashing = true; 
        oldVelX = rb.linearVelocityX;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0; 

        rb.linearVelocity = new Vector2(moveSpeed * dashMultiplier * lookingDirection.x, 0);
        yield return new WaitForSeconds(.2f);

        rb.gravityScale = originalGravity;
        rb.linearVelocityX = oldVelX;
        isDashing = false;
    }
}