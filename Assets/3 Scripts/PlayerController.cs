using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Description: Gets information from phone and controls player character

    //Objects & Components:
    private Gyroscope gyro; //Main gyroscope on player's phone

    //Input Vars:
    private Vector3 currentGyro; //Current gyro orientation of phone

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get Objects & Components:
        gyro = Input.gyro; //Get gyro object from phone
    }
    private void Start()
    {
        StartCoroutine(InitializeGyro());
    }
    private void Update()
    {
        //Input Stuff:

    }
    IEnumerator InitializeGyro()
     {
         gyro.enabled = true;
         yield return null;
         Debug.Log(gyro.attitude); // attitude has data now
     }
}
