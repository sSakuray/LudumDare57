using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] private int sceneBuildIndex;
    void Start()
    {
        title.SetActive(false);
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
