using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private LayerMask targetLayers;
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & targetLayers) != 0)
        {   
            Destroy(gameObject);
        }
    }
}
