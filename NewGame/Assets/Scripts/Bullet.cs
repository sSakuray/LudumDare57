using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private LayerMask targetLayers;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            DestroyBullet();
        }
        else
        {
            DestroyBullet();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {

        DestroyBullet();
    }
    
    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
