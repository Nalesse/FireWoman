using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotteStuff;

public class LevelManager : MonoBehaviour
{
    //Description: Manages state of level which player moves through (controls core gameplay and visual functions related to the level/background)
    //NOTE: Level behavior and animations will vary drastically between proof-of-concept testing and actual prototype

    //Objects & Components:
    private Transform backgroundGroup; //TEMP reference to level for testing purposes

    //Settings:
    [Header("Settings:")]
    public Vector2 slideRange; //Maximum distance the background moves vertically and horizontally
    public Vector3 basePhoneOrientation; //Orientation vector at which system assumes player is standing neutrally (should be able to be calibrated for)

    //Memory Vars:
    private Vector3 backgroundOrigPos; //Original position of level background elements

    private void Awake()
    {
        backgroundGroup = transform.GetChild(0);      //Get background group container object
        backgroundOrigPos = backgroundGroup.position; //Get starting position of background element
    }
    private void Update()
    {
        PositionBackground(); //Move background elements
    }

    private void PositionBackground()
    {
        //Function: Moves/animates background depending on player input

        //Get Workable Input Variable:
        Vector3 rawOrientation = InputManager.manager.avgOrientation; //Get smoothed phone orientation from inputManager
        Vector3 relativeAngles = Quaternion.FromToRotation(basePhoneOrientation, rawOrientation).eulerAngles; //Get eulers for angles between base (neutral) vector and current phone alignment
        relativeAngles = new Vector3(HotteMath.NormalizeAngle(relativeAngles.x), HotteMath.NormalizeAngle(relativeAngles.y), HotteMath.NormalizeAngle(relativeAngles.z)); //Put all angles in -180 to 180 range
        //print("Relative = " + relativeAngles);

        //Determine Background Translation:
        float slideIntensity = relativeAngles.x / 90; //Get intensity and direction in which background should slide (within range 0-1) based on vertical angle of phone
        //print("Slide = " + slideIntensity);
        Vector3 targetPos = backgroundOrigPos + new Vector3(0, slideRange.y * slideIntensity, 0); //Get target vector for background (limit full range based on known slide intensity)
        backgroundGroup.transform.position = targetPos; //TEMP: Just snap background to target position
    }
}
