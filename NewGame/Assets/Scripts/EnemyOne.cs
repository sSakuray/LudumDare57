using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOne : MonoBehaviour
{
    [Header("Зоны поведения")]
    [SerializeField] private float patrolLeftLimit; 
    [SerializeField] private float patrolRightLimit; 
    [SerializeField] private float shootingRadius; 
    [SerializeField] private float chasingRadius; 
    [SerializeField] private float meleeRadius; 
    
    [Header("Патрулирование")]
    [SerializeField] private float patrolSpeed; 
    [SerializeField] private float chasingSpeed; 
    [SerializeField] private float patrolWaitTime; 
    
    [Header("Стрельба")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float shootCooldown;
    
    [Header("Ближний бой")]
    [SerializeField] private int meleeDamage; 
    [SerializeField] private float meleeAttackRange;
    [SerializeField] private float meleeAttackCooldown; 

    [Header("Обнаружение")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    
    private Vector3 startPosition;
    private Vector3 leftPatrolPoint; 
    private Vector3 rightPatrolPoint; 
    private Transform player; 
    private Rigidbody2D rb; 
    private bool isFacingRight = true; 
    private bool canShoot = true; 
    private bool canMeleeAttack = true; 
    private bool isWaitingAtPatrolPoint = false; 
    private bool movingRight = true; 
    private enum EnemyState 
    { 
        Patrolling,  
        Shooting,   
        Chasing,       
        MeleeAttacking 
    }
    private EnemyState currentState = EnemyState.Patrolling;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;

        leftPatrolPoint = new Vector3(startPosition.x - patrolLeftLimit, startPosition.y, startPosition.z);
        rightPatrolPoint = new Vector3(startPosition.x + patrolRightLimit, startPosition.y, startPosition.z);
    }
    
    private void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        bool canSeePlayer = CheckLineOfSight();
        
        if (canSeePlayer)
        {
            if (distanceToPlayer <= meleeRadius)
            {
                currentState = EnemyState.MeleeAttacking;
            }
            else if (distanceToPlayer <= chasingRadius)
            {
                currentState = EnemyState.Chasing;
            }
            else if (distanceToPlayer <= shootingRadius)
            {
                currentState = EnemyState.Shooting;
            }
            else
            {
                currentState = EnemyState.Patrolling;
            }
        }
        else
        {
            currentState = EnemyState.Patrolling;
        }
        
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
                
            case EnemyState.Shooting:
                rb.velocity = Vector2.zero;
                FacePlayer();
                if (canShoot)
                {
                    Shoot();
                }
                break;
                
            case EnemyState.Chasing:
                ChasePlayer();
                break;
                
            case EnemyState.MeleeAttacking:
                PrepareForAttack();
                if (canMeleeAttack)
                {
                    MeleeAttack();
                }
                break;
        }
    }
    
    private bool CheckLineOfSight()
    {
        if (player == null) return false;
        
        RaycastHit2D hit = Physics2D.Linecast(
            transform.position,
            player.position,
            obstacleLayer
        );
        
        return hit.collider == null || hit.collider.CompareTag("Player");
    }
    
    private void Patrol()
    {
        if (isWaitingAtPatrolPoint)
            return;
        
        Vector3 targetPoint = movingRight ? rightPatrolPoint : leftPatrolPoint;
        
        float direction = Mathf.Sign(targetPoint.x - transform.position.x);
        
        if (direction > 0 && !isFacingRight || direction < 0 && isFacingRight)
        {
            Flip();
        }
        
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
        
        float distanceToTarget = Mathf.Abs(transform.position.x - targetPoint.x);
        if (distanceToTarget < 0.1f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }
    
    private IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        rb.velocity = Vector2.zero;
        
        yield return new WaitForSeconds(patrolWaitTime);
        
        movingRight = !movingRight;
        Flip();
        isWaitingAtPatrolPoint = false;
    }
    
    private void ChasePlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        FacePlayer();
        
        if (distanceToPlayer > meleeAttackRange)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * chasingSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, находится ли объект, с которым столкнулись, на слое obstacleLayer
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            // Разворачиваем врага в противоположную сторону
            movingRight = !movingRight;
            Flip();
            
            // Устанавливаем скорость в новом направлении
            float direction = movingRight ? 1f : -1f;
            rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
        }
    }
    
    private void PrepareForAttack()
    {
        if (player == null) return;
        
        rb.velocity = Vector2.zero;
        FacePlayer();
    }
    
    private void Shoot()
    {
        if (!canShoot || player == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        Vector2 direction = (player.position - firePoint.position).normalized;
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        StartCoroutine(ShootCooldown());
    }
    
    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
    
    private void MeleeAttack()
    {
        if (!canMeleeAttack || player == null) return;
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            transform.position,
            meleeAttackRange,
            playerLayer
        );
        
        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerController playerController = playerCollider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(meleeDamage);
            }
        }
        
        StartCoroutine(MeleeAttackCooldown());
    }
    
    private IEnumerator MeleeAttackCooldown()
    {
        canMeleeAttack = false;
        yield return new WaitForSeconds(meleeAttackCooldown);
        canMeleeAttack = true;
    }
    
    private void FacePlayer()
    {
        if (player == null) return;
        
        bool shouldFaceRight = player.position.x > transform.position.x;
        
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 position = Application.isPlaying ? startPosition : transform.position;
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(position.x - patrolLeftLimit, position.y, position.z),
            new Vector3(position.x + patrolRightLimit, position.y, position.z)
        );
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chasingRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRadius);
    }
}