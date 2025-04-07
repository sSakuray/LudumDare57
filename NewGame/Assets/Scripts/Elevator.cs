using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] private int sceneBuildIndex;
    [SerializeField] private AudioClip buttonSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        title.SetActive(false);
        
        // Получаем или добавляем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (title == null)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            title.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (title == null)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            title.SetActive(false);
        }
    }

    private void ElevatorButton()
    {
        if (title == null)
        {
            return;
        }
        if (title.activeSelf && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ElevatorButtonPress());
            if (buttonSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(buttonSound);
            }
        }
    }

    private void Update()
    {
        ElevatorButton();
    }

    private IEnumerator ElevatorButtonPress()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
