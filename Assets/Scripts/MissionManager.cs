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
    [SerializeField] private AudioEvent _winAudioEvent;
    private MissionDisplay _display;
    private int _mainMissionIndex = -1;
    private List<LinkedMissionData>[] _missionLinks;
    private List<MissionDataGroup> _activeMissionGroups;

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
        onIncrementMission = (goal, count) => IncrementMission(goal, count);
        //Events.onIncrementMission += IncrementMission;
    }

    public static void Init(ScenarioInfo scenarioInfo)
    {
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
            text = $" -{mission.title} ({mission.currentVal}/{mission.goalVal})";
            if(mission.description != "")
                text += $" <size=80%><br>   {mission.description}";
            textList.Add(text);

            LinkedMissionData linkedMission = new LinkedMissionData(mission, -1, ii);
            _instance._missionLinks[(int)mission.missionGoal].Add(linkedMission);
            ii++;
        }  
        _instance._display.Init(_instance._mainMissionIndex, textList); 

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

                string text = $" -{mission.title} ({mission.currentVal}/{mission.goalVal})";
                if(mission.description != "")
                    text += $" <size=80%><br>   {mission.description}";
                _instance._display.AddOrEditMission(linkedMission.groupIndex, linkedMission.missionIndex, text);

                if(mission.currentVal >= mission.goalVal)
                {
                    // Mission gets completed
                    _instance.StartCoroutine(_instance.CheckMissionGroupCompletion());
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
    IEnumerator CheckMissionGroupCompletion()
    {
        yield return new WaitForSeconds(0.1f);
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
                // Unregister current missions and mission group
                UnregisterMissionGroup(System.Array.IndexOf(_instance._scenarioInfo.missionsData, group));
                
                // Handle completion of main mission group
                if(_instance._scenarioInfo.mainMission == group)
                {
                    CompleteMainMission();
                    break;
                }
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
            MissionData mission = group.missions[i];
            //Debug.Log($"TEST: _instance._missionLinks[(int)mission.missionGoal].Count: {_instance._missionLinks[(int)mission.missionGoal].Count}, (int)mission.missionGoal: {(int)mission.missionGoal}, _instance._missionLinks.Length: { _instance._missionLinks.Length}, mission: {mission!=null}");
            LinkedMissionData linkedMission = new LinkedMissionData(mission, groupIndex, i);
            _instance._missionLinks[(int)mission.missionGoal].Add(linkedMission);
            text = $" -{mission.title} ({mission.currentVal}/{mission.goalVal})";
            if(mission.description != "")
                text += " <size=80%><br>   " + mission.description;
            _instance._display.AddOrEditMission(groupIndex, i, text);
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
    private static void CompleteMainMission()
    {
        Tools.PlayAudio(null, _instance._winAudioEvent);
        Debug.Log("MissionManager.CompleteMainMission()");
    }
}
