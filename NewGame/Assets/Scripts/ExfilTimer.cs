using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ExfilTimer : MonoBehaviour
{
    [SerializeField] private float timeleft;
    [SerializeField] private TMP_Text timertext;
    bool iscounting;
    void Start()
    {
        iscounting = true;
    }

    void Update()
    {
        if (iscounting)
        {
            timeleft -= Time.deltaTime;
        }

        timertext.text = timeleft.ToString("0");

        if (timeleft <= 0)
        {
            iscounting = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
