using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Resource { None, Food, Lumber, Magic }
public enum IdType { None, Building, Character, Job }
public enum VillagerType { None, Elf, Goblin }

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private ScenarioInfo _scenarioInfo;
    [SerializeField] private int _mapSize = 128;
    public static int MapSize { get { return _instance._mapSize; } }

    public static bool FinishedLoading { get; protected set; }
    public static bool FinishedStartup { get; protected set; }

    [SerializeField] private GameObject _initialBuildings;
    public static GameObject InitialBuildings { get { return _instance._initialBuildings; } }
    [SerializeField] private GameObject _buildings;
    public static GameObject Buildings { get { return _instance._buildings; } }
    [SerializeField] private GameObject _initialCharacters;
    public static GameObject InitialCharacters { get { return _instance._initialCharacters; } }
    [SerializeField] private GameObject _characters;
    public static GameObject Characters { get { return _instance._characters; } }


    private int _food;
    private int _lumber;
    private int _magic;
    private float _growthSpeedPercent = 100f;
    private int _populationLimit;

    private int _objectId;
    private HashSet<int> _objectIds;
    private Dictionary<int, GameObject> _objectIdDictionary;
    private int _characterId;
    private HashSet<int> _characterIds;
    private Dictionary<int, GameObject> _characterIdDictionary;
    private int _jobId;
    private HashSet<int> _jobIds;
    private Dictionary<int, Job> _jobIdDictionary;
    private Dictionary<VillagerType, int> _villagerCounts;

    private float _currentGameSpeed = 1f;
    private float _previousGameSpeed = 1f;

    void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
        }
        FinishedStartup = false;
    }

    void Start()
    {
        _objectIds = new HashSet<int>();
        _objectIdDictionary = new Dictionary<int, GameObject>();
        
        _characterIds = new HashSet<int>();
        _characterIdDictionary = new Dictionary<int, GameObject>();

        _jobIds = new HashSet<int>();
        _jobIdDictionary = new Dictionary<int, Job>();

        StartCoroutine(Startup());  
    }
    IEnumerator Startup()
    {
        yield return null;

        Time.timeScale = 1f;

        GameState game = SaveManager.GetData();
        _food = game.food;
        _lumber = game.lumber;
        _magic = game.magic;
        _objectId = game.objectId;
        _characterId = game.characterId;
        _jobId = game.jobId;
        if(game.scenarioInfo == null)
        {
            MissionManager.Init(Instantiate(_scenarioInfo));
        }
        else MissionManager.Init(Instantiate(game.scenarioInfo));

        FinishedLoading = true;
        yield return new WaitForSeconds(0.4f);
        FinishedStartup = true;
        Events.onSaveLoaded();

        LogDictionaries();
    }
    public static void LogDictionaries()
    {
        Debug.Log("GameManager.LogDictionaries()");
        foreach (int id in _instance._objectIds)
        {
            Debug.Log("Object id: " + id + ", val: " + _instance._objectIdDictionary[id]);
        }

        foreach (int id in _instance._characterIds)
        {
            Debug.Log("Character id: " + id + ", val: " + _instance._characterIdDictionary[id]);
        }

        foreach (int id in _instance._jobIds)
        {
            Debug.Log(  "Job id: " + id + 
                        ", target objec id: " + _instance._jobIdDictionary[id].targetObjectId +
                        ", worker: " + _instance._jobIdDictionary[id].workerId);
        }
    }

    public static void DeleteDefaultObjects()
    {
        Destroy(_instance._initialBuildings);
        Destroy(_instance._initialCharacters);
    }

    // Functions for game data
    public static int GetResource(Resource resourceType)
    {
        int amount = -1;
        switch(resourceType)
        {
            case Resource.Food:
                amount =  _instance._food;
                break;
            case Resource.Lumber:
                amount = _instance._lumber;
                break;
            case Resource.Magic:
                amount = _instance._magic;
                break;
        }
        return amount;
    }
    
    public static void AddResource(Resource resourceType, int amount)
    {
        int newValue;
        switch(resourceType)
        {
            case Resource.Food:
                newValue = _instance._food + amount;
                Events.onFoodChange(_instance._food, newValue);
                if(newValue > 0) MissionManager.onIncrementMission(MissionGoal.Food, newValue);
                _instance._food = newValue;
                break;
            case Resource.Lumber:
                newValue = _instance._lumber + amount;
                Events.onLumberChange(_instance._lumber, newValue);
                if(newValue > 0) MissionManager.onIncrementMission(MissionGoal.Lumber, newValue);
                _instance._lumber = newValue;
                break;
            case Resource.Magic:
                newValue = _instance._magic + amount;
                Events.onMagicChange(_instance._magic, newValue);
                if(newValue > 0) MissionManager.onIncrementMission(MissionGoal.Magic, newValue);
                _instance._magic = newValue;
                break;
        }
    }
    public static bool TryPay(Cost[] costs)
    {
        bool val = true;
        foreach(Cost cost in costs)
        {
            if(GetResource(cost.type) < cost.amount) val = false;
        }
        if(val) foreach(Cost cost in costs)
        {
            GameManager.AddResource(cost.type, -cost.amount);
        }
        else
        {
            Debug.Log($"TryPay() exeeded available resources");
        }

        return val;
    }

    public static int GenerateId(IdType idType, GameObject obj = null, Job job = null)
    {
        int newId = -1;
        switch(idType)
        {
            case IdType.Building:
                newId = ++_instance._objectId;
                _instance._objectIds.Add(newId);
                _instance._objectIdDictionary.Add(newId, obj);
                break;
            case IdType.Character:
                newId = ++_instance._characterId;
                _instance._characterIds.Add(newId);
                _instance._characterIdDictionary.Add(newId, obj);
                break;
            case IdType.Job:
                newId = ++_instance._jobId;
                _instance._jobIds.Add(newId);
                _instance._jobIdDictionary.Add(newId, job);
                break;
            default:
                break;
        }
        if(obj != null) Debug.Log($"GameManager.GenerateId({idType}) new id({newId}) for {obj.name}");
        else if(job != null) Debug.Log($"GameManager.GenerateId({idType}) new id({newId}) for {job.jobType}");
        else Debug.Log($"GameManager.GenerateId({idType}) new id({newId}) for NULL");
        return newId;
    }
    public static void AddId(IdType idType, int id, GameObject obj = null, Job job = null)
    {
        if(obj != null) Debug.Log($"GameManager.AddId({idType}) new id({id}) for {obj.name}");
        else if(job != null) Debug.Log($"GameManager.AddId({idType}) new id({id}) for {job.jobType}");
        else Debug.Log($"GameManager.AddId({idType}) new id({id}) for NULL");

        switch(idType)
        {
            case IdType.Building:
                if(_instance._objectId <= id)
                {
                    _instance._objectId = id + 1;
                }
                _instance._objectIds.Add(id);
                _instance._objectIdDictionary.Add(id, obj);
                break;
            case IdType.Character:
                if(_instance._characterId <= id)
                {
                    _instance._characterId = id + 1;
                }
                _instance._characterIds.Add(id);
                _instance._characterIdDictionary.Add(id, obj);
                break;
            case IdType.Job:
                if(_instance._jobId <= id)
                {
                    _instance._jobId = id + 1;
                }
                _instance._jobIds.Add(id);
                _instance._jobIdDictionary.Add(id, job);
                break;
            default:
                break;
        }
    }
    public static void RemoveId(IdType idType, int id)
    {
        Debug.Log($"GameManager.RemoveId({idType}, {id}");
        switch(idType)
        {
            case IdType.Building:
                GameObject obj = _instance._objectIdDictionary[id];
                _instance._objectIds.Remove(id);
                _instance._objectIdDictionary.Remove(id);
                break;
            case IdType.Character:
                _instance._characterIds.Remove(id);
                _instance._characterIdDictionary.Remove(id);
                break;
            case IdType.Job:
                _instance._jobIds.Remove(id);
                _instance._jobIdDictionary.Remove(id);
                break;
            default:
                break;
        }
    }
    public static GameObject GetObjectById(IdType idType, int id)
    {
        GameObject obj = null;
        switch(idType)
        {
            case IdType.Building:
                obj = _instance._objectIdDictionary[id];
                break;
            case IdType.Character:
                obj = _instance._characterIdDictionary[id];
                break;
        }
        if(obj == null) Debug.Log("GameManager.GetObjectById returning null");
        return obj;
    }
    public static Job GetJobById(int id)
    {
        return _instance._jobIdDictionary[id];
    }
    public static HashSet<int> GetIds(IdType idType)
    {
        switch(idType)
        {
            case IdType.Building:
                return _instance._objectIds;
            case IdType.Character:
                return _instance._characterIds;
            case IdType.Job:
                return _instance._jobIds;
            default:
                return new HashSet<int>();
        }
    }
    public static int GetRunningId(IdType idType)
    {
        switch(idType)
        {
            case IdType.Building:
                return _instance._objectId;
            case IdType.Character:
                return _instance._characterId;
            case IdType.Job:
                return _instance._jobId;
            default:
                return -1;
        }
    }
    public static void AdjustVillagerCount(VillagerType type, int amount)
    {
        _instance._villagerCounts[type] += amount;
        Events.onVillagerCountChange();
        if(amount > 0) MissionManager.onIncrementMission(MissionGoal.NewWorker, amount);
    }
    public static int GetVillagerCount(VillagerType type = VillagerType.None)
    {
        if(type == VillagerType.None)
        {
            // Count all
            return _instance._characterIdDictionary.Count;
        }
        else return _instance._villagerCounts[type];
    }
    public static void AdjustGrowthMultiplier(float val)
    {
        Debug.Log($"GameManager.AdjustGrowthMultiplier({val})");
        float newValue = _instance._growthSpeedPercent + val;
        Events.onGrowthModChange(_instance._growthSpeedPercent, newValue);
        _instance._growthSpeedPercent = newValue;
    }
    public static float GetGrowthValue()
    {
        return _instance._growthSpeedPercent;
    }
    public static float GetGrowthMultiplier()
    {
        // Turn percent increase into a simple multiplier
        return (100f / _instance._growthSpeedPercent);
    }
    public static void AdjustPopulationLimit(int val)
    {
        Debug.Log($"GameManager.AdjustPopulationLimit({val})");
        int newValue = _instance._populationLimit + val;
        Events.onPopLimitChange(_instance._populationLimit, newValue);
        _instance._populationLimit = newValue;
    }
    public static int GetPopulationLimit()
    {
        return _instance._populationLimit;
    }

    public static void SetGameSpeed(float newSpeed)
    {
        _instance._previousGameSpeed = _instance._currentGameSpeed;
        _instance._currentGameSpeed = newSpeed;
        Time.timeScale = newSpeed;
        Debug.Log($"GameManager: SetGameSpeed({newSpeed})");
        Events.onGameSpeedChange();
    }
    public static void SetGameSpeedToPrevious()
    {
        SetGameSpeed(_instance._previousGameSpeed);
    }
}
