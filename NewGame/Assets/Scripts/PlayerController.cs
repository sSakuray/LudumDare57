using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private bool _isFacingRight = true;

    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaDepletionRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private KeyCode sprintKey;

    [Header("Health")]
    [SerializeField] private int maxHealth;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.1f);
    [SerializeField] private Vector2 wallCheckSize = new(0.1f, 1.5f);
    [SerializeField] private Vector2 bottomCheckSize = new(0.5f, 0.1f);
    [SerializeField] private float bottomCheckOffset = 0.5f;

    [Header("Wall Movement")] 
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private bool blockMoveX;
    [SerializeField] private float jumpWallTime = 0.5f;
    [SerializeField] private float timerJumpWall;
    public Vector2 JumpAngle = new (3.5f, 10f);

    [Header("OtherParameters")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isWalled;
    [SerializeField] private bool isSprinting;
    [SerializeField] private float currentStamina;
    [SerializeField] private int currentHealth;
    [SerializeField] private float gravityDef;

    Rigidbody2D _rb;
    Collider2D _collider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        currentStamina = maxStamina;
        currentHealth = maxHealth;
        gravityDef = _rb.gravityScale;
    }

    private void Update()
    {
        isGrounded = CheckGrounded();
        isWalled = CheckWalled();
        Movement();
        Jump();
        Sprint();
        WallJump();

        if (!isWalled)
        {
            FlipCharacter();
        }
    }

    private void Movement()
    {
        if (IsOnTopOfWall())
        {
            GroundMovement();
        }
        else if (isWalled && !isGrounded)
        {
            WallMovement();
        }
        else if (isWalled && isGrounded)
        {
            GroundMovement();
            
            float moveY = Input.GetAxis("Vertical");
            if (moveY > 0) 
            {
                _rb.velocity = new Vector2(_rb.velocity.x, moveY * wallClimbSpeed);
            }
        }
        else
        {
            GroundMovement();
        }
    }

    private void GroundMovement()
    {
        if (!blockMoveX)
        {
            float moveX = Input.GetAxis("Horizontal");
            if (moveX != 0)
            {
                FlipCharacter();
            }
            float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        _rb.velocity = new Vector2(moveX * speed, _rb.velocity.y);
        }
    }

    private void WallMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        
        if (isGrounded && moveX != 0)
        {
            if ((_isFacingRight && moveX < 0) || (!_isFacingRight && moveX > 0))
            {
                isWalled = false;
                GroundMovement();
                return;
            }
        }

        if (!blockMoveX)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);

            float moveY = Input.GetAxis("Vertical");
            if (moveY != 0)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, moveY * wallClimbSpeed);
            }

            if (moveY == 0)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, 0); 
            }
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded || IsOnTopOfWall())
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            }
        }
    }

    private void WallJump()
    {
        if (isWalled && !IsOnTopOfWall() && Input.GetKeyDown(jumpKey))
        {
            blockMoveX = true;

            _isFacingRight = !_isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;

            _rb.gravityScale = gravityDef;
            _rb.velocity = new Vector2(0, 0);

            _rb.velocity = new Vector2(transform.localScale.x * JumpAngle.x, JumpAngle.y);
        }

        if (blockMoveX && (timerJumpWall += Time.deltaTime) > jumpWallTime)
        {
            if (isGrounded || isWalled || Input.GetAxisRaw("Horizontal") != 0)
            {
                blockMoveX = false;
                timerJumpWall = 0;
            }
        }
        
    }

    private bool CheckGrounded()
    {
        Collider2D col = Physics2D.OverlapBox(
            transform.position,
            groundCheckSize,
            0,
            groundLayer
        );
        return col != null;
    }

    private bool CheckWalled()
    {
        if (IsOnTopOfWall()) return false; 
        
        Collider2D col = Physics2D.OverlapBox(
            transform.position, 
            wallCheckSize, 
            0, 
            wallLayer
        );
        return col != null;
    }

    private bool IsOnTopOfWall()
    {
        if (_collider == null) return false;
        
        Vector2 bottomPosition = new Vector2(
            transform.position.x,
            transform.position.y - bottomCheckOffset
        );
        
        Collider2D colBottom = Physics2D.OverlapBox(
            bottomPosition,
            bottomCheckSize,
            0,
            wallLayer
        );
        
        Collider2D colLeft = Physics2D.OverlapBox(
            new Vector2(transform.position.x - wallCheckSize.x, transform.position.y),
            new Vector2(0.1f, wallCheckSize.y * 0.8f), 
            0,
            wallLayer
        );
        
        Collider2D colRight = Physics2D.OverlapBox(
            new Vector2(transform.position.x + wallCheckSize.x, transform.position.y),
            new Vector2(0.1f, wallCheckSize.y * 0.8f), 
            0,
            wallLayer
        );
        
        return colBottom != null && colLeft == null && colRight == null;
    }

    private void Sprint()
    {
        bool wantToSprint = Input.GetKey(sprintKey);

        if (wantToSprint && currentStamina > 0)
        {
            isSprinting = true;
        }
        else 
        {
            isSprinting = false;
        }
        
        if (isSprinting)
        {
            currentStamina -= staminaDepletionRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                isSprinting = false;
                currentStamina = 0;
            }
        }
        else
        {
            currentStamina = Mathf.Min(
                currentStamina + staminaRegenRate * Time.deltaTime,
                maxStamina
            );
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, groundCheckSize);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, wallCheckSize);
        
        Gizmos.color = Color.green;
        Vector2 bottomPosition = new Vector2(
            transform.position.x,
            transform.position.y - bottomCheckOffset
        );
        Gizmos.DrawWireCube(bottomPosition, bottomCheckSize);
    }

    private void FlipCharacter()
    {
        if (_isFacingRight && _rb.velocity.x < 0 || !_isFacingRight && _rb.velocity.x > 0)
        {
            _isFacingRight = !_isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

}
