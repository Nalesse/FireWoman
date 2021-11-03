using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Description: Gets information from phone and controls player character

    //Objects & Components:
    private Transform orientator; //TEMP: thing to show phone orientation

    //Input Vars:
    private Vector3 orientation; //Normalized vector representing the spatial direction the back of the phone is facing (when phone is being held still)

    //RUNTIME METHODS:
    private void Awake()
    {
        orientator = transform.GetChild(0).GetChild(0); //TEMP
    }
    private void Update()
    {
        //Input Stuff:
        orientation = Input.acceleration; //Get acceleration from phone

        //Input Debugging:
        orientator.eulerAngles = (orientation * Mathf.Rad2Deg) + transform.GetChild(0).eulerAngles; //Move orientator to represent phone orientation (offset by container rotation so that it can be easily re-positioned)
        print(Input.acceleration);
    }
}
