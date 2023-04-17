using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class GameState
{
    public int food;
    public int lumber;
    public int magic;
    public List<BuildingSaveData> buildingSaveData;
    public List<CharacterSaveData> characterSaveData;
    public List<Job> jobSaveData;
    public int objectId;
    public int characterId;
    public int jobId;
    public List<Vector2Int> partialGrassTiles;
    public HashSet<Vector2Int> fullGrassTiles;
    public List<Vector2Int> extraGrassSpawnLocations;
    public bool isSave;
    public int scenarioInfoId;
    public MissionDataGroup mainMission;
    public MissionDataGroup[] missionsData;
    public float traderSpeed;
    public List<Vector2Int> flags;
    public List<int> time;

    public GameState(   int food, int lumber, int magic, 
                        int objectId, int characterId, int jobId,
                        bool isSave = false)
    {
        this.food = food;
        this.lumber = lumber;
        this.magic = magic;
        this.buildingSaveData = new List<BuildingSaveData>();
        this.characterSaveData = new List<CharacterSaveData>();
        this.jobSaveData = new List<Job>();
        this.objectId = objectId;
        this.characterId = characterId;
        this.jobId = jobId;
        this.partialGrassTiles = new List<Vector2Int>();
        this.fullGrassTiles = new HashSet<Vector2Int>();
        this.extraGrassSpawnLocations = new List<Vector2Int>();
        this.isSave = isSave;
        this.scenarioInfoId = -1;
        this.mainMission = null;
        this.missionsData = null;
        this.traderSpeed = 1f;
        this.flags = new List<Vector2Int>();
        this.time = new List<int>();
    }

    public GameState()
    {
        this.food = 5;
        this.lumber = 5;
        this.magic = 5;
        this.buildingSaveData = new List<BuildingSaveData>();
        this.characterSaveData = new List<CharacterSaveData>();
        this.jobSaveData = new List<Job>();
        this.objectId = 0;
        this.characterId = 0;
        this.jobId = 0;
        this.partialGrassTiles = new List<Vector2Int>();
        this.fullGrassTiles = new HashSet<Vector2Int>();
        this.extraGrassSpawnLocations = new List<Vector2Int>();
        this.isSave = false;
        this.scenarioInfoId = -1;
        this.mainMission = null;
        this.missionsData = null;
        this.traderSpeed = 1f;
        this.flags = new List<Vector2Int>();
        this.time = new List<int>();
    }
}
