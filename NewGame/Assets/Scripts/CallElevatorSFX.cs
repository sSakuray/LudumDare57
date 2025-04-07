using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallElevatorSFX : MonoBehaviour
{
    [SerializeField] private AudioSource menuOST;
    [SerializeField] private AudioSource elevatorSFX;
    public void CallElevator()
    {
        menuOST.Stop();
        elevatorSFX.Play();
    }
}
