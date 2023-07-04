using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Resource { None, Food, Lumber, Magic }
public enum IdType { None, Building, Character, Job }
public enum Flag { None, TradeAvailable, TradingTimerEnabled, GrassTrees, GrassBuildings }

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private ScenarioInfo _scenarioInfo;
    [SerializeField] private int _mapSize = 128;
    public static int MapSize { get { return _instance._mapSize; } }
    private Dictionary<int, ObjectInfo> _objectInfoDict = new Dictionary<int, ObjectInfo>();

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
    private int _income;
    private float _growthSpeedPercent = 100f;
    private int _populationLimit;
    private float _traderSpeed;
    private List<Vector2Int> _flags;

    private int _objectId;
    private HashSet<int> _objectIds;
    private Dictionary<int, GameObject> _objectIdDictionary;
    private int _characterId;
    private HashSet<int> _characterIds;
    private Dictionary<int, GameObject> _characterIdDictionary;
    private Dictionary<VillagerType, int> _villagerCounts;
    private int _jobId;
    private HashSet<int> _jobIds;
    private Dictionary<int, Job> _jobIdDictionary;

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
        FinishedLoading = false;
        FinishedStartup = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            AddResource(Resource.Lumber, 20);
            AddResource(Resource.Food, 20);
            AddResource(Resource.Magic, 20);
        }
    }

    void Start()
    {
        _objectIds = new HashSet<int>();
        _objectIdDictionary = new Dictionary<int, GameObject>();
        
        _characterIds = new HashSet<int>();
        _characterIdDictionary = new Dictionary<int, GameObject>();
        _villagerCounts = new Dictionary<VillagerType, int>();
        foreach(VillagerType villagerType in VillagerType.GetValues(typeof(VillagerType)))
        {
            _villagerCounts.Add(villagerType, 0);
        }

        _jobIds = new HashSet<int>();
        _jobIdDictionary = new Dictionary<int, Job>();

        _flags = new List<Vector2Int>();

        StartCoroutine(Startup());  
    }
    IEnumerator Startup()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 1f;

        GameState game = SaveManager.GetData();
        _food = game.food;
        _lumber = game.lumber;
        _magic = game.magic;
        _objectId = game.objectId;
        _characterId = game.characterId;
        _jobId = game.jobId;
        yield return null;
        if(game.scenarioInfoId == -1)
        {
            MissionManager.Init(Instantiate(_scenarioInfo));
        }
        else 
        {
            MissionManager.Init(Instantiate(_scenarioInfo), game.mainMission, game.missionsData);
        }
        _traderSpeed = game.traderSpeed;
        _income = game.income;
        _flags = game.flags;
        yield return null;
        if(game.time.Count == 0) IngameUIHandler.InitTime();
        else
        {
            IngameUIHandler.InitTime(game.time[0], game.time[1], game.time[2], game.time[3], game.time[4]);
        }

        FinishedLoading = true;
        yield return new WaitForSeconds(0.4f);
        FinishedStartup = true;
        Debug.Log("GameManager.Startup(): Finished startup");
        Events.onSaveLoaded();

        LogDictionaries();
        StartCoroutine(Income());
        StartCoroutine(Autosave());
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
    IEnumerator Income()
    {
        // Run upkeep once per minute
        while(true)
        {
            yield return new WaitForSeconds(60f);

            AddResource(Resource.Magic, _income);
        }
    }
    IEnumerator Autosave()
    {
        while(true)
        {
            yield return new WaitForSeconds(SaveManager.GetAutosaveInterval());

            SaveManager.SaveData(SaveIdentifier.Auto);
        }
    }

    public static void DeleteDefaultObjects()
    {
        Destroy(_instance._initialBuildings);
        Destroy(_instance._initialCharacters);
    }

    // Functions for game data
    public static ScenarioInfo GetScenarioInfo()
    {
        return _instance._scenarioInfo;
    }
    public static ObjectInfo GetObjectInfo(int id)
    {
        if(_instance._objectInfoDict.Count == 0)
        {
            Object[] objectInfos = Resources.LoadAll("ScriptableObjects/ObjectInfo", typeof(ObjectInfo));
            foreach(ObjectInfo info in objectInfos)
            {
                _instance._objectInfoDict.Add(info.id, info);
            }
        }

        return _instance._objectInfoDict[id];
    }
    public static int GetResource(Resource resourceType)
    {
        int amount = 0;
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
        int oldValue, newValue;
        switch(resourceType)
        {
            case Resource.Food:
                oldValue = _instance._food;
                newValue = _instance._food + amount;
                _instance._food = newValue;
                Events.onFoodChange(oldValue, newValue);
                if(amount > 0) MissionManager.onIncrementMission(MissionGoal.Food, amount);
                break;
            case Resource.Lumber:
                oldValue = _instance._lumber;
                newValue = _instance._lumber + amount;
                _instance._lumber = newValue;
                Events.onLumberChange(oldValue, newValue);
                if(amount > 0) MissionManager.onIncrementMission(MissionGoal.Lumber, amount);
                break;
            case Resource.Magic:
                oldValue = _instance._magic;
                newValue = _instance._magic + amount;
                _instance._magic = newValue;
                Events.onMagicChange(oldValue, newValue);
                if(amount > 0) MissionManager.onIncrementMission(MissionGoal.Magic, amount);
                break;
            case Resource.None:
            default:
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
                if(!FinishedStartup)
                {
                    // Adjust villager counts for villagers whose data is generated on startup (initial villagers)
                    AdjustVillagerCount(obj.GetComponent<Villager>().GetVillagerType(), 1);
                }
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
                AdjustVillagerCount(obj.GetComponent<Villager>().GetVillagerType(), 1);
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
        //Debug.Log($"GameManager.RemoveId({idType}, {id}");
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
        Debug.Log($"GameManager.RemoveId({idType}, {id}, testing removal... {GetObjectById(idType, id)}");
    }
    public static GameObject GetObjectById(IdType idType, int id)
    {
        GameObject obj = null;
        switch(idType)
        {
            case IdType.Building:
                if(_instance._objectIdDictionary.ContainsKey(id)) 
                    obj = _instance._objectIdDictionary[id];
                break;
            case IdType.Character:
                if(_instance._characterIdDictionary.ContainsKey(id)) 
                    obj = _instance._characterIdDictionary[id];
                break;
        }
        if(obj == null) Debug.Log($"GameManager.GetObjectById({idType}, {id}) returning null");
        return obj;
    }
    public static Job GetJobById(int id)
    {
        if(!_instance._jobIdDictionary.ContainsKey(id))
        {
            Debug.Log($"GameManager.GetJobById({id})");
            
            foreach (int jobId in _instance._jobIds)
            {
                Debug.Log(  "Job id: " + jobId + 
                            ", target objec id: " + _instance._jobIdDictionary[id].targetObjectId +
                            ", worker: " + _instance._jobIdDictionary[id].workerId);
            }
        }
        return _instance._jobIdDictionary[id];
    }
    public static void UpdateJobInProgress(int id, bool val = true)
    {
        _instance._jobIdDictionary[id].inProgress = val;
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
        Debug.Log($"GameManager.AdjustVillagerCount({type}, {amount}): new count {_instance._villagerCounts[type]}");
        Events.onVillagerCountChange();
        MissionManager.onIncrementMission(MissionGoal.VillagerCount, GetVillagerCount());
    }
    public static int GetVillagerCount(VillagerType type = VillagerType.None)
    {
        if(type == VillagerType.None)
        {
            // Count all
            // _instance._characterIdDictionary.Count doesn't get updated fast enough
            int count = 0;
            foreach(VillagerType villagerType in VillagerType.GetValues(typeof(VillagerType)))
            {
                count += _instance._villagerCounts[villagerType];
            }
            return count;
        }
        else return _instance._villagerCounts[type];
    }
    public static void AdjustGrowthMultiplier(float val)
    {
        Debug.Log($"GameManager.AdjustGrowthMultiplier({val})");
        float newValue = _instance._growthSpeedPercent + val;
        Events.onGrowthModChange(_instance._growthSpeedPercent, newValue);
        _instance._growthSpeedPercent = newValue;
        MissionManager.onIncrementMission(MissionGoal.GrowthPercent, (int)newValue);
    }
    public static float GetGrowthValue()
    {
        return _instance._growthSpeedPercent;
    }
    public static float GetGrowthMultiplier()
    {
        // Turn percent increase into a simple multiplier, avoid lower speed than 40%
        float val = _instance._growthSpeedPercent > 40f ? _instance._growthSpeedPercent : 40f;
        return (100f / val);
    }
    public static void AdjustPopulationLimit(int val)
    {
        Debug.Log($"GameManager.AdjustPopulationLimit({val})");
        int newValue = _instance._populationLimit + val;
        _instance._populationLimit = newValue;
        Events.onPopLimitChange();
        MissionManager.onIncrementMission(MissionGoal.PopulationLimit, _instance._populationLimit);
    }
    public static int GetPopulationLimit()
    {
        return _instance._populationLimit;
    }
    public static void AdjustTraderSpeed(float val)
    {
        Debug.Log($"GameManager.AdjustTraderSpeed({val})");
        float newValue = _instance._traderSpeed + val;
        Events.onTraderSpeedChange(_instance._traderSpeed, newValue);
        _instance._traderSpeed = newValue;
    }
    public static float GetTraderSpeed()
    {
        return _instance._traderSpeed;
    }
    public static void AdjustIncome(int val)
    {
        Debug.Log($"GameManager.AdjustIncome({val})");
        _instance._income += val;
    }
    public static int GetIncome()
    {
        return _instance._income;
    }
    public static void SetFlag(Flag flag)
    {
        if(flag == Flag.None) return;

        Debug.Log($"GameManager.SetFlag({flag})");
        foreach(Vector2Int pair in _instance._flags)
        {
            if(pair.x == (int)flag)
            {
                if(pair.y == 1) return;
                _instance._flags.Remove(pair);
                break;
            }
        }
        _instance._flags.Add(new Vector2Int((int)flag, 1));
        Events.onFlagTriggered(flag);
    }
    public static bool GetFlag(Flag flag)
    {
        if(flag == Flag.None) return true;

        foreach(Vector2Int pair in _instance._flags)
        {
            if(pair.x == (int)flag)
            {
                return pair.y == 1 ? true : false;
            }
        }
        return false;
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
    public static void CreateVillager(GameObject villagerPrefab)
    {
        GameObject villagerObject = Instantiate(villagerPrefab, Characters.transform);

        Villager villager = villagerObject.GetComponent<Villager>();
        VillagerType villagerType = villager.GetVillagerType();
        int colorId = GetVillagerCount(villagerType);

        villager.SetColor(colorId);
        AdjustVillagerCount(villagerType, 1);
        
        Debug.Log($"GameManager.CreateVillager(), pop: {GameManager.GetVillagerCount()}/{GameManager.GetPopulationLimit()}");
    }
}
