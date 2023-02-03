using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterSaveData
{
    // Data for spawning object
    public Vector3 location;
    public Quaternion rotation;
    public string prefabName;
    
    // Data that object needs to keep
    public int characterId;
    public bool working;
    public bool moving;
    public Job job;
    public Vector3 targetLocation;
    public float waitTime;
    public bool cancelJob;
    public int colorChoice;

    public CharacterSaveData(   Vector3 location, Quaternion rotation, 
                                string prefabName, int characterId,
                                bool working, bool moving, 
                                Job job, Vector3 targetLocation, 
                                float waitTime, bool cancelJob,
                                int colorChoice)
    {
        this.location = location;
        this.rotation = rotation;
        this.prefabName = prefabName;
        this.characterId = characterId;
        this.working = working;
        this.moving = moving;
        this.job = job;
        this.targetLocation = targetLocation;
        this.waitTime = waitTime;
        this.cancelJob = cancelJob;
        this.colorChoice = colorChoice;
    }
    public CharacterSaveData(){}
}
