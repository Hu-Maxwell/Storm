using UnityEngine;
using System.Collections;
using System;

public class BirdMovementScript : MonoBehaviour
{

    #region vars
    public Rigidbody2D rb;
    public SpriteRenderer sprite;

    public Vector2 lookingDirection = new Vector2(1, 0);

    public float rayLenX = 2.48f;
    public float rayLenY = 3.30f;
    public LayerMask levelLayer;

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
    public float coyoteTime = 0.1f;

    public float jumpDownVel = -10.0f;
    public float groundCheckDistance = 0.1f;

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
        levelLayer = LayerMask.GetMask("level"); 
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

        UpdateDirection();
        CheckCollisions(); 
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return; 
        }
        UpdateSpeed();
    }

    private void CheckCollisions()
    {
        RaycastHit2D downRay = Physics2D.Raycast(transform.position, Vector2.down, rayLenY, levelLayer);
        RaycastHit2D leftRay = Physics2D.Raycast(transform.position, Vector2.left, rayLenX, levelLayer);
        RaycastHit2D rightRay = Physics2D.Raycast(transform.position, Vector2.right, rayLenX, levelLayer);

        Debug.DrawRay(transform.position, Vector2.down * rayLenY);
        Debug.DrawRay(transform.position, Vector2.left * rayLenX);
        Debug.DrawRay(transform.position, Vector2.right * rayLenX);

        if(downRay)
        {
            isGrounded = true;
            if(timeSinceJump > coyoteTime)
            {
                isJumping = false;
            }
            hasDoubleJumped = false;
        } else
        {
            isGrounded = false; 
        }

        if(leftRay || rightRay)
        {
            isTouchingWall = true; 
        } else
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
    }

    private IEnumerator DoubleJump()
    {
        yield return new WaitForSeconds(0.05f);

        if (Input.GetKeyDown(KeyCode.Space) && !hasDoubleJumped) // TODO: if too close to floor, don't double jump 
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