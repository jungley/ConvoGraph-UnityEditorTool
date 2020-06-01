using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomVariables_BranchCam;

/*
 * Contains the following:
 * ActorID - which character
 * DialogText - text of the dailog
 * Audio Clip - actor speaking
 * Animation clip 
 * CameraShots - Shots or Shot of the dialog
*/
[System.Serializable]
[ExecuteAlways]
public class ConvoData
{
    [SerializeField]
    //public string dialogText;
    public List<string> dialogTextList;
    public ActorField D_Actor;

    public ActorField Actor
    {
        get
        {
            return D_Actor;
        }
        set
        {
            D_Actor = value;
        }
    }

    //public AudioClip audio_clip;
    //public Animation anim_clip;
    //Calculate shot time from audio clip length?
    private CameraShot CameraShot;
    public CameraShot camerashot
    {
        get
        {
            return CameraShot;
        }
        set
        {
            CameraShot = value;
        }
    }

    //Need a default actor in the dialogue for it to be set
    //Pretty much the first actor in the SceneList
    public ConvoData(ActorField actor)
    {
        dialogTextList = new List<string>(); 
        D_Actor = actor;
    }



    //When Loading from a savenode
    public ConvoData(ActorField actor, List<string> dialog)
    {
        dialogTextList = dialog;
        D_Actor = actor;
    }
}
