using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip noAmmoSound;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private int ammo;
    [SerializeField] private Vector2 bulletSpawnOffset = new Vector2(1f, 0f);
    [SerializeField] private bool showSpawnPoint = true; 
    
    private float lastShotTime;
    private bool isOnCooldown;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastShotTime = -cooldown; 
    }

    private void Update()
    {
        AimAtMouse();
        ammoText.text = ammo.ToString();
        
        if (Input.GetKeyDown(fireKey) && !isOnCooldown && ammo > 0)
        {
            Shoot();
            StartCooldown();
            ammo--;
        }
        else if (Input.GetKeyDown(fireKey) && ammo <= 0)
        {
            audioSource.PlayOneShot(noAmmoSound);
        }
        
        if (isOnCooldown)
        {
            float cooldownProgress = (Time.time - lastShotTime) / cooldown;
            transform.Rotate(0, 0, 360 * Time.deltaTime / cooldown);
            
            if (cooldownProgress >= 1f)
            {
                isOnCooldown = false;
                transform.rotation = Quaternion.identity;
            }
        }
    }

    private void AimAtMouse()
    {
        if (isOnCooldown) return;
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        
        bool playerFacingRight = playerController != null ? playerController.IsFacingRight : true;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        bool cursorBehindPlayer = false;
        
        if (playerFacingRight)
        {
            cursorBehindPlayer = (angle > 90 || angle < -90);
            
            if (cursorBehindPlayer)
            {
                angle = 180 - angle;
                if (angle > 180) angle -= 360;
            }
        }
        else
        {
            cursorBehindPlayer = (angle < 90 && angle > -90);
            
            if (cursorBehindPlayer)
            {
                angle = 180 - angle;
                if (angle > 180) angle -= 360;
            }
            
            angle += 180;
            if (angle > 180) angle -= 360;
        }
        
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Shoot()
    {
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        bool playerFacingRight = playerController != null ? playerController.IsFacingRight : true;
        Vector2 offset = bulletSpawnOffset;
        if (!playerFacingRight)
        {
            offset.x = -offset.x;
            direction.x = -direction.x; 
            direction.y = -direction.y; 
        }
        
        float offsetX = offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle);
        float offsetY = offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle);
        Vector2 rotatedOffset = new Vector2(offsetX, offsetY);
        
        Vector2 spawnPosition = (Vector2)transform.position + rotatedOffset;
        
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        ShotgunBullet shotgunBullet = bullet.GetComponent<ShotgunBullet>();
        if (shotgunBullet != null)
        {
            shotgunBullet.SetDirection(direction);
        }
        
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    private void StartCooldown()
    {
        lastShotTime = Time.time;
        isOnCooldown = true;
    }
    
    private void OnDrawGizmos()
    {
        if (showSpawnPoint)
        {
            float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
            
            bool playerFacingRight = true;
            if (Application.isPlaying && playerController != null)
            {
                playerFacingRight = playerController.IsFacingRight;
            }
            
            Vector2 offset = bulletSpawnOffset;
            if (!playerFacingRight)
            {
                offset.x = -offset.x;
            }
            
            float offsetX = offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle);
            float offsetY = offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle);
            Vector2 rotatedOffset = new Vector2(offsetX, offsetY);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere((Vector2)transform.position + rotatedOffset, 0.1f);
        }
    }
}
