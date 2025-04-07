using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TonextScene : MonoBehaviour
{
    [SerializeField] private int sceneToMove = 1;

    public void ToNextScene()
    {
        SceneManager.LoadScene(sceneToMove);
    }
}
