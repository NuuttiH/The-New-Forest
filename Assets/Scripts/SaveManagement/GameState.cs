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
    public ScenarioInfo scenarioInfo;
    public float traderSpeed;
    public List<Vector2Int> flags;

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
        this.scenarioInfo = null;
        this.traderSpeed = 1f;
        this.flags = new List<Vector2Int>();
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
        this.scenarioInfo = null;
        this.traderSpeed = 1f;
        this.flags = new List<Vector2Int>();
    }
}
