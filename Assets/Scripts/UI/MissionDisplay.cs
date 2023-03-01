using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionDisplay : MonoBehaviour
{
    private Transform _mainMissionGroup;
    private List<GameObject> _missionGroup;

    [SerializeField] private GameObject _missionGroupPrefab;

    private Dictionary<int, int> _groupIndexToActualIndex;
    // private int mainMissionIndex = -1;

    void Start()
    {
        // Prepaire display
        int childCount = 0;
        foreach (Transform child in this.gameObject.transform) childCount++;
        for(int i=childCount-1; i>=0; i--)
        {
            if(i == 0)
            {
                _mainMissionGroup = this.gameObject.transform.GetChild(0);
            }
            else
            {
                Destroy(this.gameObject.transform.GetChild(i).gameObject);
            }
        }
        _missionGroup = new List<GameObject>();
        _groupIndexToActualIndex = new Dictionary<int, int>();
        MissionManager.SetDisplay(this);
    }
    public void Init(int groupIndex, List<string> text)
    {
        // Add main mission
        _groupIndexToActualIndex.Add(groupIndex, 0);
        _missionGroup.Add(_mainMissionGroup.gameObject);
        _mainMissionGroup.GetChild(0).GetComponent<TextMeshProUGUI>().text = text[0];
        _mainMissionGroup.GetChild(1).GetComponent<TextMeshProUGUI>().text = text[1];
        for(int i=2; i<text.Count; i++)
        {
            AddMission(groupIndex, text[i]);
        }
    }

    public void AddMissionGroup(int index, string message)
    {
        GameObject newGroup = Instantiate(_missionGroupPrefab, this.gameObject.transform);
        newGroup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        _groupIndexToActualIndex.Add(index, _missionGroup.Count);
        _missionGroup.Add(newGroup);
    }
    public void RemoveMissionGroup(int index)
    {
        int adjustedIndex = _groupIndexToActualIndex[index];
        _groupIndexToActualIndex.Remove(index);
        Destroy(_missionGroup[adjustedIndex]);

        Dictionary<int, int> newDict = new Dictionary<int, int>();
        foreach(KeyValuePair<int, int> entry in _groupIndexToActualIndex)
        {
            if(entry.Value > adjustedIndex)
            {
                newDict.Add(entry.Key, entry.Value - 1);
            }
            else newDict.Add(entry.Key, entry.Value);
        }
        _groupIndexToActualIndex = newDict;
    }
    public void AddMission(int groupIndex, string message)
    {
        // Copy a mission gameObject
        GameObject group = _missionGroup[_groupIndexToActualIndex[groupIndex]];
        GameObject newMission = Instantiate(group.transform.GetChild(1).gameObject, group.transform);
        // Write message
        newMission.GetComponent<TextMeshProUGUI>().text = message;
    }
    public void AddOrEditMission(int groupIndex, int missionIndex, string message)
    {
        GameObject group = _missionGroup[_groupIndexToActualIndex[groupIndex]];

        if(group == null)
        {
            Debug.Log($"MissionDisplay.AddOrEditMission({groupIndex}, {missionIndex}, {message}) ERROR: group == null");
            return;
        }

        int childCount = 0;
        foreach (Transform child in group.transform) childCount++;
        if(missionIndex+1 >= childCount)
        {
            // Index too high, must add new mission insteads
            AddMission(groupIndex, message);
        }
        else group.transform.GetChild(missionIndex+1).gameObject.
            GetComponent<TextMeshProUGUI>().text = message;
    }
}
