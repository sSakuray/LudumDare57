using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private AudioClip shootSound;
    
    private float lastShotTime;
    private bool isOnCooldown;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastShotTime = -cooldown; // Чтобы можно было стрелять сразу
    }

    private void Update()
    {
        // Поворот оружия к курсору мыши
        AimAtMouse();
        
        // Обработка выстрела
        if (Input.GetKeyDown(fireKey) && !isOnCooldown)
        {
            Shoot();
            StartCooldown();
        }
        
        // Вращение во время кулдауна
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
        
        // Определяем направление взгляда персонажа
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Проверяем, нужно ли перевернуть оружие
        if ((angle > 90 || angle < -90) && facingRight)
        {
            FlipWeapon();
        }
        else if ((angle < 90 && angle > -90) && !facingRight)
        {
            FlipWeapon();
        }
        
        // Поворачиваем оружие
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FlipWeapon()
    {
        facingRight = !facingRight;
        spriteRenderer.flipY = !spriteRenderer.flipY;
    }

    private void Shoot()
    {
        // Создаем пулю
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Воспроизводим звук выстрела
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
}
