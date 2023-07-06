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
    [SerializeField] private AudioEvent _missionAudioEvent;
    [SerializeField] private GameObject _winScreen;
    private MissionDisplay _display;
    public static int MAIN_MISSION_INDEX = -1;
    private List<LinkedMissionData>[] _missionLinks;
    private List<MissionDataGroup> _activeMissionGroups;
    private bool _needCheckForCompletion;

    public static System.Action<MissionGoal, int> onIncrementMission = delegate { };
    
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
    }

    public static void Init(ScenarioInfo scenarioInfo, 
                            MissionDataGroup mainMission = null, MissionDataGroup[] missionsData = null)
    {
        if(mainMission != null && missionsData != null)
        {
            scenarioInfo.mainMission = mainMission;
            scenarioInfo.missionsData = missionsData;
        }
        _instance._scenarioInfo = scenarioInfo;

        // Manage the main mission for the scenario
        List<string> textList = new List<string>();
        string text = _instance._scenarioInfo.mainMission.title;
        if(_instance._scenarioInfo.mainMission.description != "")
            text += " <size=80%><br> " + _instance._scenarioInfo.mainMission.description;
        textList.Add(text);

        int ii = 0;
        foreach(MissionData mission in _instance._scenarioInfo.mainMission.missions)
        {
            // Make initial updates to mission values if needed
            MissionData updatedMission = UpdateMission(mission);
            
            // Make mission text
            text = $" -{mission.title} ({updatedMission.currentVal}/{updatedMission.goalVal})";
            if(updatedMission.description != "")
                text += $" <size=80%><br>   {updatedMission.description}";
            textList.Add(text);

            // Make mission link data
            LinkedMissionData linkedMission = new LinkedMissionData(updatedMission, -1, ii);
            _instance._missionLinks[(int)updatedMission.missionGoal].Add(linkedMission);
            ii++;
        }  
        _instance._display.Init(MAIN_MISSION_INDEX, textList); 

        // Handle the current missions
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
        // Event subscriptions
        onIncrementMission = delegate { };
        onIncrementMission = (goal, count) => IncrementMission(goal, count);
        Events.onFlagTriggered += IncrementMissionFlag;
    }

    public static ScenarioInfo GetScenarioInfo()
    {
        return _instance._scenarioInfo;
    }
    public static void SetDisplay(MissionDisplay display)
    {
        _instance._display = display;
    }

    private static void IncrementMissionFlag(Flag flag)
    {
        IncrementMission(MissionGoal.TriggerFlag, (int)flag);
    }
    private static void IncrementMission(MissionGoal goal, int val)
    {
        if(!GameManager.FinishedStartup) return;
        //Debug.Log($"MissionManager.IncrementMission({goal}, {val})");
        if(_instance._missionLinks[(int)goal].Count == 0) return;
        
        // Check all missions with the goal that was incremented
        foreach(LinkedMissionData linkedMission in _instance._missionLinks[(int)goal])
        {
            MissionData mission = linkedMission.missionData;

            if(mission.currentVal >= mission.goalVal) continue; // Already completed
            
            
            if(mission.missionGoalId == 0)
            {
                // No additional mission specifier, adjust value

                if(goal == MissionGoal.TreeCount || goal == MissionGoal.VillagerCount 
                    || goal == MissionGoal.PopulationLimit || goal == MissionGoal.GrowthPercent)
                {
                    // Update whole value
                    mission.currentVal = val;
                    if(linkedMission.groupIndex == MAIN_MISSION_INDEX)
                    {
                        _instance._scenarioInfo.mainMission.missions[linkedMission.missionIndex]
                            .currentVal = val;
                    }
                    //else _instance._scenarioInfo.missionsData[linkedMission.groupIndex].missions[linkedMission.missionIndex].currentVal = val;
                }
                else
                {
                    // Increment by val only
                    mission.currentVal += val;
                    if(linkedMission.groupIndex == MAIN_MISSION_INDEX)
                    {
                        _instance._scenarioInfo.mainMission.missions[linkedMission.missionIndex]
                            .currentVal += val;
                    }
                    //else _instance._scenarioInfo.missionsData[linkedMission.groupIndex].missions[linkedMission.missionIndex].currentVal += val;
                }
            }
            else
            {
                // Additional mission specifier, only increment if val matches
                if(mission.missionGoalId == val)
                {
                    // Adjust mission by 1
                    mission.currentVal += 1;
                    if(linkedMission.groupIndex == MAIN_MISSION_INDEX)
                    {
                        _instance._scenarioInfo.mainMission.missions[linkedMission.missionIndex]
                            .currentVal += 1;
                    }
                    //else _instance._scenarioInfo.missionsData[linkedMission.groupIndex].missions[linkedMission.missionIndex].currentVal += 1;
                }
                else
                {
                    // Do not increment, skip rest
                    continue;
                }
            }
            //Debug.Log($"INCREMENTING: {goal}");

            string text = $" -{mission.title} ({mission.currentVal}/{mission.goalVal})";
            if(mission.description != "")
                text += $" <size=80%><br>   {mission.description}";
            _instance._display.AddOrEditMission(linkedMission.groupIndex, linkedMission.missionIndex, text);

            // Check if mission is now completed
            if(mission.currentVal >= mission.goalVal)
            {
                if(linkedMission.groupIndex == MAIN_MISSION_INDEX)
                {
                    CheckMainMissionGroupCompletion();
                }
                else
                {
                    GameManager.AddResource(mission.rewardType, mission.rewardVal);
                    _instance._needCheckForCompletion = true;
                    _instance.StartCoroutine(_instance.CheckMissionGroupCompletion());
                }
            }
            else
            {
                // Mission is still not completed
            }
        }
    }
    private static void CheckMainMissionGroupCompletion()
    {
        //Debug.Log("MissionManager.CheckMainMissionGroupCompletion()");
        foreach(MissionData mission in _instance._scenarioInfo.mainMission.missions)
        {
            if(mission.currentVal < mission.goalVal) return;
        }
        
        Instantiate(_instance._winScreen);
        //Debug.Log("MissionManager: Game over");
    }
    IEnumerator CheckMissionGroupCompletion()
    {
        // Avoid repeated calls causing unnecessary execution
        yield return new WaitForSeconds(0.1f);
        if(!_instance._needCheckForCompletion) yield break;
        _needCheckForCompletion = false;

        // Check all active mission groups
        foreach(MissionDataGroup group in _instance._activeMissionGroups)
        {
            bool completed = true;
            if(completed) 
                foreach(MissionData mission in group.missions)
                {
                    if(mission.currentVal < mission.goalVal)
                    {
                        completed = false;
                        break;
                    }
                }
            if(completed)   // Handle completion of a mission group
            {
                // Give reward
                GameManager.SetFlag(group.rewardFlag);
                // Unregister current missions and mission group
                UnregisterMissionGroup(System.Array.IndexOf(_instance._scenarioInfo.missionsData, group));
                
                // Handle completion of regular mission group
                Tools.PlayAudio(null, _instance._missionAudioEvent);
                MessageLog.NewMessage(new MessageData(
                    $"Completed mission: '{group.title}'!", MessageType.Progress));

                // Pick up the next mission group
                MissionDataGroup nextGroup = _instance._scenarioInfo.missionsData[group.nextMissionId];
                nextGroup.currentVal++;
                if(nextGroup.currentVal >= nextGroup.previousLinkCount)
                {
                    RegisterMissionGroup(group.nextMissionId);
                }
                break;
            }
        }
    }
    private static void RegisterMissionGroup(int groupIndex)
    {
        MissionDataGroup group = _instance._scenarioInfo.missionsData[groupIndex];
        _instance._activeMissionGroups.Add(group);
        string text = $"{group.title}";
        if(group.description != "")
            text += " <size=80%><br>   " + group.description;
        _instance._display.AddMissionGroup(groupIndex, text);
        MessageLog.NewMessage(new MessageData(
            $"New mission: '{group.title}'!", MessageType.Progress));
        
        // Register missions
        for(int i=0; i<group.missions.Length; i++)
        {
            MissionData mission = UpdateMission(group.missions[i]);
            //Debug.Log($"TEST: _instance._missionLinks[(int)mission.missionGoal].Count: {_instance._missionLinks[(int)mission.missionGoal].Count}, (int)mission.missionGoal: {(int)mission.missionGoal}, _instance._missionLinks.Length: { _instance._missionLinks.Length}, mission: {mission!=null}");
            LinkedMissionData linkedMission = new LinkedMissionData(mission, groupIndex, i);
            _instance._missionLinks[(int)mission.missionGoal].Add(linkedMission);
            text = $" -{mission.title} ({mission.currentVal}/{mission.goalVal})";
            if(mission.description != "")
                text += " <size=80%><br>   " + mission.description;
            _instance._display.AddOrEditMission(groupIndex, i, text);

            // Check if mission is already completed
            if(mission.missionGoal == MissionGoal.BuyFromTrader || mission.missionGoal == MissionGoal.TriggerFlag)
            {
                if(GameManager.GetFlag((Flag)mission.missionGoalId))
                {
                    IncrementMission(mission.missionGoal, mission.missionGoalId);
                }
            }
        }
    }
    private static void UnregisterMissionGroup(int groupIndex)
    {
        MissionDataGroup group = _instance._scenarioInfo.missionsData[groupIndex];
        _instance._activeMissionGroups.Remove(group);
        _instance._display.RemoveMissionGroup(groupIndex);

        // Unregister missions
        foreach(MissionData mission in group.missions)
        {
            for(int i=0; i<_instance._missionLinks[(int)mission.missionGoal].Count; i++)
            {
                LinkedMissionData linkedMission = _instance._missionLinks[(int)mission.missionGoal][i];
                if(linkedMission.missionData == mission)
                {
                    _instance._missionLinks[(int)mission.missionGoal].Remove(linkedMission);
                }
            }
        }
    }

    private static MissionData UpdateMission(MissionData mission)
    {
        if(!GameManager.FinishedStartup) return mission; // Too early to receive updated info

        switch(mission.missionGoal)
            {
            case MissionGoal.TreeCount:
                mission.currentVal = BuildingSystem.GetTreeCount();
                break;
            case MissionGoal.VillagerCount:
                mission.currentVal = GameManager.GetVillagerCount();
                break;
            case MissionGoal.PopulationLimit:
                mission.currentVal = GameManager.GetPopulationLimit();
                break;
            case MissionGoal.GrowthPercent:
                mission.currentVal = (int)GameManager.GetGrowthValue();
                break;
            default:
                break;
        }
        return mission;
    }     
}
