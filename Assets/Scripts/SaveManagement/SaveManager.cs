using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public enum SaveIdentifier {None, Auto, First, Second, Third}

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    private GameState _saveData;
    [SerializeField] private float _autosaveInterval = 80f;

    void Awake()
    {
        if(_instance == null) _instance = this;
        else
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this.gameObject);

        //Debug.Log("SaveManager, data location: '" + Application.persistentDataPath + "'");
        _saveData = new GameState();
    }

    public static GameState GetData(SaveIdentifier saveIdentifier = SaveIdentifier.None)
    {
        // Get data for currently loaded save by default
        if(saveIdentifier == SaveIdentifier.None) return _instance._saveData;

        // Else, fetch data from a file
        GameState data = new GameState();
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameState));
        
            FileStream stream = new FileStream( _instance.GetSavePath(saveIdentifier), 
                                                FileMode.Open, FileAccess.Read);
            data = serializer.Deserialize(stream) as GameState;
            stream.Close();
        }
        catch(FileNotFoundException e)
        {
            // This is fine
        }
        
        return data;
    }

    public static void LoadData(SaveIdentifier saveIdentifier)
    {
        // Load data from XML file
        //Debug.Log($"SaveManager, LoadData(): loading save data '{_instance.GetSavePath(saveIdentifier)}'...");
        XmlSerializer serializer = new XmlSerializer(typeof(GameState));
        
        FileStream stream = new FileStream( _instance.GetSavePath(saveIdentifier), 
                                            FileMode.Open, FileAccess.Read);
        _instance._saveData = serializer.Deserialize(stream) as GameState;

        stream.Close();

        // Adjust game world based on save data loaded
        GameManager.DeleteDefaultObjects();

        foreach(CharacterSaveData data in _instance._saveData.characterSaveData)
        {
            GameObject character = Instantiate(Resources.Load(
                    "Prefabs/Characters/" + data.prefabName), 
                    data.location, 
                    data.rotation,
                    GameManager.Characters.transform) 
                    as GameObject;
            character.GetComponent<Villager>().Init(data, 0.1f);
        }

        foreach(BuildingSaveData data in _instance._saveData.buildingSaveData)
        {
            //Debug.Log("loading... " + data.prefabName);
            GameObject building = Instantiate(Resources.Load(
                    "Prefabs/Buildings/" + data.prefabName), 
                    data.location, 
                    data.rotation,
                    GameManager.Buildings.transform) 
                    as GameObject;
            building.GetComponent<PlaceableObject>().Init(data, 0.1f);
        }

        foreach(Job job in _instance._saveData.jobSaveData)
        {
            JobManager.QueueJob(job, false);
        }

        GrassSystem.LoadGrassTiles( _instance._saveData.partialGrassTiles, 
                                    _instance._saveData.fullGrassTiles);
                                    

        //Debug.Log("SaveManager, LoadData(): Save loaded!");
    }

    public static void SaveData(SaveIdentifier saveIdentifier)
    {
        // Update save data to contain current game state
        _instance._saveData.food = GameManager.GetResource(Resource.Food);
        _instance._saveData.lumber = GameManager.GetResource(Resource.Lumber);
        _instance._saveData.magic = GameManager.GetResource(Resource.Magic);
        _instance._saveData.objectId = GameManager.GetRunningId(IdType.Building);
        _instance._saveData.characterId = GameManager.GetRunningId(IdType.Character);
        _instance._saveData.jobId = GameManager.GetRunningId(IdType.Job);
        _instance._saveData.traderSpeed = GameManager.GetTraderSpeed();
        _instance._saveData.income = GameManager.GetIncome();
        //_instance._saveData.scenarioInfoId = MissionManager.GetScenarioInfo();

        _instance._saveData.characterSaveData = new List<CharacterSaveData>();
        foreach (int id in GameManager.GetIds(IdType.Character)){
            GameObject obj = GameManager.GetObjectById(IdType.Character, id);
            if(obj != null)
            {
                CharacterSaveData newData = obj.GetComponent<Villager>().FormSaveData();
                _instance._saveData.characterSaveData.Add(newData);
            }
        }

        _instance._saveData.buildingSaveData = new List<BuildingSaveData>();
        foreach (int id in GameManager.GetIds(IdType.Building)){
            GameObject obj = GameManager.GetObjectById(IdType.Building, id);
            if(obj != null)
            {
                BuildingSaveData newData = obj.GetComponent<PlaceableObject>().FormSaveData();
                _instance._saveData.buildingSaveData.Add(newData);
            }
        }

        _instance._saveData.jobSaveData = new List<Job>();
        foreach (int id in GameManager.GetIds(IdType.Job)){
            Job job = GameManager.GetJobById(id);
            _instance._saveData.jobSaveData.Add(job);
        }

        
        _instance._saveData.partialGrassTiles = GrassSystem.GetPartialGrassTiles();
        _instance._saveData.fullGrassTiles = GrassSystem.GetFullGrassTiles();

        ScenarioInfo scenarioInfo = MissionManager.GetScenarioInfo();
        _instance._saveData.scenarioInfoId = scenarioInfo.id;
        _instance._saveData.mainMission = scenarioInfo.mainMission;
        _instance._saveData.missionsData = scenarioInfo.missionsData;

        _instance._saveData.time = IngameUIHandler.GetTime();
        
        _instance._saveData.isSave = true;

        // Write save data to XML file
        //Debug.Log($"SaveManager, SaveData(): Writing save '{_instance.GetSavePath(saveIdentifier)}'...");
        XmlSerializer serializer = new XmlSerializer(typeof(GameState));
        
        FileStream stream = new FileStream( _instance.GetSavePath(saveIdentifier), 
                                            FileMode.Create);
        serializer.Serialize(stream, _instance._saveData);

        stream.Close();
        //Debug.Log("SaveManager, SaveData(): Save written!");
    }

    public string GetSavePath(SaveIdentifier saveIdentifier)
    {
        string savePath = "";
        if(saveIdentifier == SaveIdentifier.Auto)
        {
            savePath = System.IO.Path.Combine(Application.persistentDataPath, "autosave.xml");
        }
        else
        {
            string identifier = "save " + (((int)saveIdentifier)-1) + ".xml";
            savePath = System.IO.Path.Combine(Application.persistentDataPath, identifier);
        }
        return savePath;
    }

    public static float GetAutosaveInterval()
    {
        return _instance._autosaveInterval;
    }
}
