using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
public class LinkedMissionData {
    public MissionData missionData;
    public int groupIndex;
    public int missionIndex;

    public LinkedMissionData(MissionData missionData, int groupIndex, int missionIndex)
    {
        this.missionData = missionData;
        this.groupIndex = groupIndex;
        this.missionIndex = missionIndex;
    }
}

public class MissionManager : MonoBehaviour
{
    private static MissionManager _instance;
    [SerializeField] private ScenarioInfo _scenarioInfo;
    private MissionDisplay _display;
    private int _mainMissionIndex = -1;
    private List<LinkedMissionData>[] _missionLinks;
    private List<MissionDataGroup> _activeMissionGroups;
    
    void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
        }
        _missionLinks = new List<LinkedMissionData>[System.Enum.GetValues(typeof(MissionGoal)).Length];
        for(int i=0; i<System.Enum.GetValues(typeof(MissionGoal)).Length; i++)
            _missionLinks[i] = new List<LinkedMissionData>();
        _activeMissionGroups = new List<MissionDataGroup>();
        //Events.onIncrementMission = (goal, count) => IncrementMission(goal, count);
        Events.onIncrementMission += IncrementMission;
    }

    public static void Init(ScenarioInfo scenarioInfo)
    {
        _instance._scenarioInfo = scenarioInfo;

        // Send text for main mission
        List<string> text = new List<string>();
        text.Add(_instance._scenarioInfo.mainMission.title + _instance._scenarioInfo.mainMission.description);
        foreach(MissionData mission in _instance._scenarioInfo.mainMission.missions)
        {
            text.Add($" -{mission.title} {mission.description} ({mission.currentVal}/{mission.goalVal})");
        }  
        _instance._display.Init(_instance._mainMissionIndex, text); 

        // Figure out current missions
        int groupIndex = 0;
        foreach(MissionDataGroup group in _instance._scenarioInfo.missionsData)
        {
            // Check missiongroups that have been reached
            if(group.currentVal >= group.previousLinkCount)
            {
                bool missionsFinished = true;
                for(int i=0; i<group.missions.Length; i++)
                {
                    if(group.missions[i].currentVal < group.missions[i].goalVal) missionsFinished = false;
                }
                if(!missionsFinished)
                {
                    RegisterMissionGroup(groupIndex);
                }
            }
            groupIndex++;
        }
    }

    public static ScenarioInfo GetScenarioInfo()
    {
        return _instance._scenarioInfo;
    }
    public static void SetDisplay(MissionDisplay display)
    {
        _instance._display = display;
    }

    private static void IncrementMission(MissionGoal goal, int count)
    {
        Debug.Log($"MissionManager.IncrementMission({goal}, {count})");
        if(_instance._missionLinks[(int)goal].Count == 0) return;
        
        foreach(LinkedMissionData linkedMission in _instance._missionLinks[(int)goal])
        {
            MissionData mission = linkedMission.missionData;
            if(mission.currentVal < mission.goalVal)
            {
                // Mission not completed yet
                mission.currentVal += count;
                Debug.Log($"INCREMENTING: {goal}");

                string text = $" -{mission.title} {mission.description} ({mission.currentVal}/{mission.goalVal})";
                _instance._display.AddOrEditMission(linkedMission.groupIndex, linkedMission.missionIndex, text);

                if(mission.currentVal >= mission.goalVal)
                {
                    // Mission gets completed
                    CheckMissionGroupCompletion();
                }
                else
                {
                    // Mission is still not completed
                }
            }
            else
            {
                // Mission already completed
                mission.currentVal += count;
                if(mission.currentVal >= mission.goalVal)
                {
                    // Mission is still completed
                }
                else
                {
                    // Mission becomes uncompleted
                }
            }
        }
    }
    private static void CheckMissionGroupCompletion()
    {
        // Check all mission groups
        int index = 0;
        foreach(MissionDataGroup group in _instance._activeMissionGroups)
        {
            bool completed = true;
            foreach(MissionData mission in group.missions)
            {
                if(mission.currentVal < mission.goalVal)
                {
                    completed = false;
                    break;
                }
            }
            if(completed)
            {
                // Handle completion of a mission group
                _instance._activeMissionGroups.Remove(group);
                foreach(MissionData mission in group.missions)
                {
                    // Unregister missions
                    for(int i=0; i<_instance._missionLinks[(int)mission.missionGoal].Count; i++)
                    {
                        LinkedMissionData linkedMission = _instance._missionLinks[(int)mission.missionGoal][i];
                        if(linkedMission.missionData == mission)
                        {
                            _instance._missionLinks[(int)mission.missionGoal].Remove(linkedMission);
                        }
                    }
                }
                if(_instance._scenarioInfo.mainMission == group)
                {
                    CompleteMainMission();
                    break;
                }
                MissionDataGroup nextGroup = _instance._scenarioInfo.missionsData[group.nextMissionId];
                nextGroup.currentVal++;
                if(nextGroup.currentVal >= nextGroup.previousLinkCount)
                {
                    RegisterMissionGroup(index);
                }
                break;
            }
            index++;
        }
    }
    private static void RegisterMissionGroup(int groupIndex)
    {
        MissionDataGroup group = _instance._scenarioInfo.missionsData[groupIndex];
        _instance._activeMissionGroups.Add(group);
        string text = $" {group.title} {group.description}";
        _instance._display.AddMissionGroup(groupIndex, text);
        
        for(int i=0; i<group.missions.Length; i++)
        {
            MissionData mission = group.missions[i];
            //Debug.Log($"TEST: _instance._missionLinks[(int)mission.missionGoal].Count: {_instance._missionLinks[(int)mission.missionGoal].Count}, (int)mission.missionGoal: {(int)mission.missionGoal}, _instance._missionLinks.Length: { _instance._missionLinks.Length}, mission: {mission!=null}");
            LinkedMissionData linkedMission = new LinkedMissionData(mission, groupIndex, i);
            _instance._missionLinks[(int)mission.missionGoal].Add(linkedMission);
            text = $" -{mission.title} {mission.description} ({mission.currentVal}/{mission.goalVal})";
            _instance._display.AddOrEditMission(groupIndex, i, text);
        }
    }
    private static void CompleteMainMission()
    {
        Debug.Log("MissionManager.CompleteMainMission()");
    }
}
