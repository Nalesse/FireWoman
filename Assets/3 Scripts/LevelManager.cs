using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotteStuff;

public class LevelManager : MonoBehaviour
{
    //Description: Manages state of level which player moves through (controls core gameplay and visual functions related to the level/background)
    //NOTE: Level behavior and animations will vary drastically between proof-of-concept testing and actual prototype
    //NOTE: Also governs game flow, including when player loses

    //Objects & Components:
    private Transform backgroundGroup; //TEMP reference to level for testing purposes
    private Animator playerAnimator;   //Reference to player animator

    //Settings:
    [Header("Settings:")]
    [Range(0, 1)] public float tiltSensitivity; //Increasing this will decrease the amount phone has to be tilted to fully tilt the player
    public Vector2 slideRange;                  //The two ends of the range background elements slide when phone is being tilted
    public AnimationCurve stageSlideCurve;      //Describes how stage slides into position depending on lean value
    public Vector3 basePhoneOrientation;        //Orientation vector at which system assumes player is standing neutrally (should be able to be calibrated for)

    //Memory Vars:
    private Vector3 backgroundOrigPos; //Original position of level background elements

    private void Awake()
    {
        //Get Objects & Components:
        backgroundGroup = transform.GetChild(0); //Get background group container object
        playerAnimator = transform.GetChild(1).GetComponentInChildren<Animator>(); //Get player animator

        backgroundOrigPos = backgroundGroup.position; //Get starting position of background element
    }
    private void Update()
    {
        AnimateScene(); //Move background elements
    }

    private void AnimateScene()
    {
        //Function: Moves/animates background depending on player input

        //Get & Refine Input Variable:
        Vector3 rawOrientation = InputManager.manager.avgOrientation; //Get smoothed phone orientation from inputManager
        Vector3 relativeAngles = Quaternion.FromToRotation(basePhoneOrientation, rawOrientation).eulerAngles; //Get eulers for angles between base (neutral) vector and current phone alignment
        relativeAngles = new Vector3(HotteMath.NormalizeAngle(relativeAngles.x), HotteMath.NormalizeAngle(relativeAngles.y), HotteMath.NormalizeAngle(relativeAngles.z)); //Put all angles in -180 to 180 range
        float tiltIntensity = relativeAngles.x / 90; //Get intensity and direction in which phone is tilting (between -1 and 1)
        tiltIntensity = Mathf.Clamp(HotteMath.Map(tiltIntensity, -(1 - tiltSensitivity), 1 - tiltSensitivity, -1, 1), -1, 1); //Magnify phone tilt based on given setting

        //Animate Background:
        float waterSlide = Mathf.Lerp(-slideRange.x, -slideRange.y, stageSlideCurve.Evaluate(-tiltIntensity)); //Find target elevation of water side of level
        float fireSlide = Mathf.Lerp(slideRange.x, slideRange.y, stageSlideCurve.Evaluate(tiltIntensity));    //Find target elevation of fire side of level
        Vector3 targetPosWater = backgroundOrigPos + new Vector3(0, waterSlide, 0); //Get target vector for water side of level
        Vector3 targetPosFire = backgroundOrigPos + new Vector3(0, fireSlide, 0); //Get target vector for fire side of level
        backgroundGroup.transform.GetChild(0).position = targetPosWater; //Move water background to target position
        backgroundGroup.transform.GetChild(1).position = targetPosFire;  //Move fire background to target position

        //Animate Player:
        float animatorTiltIntensity = 1f - ((tiltIntensity / 2) + 0.5f); //Get adjusted tilt intensity between 0 and 1 (and flipped)
        playerAnimator.SetFloat("Frame", animatorTiltIntensity); //Set lean based on tilt value
    }
}
