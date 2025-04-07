using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TonextSceneWithDelay : MonoBehaviour
{
    [SerializeField] private int sceneToMove = 1;
    [SerializeField] private float delayTime = 18f;

    void Start()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(sceneToMove);
    }
}
