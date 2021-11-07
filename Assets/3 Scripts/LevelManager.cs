using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotteStuff;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //Description: Manages state of level which player moves through (controls core gameplay and visual functions related to the level/background)
    //NOTE: Level behavior and animations will vary drastically between proof-of-concept testing and actual prototype
    //NOTE: Also governs game flow, including when player loses

    //Objects & Components:
    private Transform backgroundGroup; //TEMP reference to level for testing purposes
    private Animator playerAnimator;   //Reference to player animator
    private AudioSource audioSource;   //Reference to level audio source
    public AudioClip fireDeathSound;   //Sound that plays when player falls in fire
    public AudioClip waterDeathSound;  //Sound that plays when player falls in water

    //Settings:
    [Header("Settings:")]
    [Range(0, 1)] public float tiltSensitivity;   //Increasing this will decrease the amount phone has to be tilted to fully tilt the player
    public Vector2 slideRange;                    //The two ends of the range background elements slide when phone is being tilted
    public AnimationCurve stageSlideCurve;        //Describes how stage slides into position depending on lean value
    public Vector3 basePhoneOrientation;          //Orientation vector at which system assumes player is standing neutrally (should be able to be calibrated for)
    [Range(0, 1)] public float endStageSnapSpeed; //How fast stage snaps into end position when game is over
    public float stageAdvancePos;                 //Y position stages advance to when player falls in its direction
    public float stageRecedePos;                  //Y position stages recede to when player falls in other direction
    public float respawnTime;                     //Time to wait (in seconds) (after player has died) before reloading the scene

    //Memory Vars:
    internal float timeDead = 0; //How long player has been dead for, always zero when player is alive
    private Vector3 backgroundOrigPos; //Original position of level background elements

    private void Awake()
    {
        //Get Objects & Components:
        backgroundGroup = transform.GetChild(0); //Get background group container object
        playerAnimator = transform.GetChild(1).GetComponentInChildren<Animator>(); //Get player animator
        audioSource = GetComponent<AudioSource>(); //Get audio source component

        backgroundOrigPos = backgroundGroup.position; //Get starting position of background element
    }
    private void Update()
    {
        AnimateScene(); //Move background elements

        //Increment Timers:
        if (timeDead > 0) //Only increment death timer while player is dead
        {
            timeDead += Time.deltaTime; //Increment by deltaTime
            if (timeDead >= respawnTime) SceneManager.LoadScene(SceneManager.GetActiveScene().name); //Reload scene if death timer exceeds given respawn time
        }
    }

    private void AnimateScene()
    {
        //Function: Moves/animates background depending on player input

        //Initial Check:
        if (timeDead > 0) { AnimateSceneDead(); return; } //Use special scene animator if player is dead

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
        backgroundGroup.GetChild(0).position = targetPosWater; //Move water background to target position
        backgroundGroup.GetChild(1).position = targetPosFire;  //Move fire background to target position

        //Animate Player:
        float animatorTiltIntensity = 1f - ((tiltIntensity / 2) + 0.5f); //Get adjusted tilt intensity between 0 and 1 (and flipped)
        playerAnimator.SetFloat("Frame", animatorTiltIntensity); //Set lean based on tilt value
        if (animatorTiltIntensity >= 1 || animatorTiltIntensity <= 0) //Player death by lean
        {
            //Animate Death:
            if (animatorTiltIntensity >= 1) audioSource.clip = fireDeathSound; //Death by fire, set corresponding audio
            else audioSource.clip = waterDeathSound; //Death by water, set corresponding audio
            audioSource.Play(); //Play death sound

            //Indicate Death:
            playerAnimator.SetBool("Fell", true); //Animate player death (selected automatically by animator)
            timeDead += Time.deltaTime; //Indicate that player has died (by starting death timer)
        }

        //Adjust Audio Intensity:
        backgroundGroup.GetChild(0).GetComponent<AudioSource>().volume = 1 - animatorTiltIntensity;     //Set loudness of water depending on how close player is
        backgroundGroup.GetChild(1).GetComponent<AudioSource>().volume = animatorTiltIntensity; //Set loudness of fire depending on how close player is

    }
    private void AnimateSceneDead()
    {
        //Function: Scene animation stuff that happens after player has died (one background covers the entire screen while the other one recedes

        //Animate Background:
        Vector3 targetFirePosition = new Vector3(); //Initialize container for fire stage position
        Vector3 targetWaterPosition = new Vector3(); //Initialize container for water stage position
        if (playerAnimator.GetFloat("Frame") > 0.5) //FIRE death scene animation
        {
            targetFirePosition.y = -stageAdvancePos; //Set fire to advance down
            targetWaterPosition.y = -stageRecedePos; //Set water to recede down
        }
        else //WATER death scene animation
        {
            targetFirePosition.y = stageRecedePos;   //Set fire to recede up
            targetWaterPosition.y = stageAdvancePos; //Set water to advance up
        }
        backgroundGroup.transform.GetChild(0).position = Vector3.Lerp(backgroundGroup.transform.GetChild(0).position, targetWaterPosition, endStageSnapSpeed * Time.deltaTime * 200);  //Move water background toward target position
        backgroundGroup.transform.GetChild(1).position = Vector3.Lerp(backgroundGroup.transform.GetChild(1).position, targetFirePosition, endStageSnapSpeed * Time.deltaTime * 200); //Move fire background toward target position
    }
}
