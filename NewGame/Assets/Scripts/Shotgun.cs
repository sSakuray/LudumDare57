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
    
    [Header("Bullet Spawn Settings")]
    [SerializeField] private Vector2 bulletSpawnOffset = new Vector2(1f, 0f); // Смещение точки спавна пули
    [SerializeField] private bool showSpawnPoint = true; // Показывать точку спавна в редакторе
    
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
        // Получаем направление выстрела на основе угла поворота оружия
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // Определяем позицию спавна пули с учетом смещения и направления персонажа
        bool playerFacingRight = playerController != null ? playerController.IsFacingRight : true;
        Vector2 offset = bulletSpawnOffset;
        if (!playerFacingRight)
        {
            offset.x = -offset.x; // Инвертируем X-смещение, если персонаж смотрит влево
            direction.x = -direction.x; // Инвертируем направление X, если персонаж смотрит влево
            direction.y = -direction.y; // Инвертируем направление Y, если персонаж смотрит влево
        }
        
        // Вращаем смещение в соответствии с углом оружия
        float offsetX = offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle);
        float offsetY = offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle);
        Vector2 rotatedOffset = new Vector2(offsetX, offsetY);
        
        // Вычисляем позицию спавна пули
        Vector2 spawnPosition = (Vector2)transform.position + rotatedOffset;
        
        // Создаем пулю с нулевым поворотом
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        // Устанавливаем направление пули через новый метод
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
            // Получаем угол поворота оружия
            float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
            
            // Определяем направление в зависимости от того, смотрит ли персонаж влево или вправо
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
            
            // Вращаем смещение в соответствии с углом оружия
            float offsetX = offset.x * Mathf.Cos(angle) - offset.y * Mathf.Sin(angle);
            float offsetY = offset.x * Mathf.Sin(angle) + offset.y * Mathf.Cos(angle);
            Vector2 rotatedOffset = new Vector2(offsetX, offsetY);
            
            // Рисуем точку спавна пули
            Gizmos.color = Color.red;
            Gizmos.DrawSphere((Vector2)transform.position + rotatedOffset, 0.1f);
        }
    }
}
