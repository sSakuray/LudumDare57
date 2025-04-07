using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private bool _isFacingRight = true;
    
    public bool IsFacingRight => _isFacingRight;

    [Header("Stamina")]
    [SerializeField] private int maxStamina;
    [SerializeField] private float staminaDepletionRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private int currentStamina;
    [SerializeField] private KeyCode sprintKey;
    [SerializeField] private Image staminaBar;
    private float staminaFloat;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image[] heartImages; 
    [SerializeField] private Sprite fullHeartSprite; 
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.1f);
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] private Vector2 wallCheckSize = new(0.1f, 1.5f);
    [SerializeField] private Vector2 wallCheckOffset = new(0.5f, 0f);
    [SerializeField] private Vector2 bottomCheckSize = new(0.5f, 0.1f);
    [SerializeField] private Vector2 bottomCheckOffset = new(0f, -0.5f);

    [Header("Wall Movement")] 
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private bool blockMoveX;
    [SerializeField] private float jumpWallTime = 0.5f;
    [SerializeField] private float timerJumpWall;
    public Vector2 JumpAngle = new (3.5f, 10f);

    [Header("VFX & Die")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float knockbackForceY; 
    public Vector2 hitAngle = new (2.5f, 4f);
    private bool isDead = false;
    private Animator camAnim;

    [Header("OtherParameters")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isWalled;
    [SerializeField] private bool isSprinting;
    [SerializeField] private int currentHealth;
    [SerializeField] private float gravityDef;
    [SerializeField] private GameObject parentGameOjb;
    private bool isJumping = false;
    private bool wasGrounded = true;
    private bool isWallClimbing = false;
    private Animator playerAnim;
    Rigidbody2D _rb;
    Collider2D _collider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        currentStamina = maxStamina;
        staminaFloat = maxStamina;
        currentHealth = maxHealth;
        gravityDef = _rb.gravityScale;
        camAnim = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Animator>();
        UpdateHealthUI();
        playerAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();
        isWalled = CheckWalled();
        Movement();
        Jump();
        Sprint();
        WallJump();
        HandleJumpAnimation();
        HandleWallClimbAnimation();
        if (!isWalled)
        {
            FlipCharacter();
        }
        UpdateWalkAnimation();

    }

    private void HandleJumpAnimation()
    {
        if (playerAnim == null) return;

        if (!isGrounded && wasGrounded && !isJumping)
        {
            isJumping = true;
            playerAnim.SetBool("isJumpsS", true);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isRunning", false);
        }

        if (isGrounded && !wasGrounded && isJumping)
        {
            isJumping = false;
            playerAnim.SetBool("isJumpsS", false);
        }
    }

    private void HandleWallClimbAnimation()
    {
        if (playerAnim == null) return;

        if (isWalled && !isGrounded && Input.GetAxis("Vertical") != 0)
        {
            isWallClimbing = true;
            playerAnim.SetBool("isWallClimbingsS", true);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isRunning", false);
            playerAnim.SetBool("isJumpsS", false);
        }
        else if (isWallClimbing && (!isWalled || isGrounded))
        {
            isWallClimbing = false;
            playerAnim.SetBool("isWallClimbingsS", false);
        }
    }

    private void UpdateWalkAnimation()
    {
        if (playerAnim == null || isJumping || isWallClimbing) return;
        
        float moveInput = Input.GetAxis("Horizontal");
        bool isMoving = Mathf.Abs(moveInput) > 0.1f;
        
        playerAnim.SetBool("isRunning", isSprinting && staminaFloat > 0 && isMoving);
        playerAnim.SetBool("isWalk", isMoving && !isSprinting);
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
                if (playerAnim != null)
                {
                    playerAnim.SetBool("isWallClimbingsS", true);
                }

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
            isWallClimbing = false;
            isJumping = true;
            if (playerAnim != null)
            {
                playerAnim.SetBool("isWallClimbingsS", false);
                playerAnim.SetBool("isJumpsS", true);
            }
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
            transform.position + new Vector3(groundCheckOffset.x, groundCheckOffset.y, 0),
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
            transform.position + new Vector3(wallCheckOffset.x * (_isFacingRight ? 1 : -1), wallCheckOffset.y, 0), 
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
            transform.position.x + bottomCheckOffset.x,
            transform.position.y + bottomCheckOffset.y
        );
        
        Collider2D colBottom = Physics2D.OverlapBox(
            bottomPosition,
            bottomCheckSize,
            0,
            wallLayer
        );
        
        Collider2D colLeft = Physics2D.OverlapBox(
            new Vector2(transform.position.x - wallCheckOffset.x, transform.position.y + wallCheckOffset.y),
            new Vector2(0.1f, wallCheckSize.y * 0.8f), 
            0,
            wallLayer
        );
        
        Collider2D colRight = Physics2D.OverlapBox(
            new Vector2(transform.position.x + wallCheckOffset.x, transform.position.y + wallCheckOffset.y),
            new Vector2(0.1f, wallCheckSize.y * 0.8f), 
            0,
            wallLayer
        );
        
        return colBottom != null && colLeft == null && colRight == null;
    }

    private void Sprint()
    {
        bool wantToSprint = Input.GetKey(sprintKey);

        if (wantToSprint && staminaFloat > 0 && (isGrounded || IsOnTopOfWall()))
        {
            isSprinting = true;
        }
        else 
        {
            isSprinting = false;
        }
        
        if (isSprinting)
        {
            staminaFloat -= staminaDepletionRate * Time.deltaTime;
            currentStamina = (int)staminaFloat;
            if (currentStamina <= 0)
            {
                isSprinting = false;
                currentStamina = 0;
                staminaFloat = 0;
            }
        }
        else
        {
            staminaFloat = Mathf.Min(
                staminaFloat + staminaRegenRate * Time.deltaTime,
                maxStamina
            );
            currentStamina = (int)staminaFloat;
        }
        UpdateStaminaBar();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(groundCheckOffset.x, groundCheckOffset.y, 0), 
            groundCheckSize
        );
        
        Gizmos.color = Color.blue;
        Vector3 wallCheckPosition = transform.position + 
            new Vector3(wallCheckOffset.x * (Application.isPlaying ? (_isFacingRight ? 1 : -1) : 1), wallCheckOffset.y, 0);
        Gizmos.DrawWireCube(wallCheckPosition, wallCheckSize);
        
        Gizmos.color = Color.green;
        Vector2 bottomPosition = new Vector2(
            transform.position.x + bottomCheckOffset.x,
            transform.position.y + bottomCheckOffset.y
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spike") && !isDead)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, knockbackForceY);
            
            TakeDamage(1);
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        AudioSource.PlayClipAtPoint(hitSound, transform.position);
        
        blockMoveX = true;
        _rb.gravityScale = gravityDef;
        _rb.velocity = Vector2.zero;
        
        _rb.velocity = new Vector2(-Mathf.Abs(hitAngle.x), hitAngle.y);
        
        currentHealth -= damageAmount;
        UpdateHealthUI();
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            camAnim.SetTrigger("Shake");
        }
    }
    private void UpdateHealthUI()
    {
        if (heartImages == null || heartImages.Length == 0 || fullHeartSprite == null || emptyHeartSprite == null) return;
        
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = true;
            
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
    private void Die()
    {
        camAnim.SetTrigger("Shake");
        isDead = true;
        _rb.simulated = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        LevelReloader.ReloadAfterDelay(2f);
        
        if (parentGameOjb != null)
        {
            Destroy(parentGameOjb);
        }
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            float staminaPercent = (float)staminaFloat / maxStamina;
            
            staminaBar.fillAmount = staminaPercent;
        }
    }

    public static class LevelReloader
    {
        private class TempMonoBehaviour : MonoBehaviour {}
        
        public static void ReloadAfterDelay(float delay)
        {
            var tempObj = new GameObject("TempReloader");
            tempObj.AddComponent<TempMonoBehaviour>()
                .StartCoroutine(ReloadCoroutine(delay, tempObj));
        }

        private static IEnumerator ReloadCoroutine(float delay, GameObject tempObj)
        {
            yield return new WaitForSecondsRealtime(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            
        }
    }
}
