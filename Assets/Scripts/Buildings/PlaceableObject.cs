using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Cost
{
    public int amount;
    public Resource type;

    public Cost(int amount, Resource type)
    {
        this.amount = amount;
        this.type = type;
    }
}

// Handle building placement and form the basis of specialized building scripts
public class PlaceableObject : MonoBehaviour
{
    //[HideInInspector] 
    public int buildingId = -1;
    protected bool _initialized = false;
    public string Name 
    { 
        get 
        { 
            if(objectInfo != null)
            {
                return objectInfo.name;
            }
            else return gameObject.name; 
        } 
    }
    [SerializeField] protected ObjectInfo objectInfo;
    public Cost[] BuildingCost;
    public bool Placeable { get; protected set; }
    public bool Placed { get; protected set; }
    public Vector3Int Size { get; protected set; }
    protected Vector3Int _startTile;
    protected Vector3[] _vertices;

    [SerializeField] protected bool _requireGrowth = true;
    [SerializeField] protected float _growTime = 1f;
    [SerializeField] protected float _growthTics = 1f;
    protected Vector3 _originalScale;
    [SerializeField] protected float _growthProgress = 0f;
    protected float _ticSize;
    [SerializeField] protected bool _requireConstruction = false;
    [SerializeField] private GameObject _constructionObject;
    [SerializeField] protected float _constructionTime = 1f;
    [SerializeField] protected float _constructionDistance = 6.3f;
    protected bool _finishedConstruction = false;

    protected int _cutDownjobIndex;
    protected int _jobIndex;
    [SerializeField] protected bool _cuttable = true;
    protected bool _cutByDefault = false;
    [SerializeField] protected Resource _cutDownResourceType = Resource.Lumber;
    [SerializeField] protected int _lumberValue = 1;
    [SerializeField] protected float _woodCuttingTime = 5f;
    [SerializeField] protected float _woodCuttingDistance = 6.3f;
    [SerializeField] protected BuildJobType _deconstructType = BuildJobType.Cut;
    [SerializeField] protected float _growthSpeedIncreasePercent = 0f;
    public float GrowthSpeedIncreasePercent { get; protected set; }
    [SerializeField] protected bool _spawnGrass = false;
    [SerializeField] protected bool _requireGrass = true;
    public bool RequireGrass { get { return _requireGrass; } }


    void Start()
    {
        Placeable = false;

        if(!GameManager.FinishedStartup) // Placement during game startup
        {
            Placed = true;
            StartCoroutine(Initialize(null, 0.5f));
        }
        else // Placement during gameplay
        {
            Placed = false;
            GetComponent<NavMeshObstacle>().enabled = false;
            StartCoroutine(EnablePlacement());  
            StartCoroutine(Initialize(null, 0.01f));
        }
    }
    public void Init(BuildingSaveData data = null, float wait = 0.3f)
    {
        StartCoroutine(Initialize(data, wait));
    }
    IEnumerator Initialize(BuildingSaveData data, float wait)
    {
        if(data == null) Debug.Log($"PlaceableObject.Initialize(null, {wait})");
        else Debug.Log($"PlaceableObject.Initialize(data, {wait})");
        yield return new WaitForSeconds(wait);
        if(!_initialized)
        {
            _initialized = true;
            
            GetColliderVertexPositionsLocal();
            CalculateSizeInCells();

            if(data == null)
            {
                buildingId = GameManager.GenerateId(IdType.Building, this.gameObject);
                _cutDownjobIndex = -1;
                _jobIndex = -1;

                if(Placed)
                {
                    // Placed during startup but no save data -> default object
                    PlaceInStartup();
                    if(_growthProgress >= 1f) FinishGrowth();
                } 
            }
            else
            {
                buildingId = data.buildingId;
                GameManager.AddId(IdType.Building, buildingId, this.gameObject);

                buildingId = data.buildingId;
                Size = data.size;
                _startTile = data.startTile;
                _growthProgress = data.growthProgress;
                _cutDownjobIndex = data.cutDownjobIndex;
                _jobIndex = data.jobIndex;
                
                GetComponent<OpenPopUpOnClick>().Init();

                PlaceInStartup();
            }
        } 
    }
    IEnumerator EnablePlacement()
    {
        // Prevent instant placement
        yield return new WaitForSeconds(0.2f);
        Placeable = true;
    }
    public BuildingSaveData FormSaveData()
    {
        return new BuildingSaveData(   
                        gameObject.transform.position, gameObject.transform.rotation, 
                        objectInfo.name, buildingId,
                        Size, _startTile, 
                        _growthProgress, _cutDownjobIndex, 
                        _jobIndex);
    }


    private void GetColliderVertexPositionsLocal()
    {
        BoxCollider b = gameObject.GetComponent<BoxCollider>();
        _vertices = new Vector3[4];
        _vertices[0] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
        _vertices[1] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
        _vertices[2] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
        _vertices[3] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
    }

    private void CalculateSizeInCells()
    {
        Vector3Int[] vertices = new Vector3Int[_vertices.Length];

        for(int i=0; i<_vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(_vertices[i]);
            vertices[i] = BuildingSystem.GridLayout.WorldToCell(worldPos);
        }

        Size = new Vector3Int(  x:(Math.Abs((vertices[0] - vertices[1]).x)), 
                                y:(Math.Abs((vertices[0] - vertices[3]).y)),
                                z:1);
        // Fix size into a square which a center cell?

    }

    public Vector3 GetStartPosition()
    {
        /*
        Debug.Log("StartPosition: " + transform.TransformPoint(_vertices[0]));
        Debug.Log("Other positions: " + transform.TransformPoint(_vertices[1])
                + ", " + transform.TransformPoint(_vertices[2])
                + ", " + transform.TransformPoint(_vertices[3]));
        */
        return transform.TransformPoint(_vertices[0]);
    }

    public void Place(Vector3Int start)
    {
        Debug.Log($"Placing {this.gameObject.name}...");

        if(!GameManager.TryPay(BuildingCost))
        {
            Debug.Log($"Failed to place {this.gameObject.name}, due to cost");
            Destroy(this.gameObject);
            return;
        }

        Destroy(gameObject.GetComponent<ObjectDrag>());
        GetComponent<NavMeshObstacle>().enabled = true;
        _cuttable = false;
        Placed = true;
        _startTile = start;
        BuildingSystem.TakeArea(start, Size);

        _originalScale = transform.localScale;
        _ticSize = 1f / _growthTics;
        
        FinishPlacing();
        this.gameObject.GetComponent<OpenPopUpOnClick>().Init();
        if(_requireGrowth) StartCoroutine(InitialGrowth());
        else if(_requireConstruction) StartConstruction();
        if(_requireConstruction && !_finishedConstruction) _constructionObject.SetActive(false);
    }
    public void PlaceInStartup()
    {
        Vector3Int start = BuildingSystem.GridLayout.WorldToCell(GetStartPosition());

        Debug.Log("Placing (" + this.gameObject.name + ") in startup...");
        
        GetComponent<NavMeshObstacle>().enabled = true;
        _cuttable = false;
        _startTile = start;
        BuildingSystem.TakeArea(start, Size);

        _originalScale = transform.localScale;
        _ticSize = 1f / _growthTics;
        
        FinishPlacing();
        this.gameObject.GetComponent<OpenPopUpOnClick>().Init();

        // Adjust growth to match savedata if still growing
        if(_requireGrowth)
        {
            if(_growthProgress < 1f)
            {
                transform.localScale = (_originalScale * _ticSize);
                float adjustedGrowth = _ticSize;
                while(adjustedGrowth < _growthProgress)
                {
                    transform.localScale += (_originalScale * _ticSize);
                    adjustedGrowth += _ticSize;
                }
                StartCoroutine(ManageGrowth());
            }
        } 
        else if(_requireConstruction && !_finishedConstruction) StartConstruction();
        
        if(_requireConstruction && !_finishedConstruction) _constructionObject.SetActive(false);
    }
    public virtual void FinishPlacing()
    {
        // Override
    }
    IEnumerator InitialGrowth()
    {
        yield return new WaitForSeconds(0.1f);
        if(_growthProgress < 0.1f)
        {
            // Run first growth tick to show a little tree
            transform.localScale = (_originalScale * _ticSize);
            _growthProgress = _ticSize;
        }
        StartCoroutine(ManageGrowth());
    }

    IEnumerator ManageGrowth()
    {
        while(_growthProgress < 1f)
        {
            yield return new WaitForSeconds(
                1f / GameManager.GetGrowthMultiplier() * _growTime / _growthTics);
            transform.localScale += (_originalScale * _ticSize);
            _growthProgress += _ticSize;
        }
        // Default behaviour for finishing growth
        if(_requireConstruction)
        {
            if(_growthSpeedIncreasePercent != 0f)
                GameManager.AdjustGrowthMultiplier(0.5f * _growthSpeedIncreasePercent);
            StartConstruction();
        }
        else
        {
            if(_spawnGrass) GrassSystem.AddGrassSpawnLocationArea(_startTile, Size);
            if(_growthSpeedIncreasePercent != 0f)
                GameManager.AdjustGrowthMultiplier(_growthSpeedIncreasePercent);
        }
        MessageLog.NewMessage(new MessageData($"{objectInfo.name} has finished growing.", 
                                                MessageType.Unimportant));
        FinishGrowth();
    }
    public virtual void FinishGrowth()
    {
        // Override if necessery
    }
    public void StartConstruction()
    {
        Debug.Log($"Starting construction job for {gameObject.name}");
        Job newJob = new Job(   JobType.Build, Resource.None,
                                this.buildingId, this._constructionObject.transform.position, 
                                _constructionTime, _constructionDistance);
        _jobIndex = JobManager.QueueJob(newJob, true);
    }
    public void Construct()
    {
        this._constructionObject.SetActive(true);
        this._finishedConstruction = true;
        if(_spawnGrass) GrassSystem.AddGrassSpawnLocationArea(_startTile, Size);
        if(_growthSpeedIncreasePercent != 0f)
        {
            if(_requireGrowth) 
                GameManager.AdjustGrowthMultiplier(0.5f * _growthSpeedIncreasePercent);
            else GameManager.AdjustGrowthMultiplier(_growthSpeedIncreasePercent);
        }

        MessageLog.NewMessage(new MessageData($"{objectInfo.name} has finished construction.", 
                                                MessageType.Unimportant));
        FinishConstruction();
    }
    public virtual void FinishConstruction()
    {
        // Override if necessery
    }


    public void Cut(bool rewardLumber = true)
    {
        if(rewardLumber) GameManager.AddResource(Resource.Lumber, _lumberValue);
        Unplace();
    }
    public void Unplace()
    {
        float appliedGrowthMultiplier = 0f;
        if(_requireGrowth)
        {
            if(_growthProgress >= 1f)
            {
                if(!_requireConstruction || _finishedConstruction) appliedGrowthMultiplier = -1f;
                else appliedGrowthMultiplier = -0.5f;
            }
        }
        else if(_requireConstruction && _finishedConstruction)
        {
            appliedGrowthMultiplier = -1f;
        }
        if(_growthSpeedIncreasePercent != 0f)
            GameManager.AdjustGrowthMultiplier(appliedGrowthMultiplier * _growthSpeedIncreasePercent);
        if(_spawnGrass) Debug.LogError("TODO remove spawn grass area");

        PrepUnplace();
        BuildingSystem.ReleaseArea(_startTile, Size);
        if(_jobIndex != -1) JobManager.RemoveJob(_jobIndex);
        if(_cutDownjobIndex != -1) JobManager.RemoveJob(_cutDownjobIndex);
        GameManager.RemoveId(IdType.Building, buildingId);
        Destroy(this.gameObject);
    }
    public virtual void PrepUnplace()
    {
        // Override
    }

    public void MakeCuttable(bool val = true)
    {
        if(val)
        {
            _cutDownjobIndex = JobManager.QueueJob(
                new Job( JobType.Build, _cutDownResourceType,
                        this.buildingId, this.transform.position, 
                        _woodCuttingTime, _woodCuttingDistance,
                        -1, _deconstructType), true);
        }
        else
        {
            JobManager.RemoveJob(_cutDownjobIndex);
        }
        _cuttable = val;
    }
    /*
    public bool PayCost()
    {
        foreach(Cost cost in BuildingCost)
        {
            if(GameManager.GetResource(cost.type) < cost.amount);
            {
                Debug.Log($"Not enought {cost.type}, {GameManager.GetResource(cost.type)}<{cost.amount}");
                return false;
            }
        }
        foreach(Cost cost in BuildingCost)
        {
            GameManager.AddResource(cost.type, -1 * cost.amount);
        }
        return true;
    }*/
}
