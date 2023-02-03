using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingSaveData
{
    // Data for spawning object
    public Vector3 location;
    public Quaternion rotation;
    public string prefabName;
    
    // Data that object needs to keep
    public int buildingId;
    public Vector3Int size;
    public Vector3Int startTile;
    public float growthProgress;
    public int cutDownjobIndex;
    public int jobIndex;

    public BuildingSaveData(   Vector3 location, Quaternion rotation, 
                                string prefabName, int buildingId,
                                Vector3Int size, Vector3Int startTile, 
                                float growthProgress, int cutDownjobIndex, 
                                int jobIndex)
    {
        this.location = location;
        this.rotation = rotation;
        this.prefabName = prefabName;
        this.buildingId = buildingId;
        this.size = size;
        this.startTile = startTile;
        this.growthProgress = growthProgress;
        this.cutDownjobIndex = cutDownjobIndex;
        this.jobIndex = jobIndex;
    }
    public BuildingSaveData(){}
}
