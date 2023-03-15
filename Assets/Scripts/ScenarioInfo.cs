using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MissionGoal {   None, Food, Lumber, Magic, BuildLumber, BuildFood,
                            BuildShrine, BuildTent, NewWorker, NewGrass, TreeCount,
                            BuildGreatTree }

[Serializable]
public class MissionData {
    public string title;
    [TextArea(2, 10)]
    public string description;
    public MissionGoal missionGoal;
    public int goalVal;
    public int currentVal = 0;
    /*
    public MissionData(string title, string description, MissionGoal missionGoal, int goalVal)
    {
        this.title = title;
        this.description = description;
        this.missionGoal = missionGoal;
        this.goalVal = goalVal;
    }*/
}

[Serializable]
public class MissionDataGroup {
    public string title;
    [TextArea(2, 10)]
    public string description;
    public MissionData[] missions;
    public int nextMissionId;
    public int previousLinkCount = 0;
    [HideInInspector] public int currentVal = 0;
    /*
    public MissionDataGroup(string title)
    {
        this.title = title;
    }*/
}

[CreateAssetMenu(fileName = "90 ScenarioInfo", menuName = "ScriptableObject/ObjectInfo/ScenarioInfo")]
public class ScenarioInfo : ObjectInfo
{
    public MissionDataGroup mainMission;
    public MissionDataGroup[] missionsData;
}
