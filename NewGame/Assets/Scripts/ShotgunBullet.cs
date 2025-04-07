using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    
    public float Speed => speed;
    
    private Rigidbody2D rb;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void SetDirection(Vector2 direction)
    {
        if (!initialized && rb != null)
        {
            rb.velocity = direction.normalized * speed;
            initialized = true;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            Debug.DrawRay(transform.position, direction.normalized * 2, Color.red, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void HandleCollision(GameObject hitObject)
    {
        EnemyOne enemy = hitObject.GetComponent<EnemyOne>();
        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
        }
        else if (!hitObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
