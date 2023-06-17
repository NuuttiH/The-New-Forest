using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public struct ExtraGrassSpawn
{
    public Vector2Int location;
    public List<Vector2Int> extraSpawnLocations;  // Other linked spawn locations

    public ExtraGrassSpawn(Vector2Int location)
    {
        this.location = location;
        this.extraSpawnLocations = new List<Vector2Int>();
    }
}

public class GrassSystem : MonoBehaviour
{
    private static GrassSystem _instance;
    private bool _initialized = false;

    [SerializeField] private  GridLayout _gridLayout;
    public static GridLayout GridLayout { get { return _instance._gridLayout; } }
    private Grid _grid;
    [SerializeField] private Tilemap _mainTilemap;
    [SerializeField] private TileBase _grassTile;
    [SerializeField] private TilemapRenderer _tileMapRenderer;

    private List<Vector2Int> _potentialGrassTilesList;
    private HashSet<Vector2Int> _potentialGrassTilesSet;
    private HashSet<Vector2Int> _grassTilesSet;
    private HashSet<Vector2Int> _extraGrassGrowthCores;
    private bool _reducedWait = false;

    [SerializeField] private float _baseGrassGrowthWaitTime = 10f;
    
    private void Awake()
    {
        if(_instance == null) _instance = this;
        else
        {
            Destroy(this);
            return;
        }
        _grid = GridLayout.gameObject.GetComponent<Grid>();
        _potentialGrassTilesList = new List<Vector2Int>();
        _potentialGrassTilesSet = new HashSet<Vector2Int>();
        _grassTilesSet = new HashSet<Vector2Int>();
        _extraGrassGrowthCores = new HashSet<Vector2Int>();
    }
    
    void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize(float wait = 0.3f)
    {
        // LoadGrassTiles handles initialization when loading save
        yield return new WaitForSeconds(wait);
        if(!_initialized) 
        {
            _initialized = true;
            Debug.Log($"GrassSystem: default initialization, tilemap.CellBounds: {_instance._mainTilemap.cellBounds}");

            // Read existing grass data from level tilemap
            foreach(var pos in _instance._mainTilemap.cellBounds.allPositionsWithin)
            {
                if(_instance._mainTilemap.HasTile(pos))
                {
                    Vector2Int pos2D = new Vector2Int(pos.x, pos.y);
                    _grassTilesSet.Add(pos2D);
                    _potentialGrassTilesSet.Remove(pos2D);
                    
                    // Check nearby spots as potential growth
                    Vector2Int nearbyPosition = new Vector2Int(pos2D.x, pos2D.y + 1);
                    if(!_grassTilesSet.Contains(nearbyPosition)) 
                        _potentialGrassTilesSet.Add(nearbyPosition);

                    nearbyPosition = new Vector2Int(pos2D.x + 1, pos2D.y);
                    if(!_grassTilesSet.Contains(nearbyPosition)) 
                        _potentialGrassTilesSet.Add(nearbyPosition);

                    nearbyPosition = new Vector2Int(pos2D.x, pos2D.y - 1);
                    if(!_grassTilesSet.Contains(nearbyPosition)) 
                        _potentialGrassTilesSet.Add(nearbyPosition);

                    nearbyPosition = new Vector2Int(pos2D.x - 1, pos2D.y);
                    if(!_grassTilesSet.Contains(nearbyPosition)) 
                        _potentialGrassTilesSet.Add(nearbyPosition);
                }
            }

            foreach(Vector2Int pos2D in _potentialGrassTilesSet)
            {
                _potentialGrassTilesList.Add(pos2D);
            }

            StartCoroutine(GrowGrass());
        }
    }

    public static void LoadGrassTiles(  List<Vector2Int> potentialGrassTiles, 
                                        HashSet<Vector2Int> grassTiles)
    {
        _instance._initialized = true;
        Debug.Log($"GrassSystem: Initialization from save data of {potentialGrassTiles.Count} potential and {grassTiles.Count} grown grass tiles");
        
        _instance._potentialGrassTilesList = potentialGrassTiles;
        foreach(Vector2Int pos2D in potentialGrassTiles)
        {
            _instance._potentialGrassTilesSet.Add(pos2D);
            Vector3Int pos3D = new Vector3Int(pos2D.x, pos2D.y, 0);
            _instance._mainTilemap.SetTile(pos3D, _instance._grassTile);
        }
        _instance._grassTilesSet = grassTiles;
        foreach(Vector2Int pos2D in grassTiles)
        {
            Vector3Int pos3D = new Vector3Int(pos2D.x, pos2D.y, 0);
            _instance._mainTilemap.SetTile(pos3D, _instance._grassTile);
        }

        _instance.StartCoroutine(_instance.GrowGrass());
    }
    public static List<Vector2Int> GetPartialGrassTiles()
    {
        return _instance._potentialGrassTilesList;
    }
    public static HashSet<Vector2Int> GetFullGrassTiles()
    {
        return _instance._grassTilesSet;
    }

    public static void RecordNewGrassTile(Vector2Int grass2D)
    {
        Debug.Log("GrassSystem: Recording tile: " + grass2D.x + ", " + grass2D.y);

        _instance._grassTilesSet.Add(grass2D);
        _instance._potentialGrassTilesSet.Remove(grass2D);
        _instance._potentialGrassTilesList.Remove(grass2D);
        
        // Check nearby spots as potential growth
        Vector2Int nearbyPosition = new Vector2Int(grass2D.x, grass2D.y + 1);
        if(!_instance._grassTilesSet.Contains(nearbyPosition))
        {
            _instance._potentialGrassTilesList.Add(nearbyPosition);
            _instance._potentialGrassTilesSet.Add(nearbyPosition);
        }

        nearbyPosition = new Vector2Int(grass2D.x + 1, grass2D.y);
        if(!_instance._grassTilesSet.Contains(nearbyPosition)) 
        {
            _instance._potentialGrassTilesList.Add(nearbyPosition);
            _instance._potentialGrassTilesSet.Add(nearbyPosition);
        }

        nearbyPosition = new Vector2Int(grass2D.x, grass2D.y - 1);
        if(!_instance._grassTilesSet.Contains(nearbyPosition))
        {
            _instance._potentialGrassTilesList.Add(nearbyPosition);
            _instance._potentialGrassTilesSet.Add(nearbyPosition);
        }

        nearbyPosition = new Vector2Int(grass2D.x - 1, grass2D.y);
        if(!_instance._grassTilesSet.Contains(nearbyPosition))
        {
            _instance._potentialGrassTilesList.Add(nearbyPosition);
            _instance._potentialGrassTilesSet.Add(nearbyPosition);
        }

        Vector3Int grass3D = new Vector3Int(grass2D.x, grass2D.y, 0);
        _instance._mainTilemap.SetTile(grass3D, _instance._grassTile);
        MissionManager.onIncrementMission(MissionGoal.NewGrass, 1);
    }

    IEnumerator GrowGrass()
    {
        // Spawn new grass tiles adjacent to existing ones
        while(true)
        {
            // Wait for a time depending on growth speed
            float waitTime = _instance._baseGrassGrowthWaitTime * GameManager.GetGrowthMultiplier();
            if(_instance._reducedWait || waitTime < 0.5f)
            {
                waitTime = 0.5f;
                _instance._reducedWait = false;
            }
            Debug.Log($"GrassSystem: Trying to grow grass...(waiTime: {waitTime})");

            yield return new WaitForSeconds(waitTime);

            // Choose random grass tile
            Vector2Int grass2D = _instance._potentialGrassTilesList[
                Random.Range(0, (_instance._potentialGrassTilesList.Count - 1))];
            
            
            if(!_instance._grassTilesSet.Contains(grass2D))
            {
                RecordNewGrassTile(grass2D);
            }
            else
            {
                _instance._potentialGrassTilesList.Remove(grass2D);
                _instance._potentialGrassTilesSet.Remove(grass2D);
                _reducedWait = true;
                Debug.Log($"GrassSystem: Chosen tile ({grass2D}) fully grown!");
            }
        }
    }
    
    public static void TakeArea(Vector3Int start, Vector3Int size)
    {
        Debug.Log("GrassSystem: TakeArea(" + start + ", " + size);
        
        for(int x = start.x; x < (start.x + size.x); x++)
        {
            for(int y = start.y; y < (start.y + size.y); y++)
            {
                RecordNewGrassTile(new Vector2Int(x, y));
            }
        }
    }

    public static void AddExtraGrassSpawnArea(Vector3Int start, Vector3Int size)
    {
        Vector2Int core = new Vector2Int(start.x, start.y);
        // Center core if size dimension >= 3
        if(size.x >= 3) core.x += Mathf.FloorToInt((size.x - 1) / 2);
        if(size.y >= 3) core.y += Mathf.FloorToInt((size.y - 1) / 2);

        if(_instance._extraGrassGrowthCores.Contains(core))
        {
            Debug.Log($"GrassSystem: AddExtraGrassSpawnArea({start}, {size}), error, core location ({core}) is already in use");
            return;
        }

        ExtraGrassSpawn newSpawn = new ExtraGrassSpawn(core);

        string s = "";
        
        for(int x = start.x; x < (start.x + size.x); x++)
        {
            for(int y = start.y; y < (start.y + size.y); y++)
            {
                Vector2Int location = new Vector2Int(x, y);
                if(!HasGrass(location))
                {
                    newSpawn.extraSpawnLocations.Add(location);
                    s += $"{location}, ";
                }
            }
        }
        _instance._extraGrassGrowthCores.Add(core);
        _instance.StartCoroutine(_instance.ExtraGrowGrass(newSpawn));
        Debug.Log($"GrassSystem: AddExtraGrassSpawnArea({start}, {size}), core location: {core}, other locations {s}");
    }
    public static void RemoveExtraGrassSpawnArea(Vector3Int start, Vector3Int size)
    {
        Vector2Int core = new Vector2Int(start.x, start.y);
        // Center core if size dimension >= 3
        if(size.x >= 3) core.x += Mathf.FloorToInt((size.x - 1) / 2);
        if(size.y >= 3) core.y += Mathf.FloorToInt((size.y - 1) / 2);

        _instance._extraGrassGrowthCores.Remove(core);
    }
    IEnumerator ExtraGrowGrass(ExtraGrassSpawn spawn)
    {
        Debug.Log($"GrassSystem.ExtraGrowGrass in {spawn.location} started...)");
        // Wait for a time depending on random modifier, slightly slower than unmodified base speed
        float waitTime = _instance._baseGrassGrowthWaitTime * Random.Range(0.75f, 2f);
        yield return new WaitForSeconds(waitTime);

        // Grow grass on core location first if possible
        if(!HasGrass(spawn.location))
        {
            RecordNewGrassTile(spawn.location);
        }

        // Spawn new grass tiles adjacent to existing ones
        while(true)
        {
            waitTime = _instance._baseGrassGrowthWaitTime 
                        * GameManager.GetGrowthMultiplier()
                        * Random.Range(0.5f, 1.5f);
            if(waitTime < 3f) waitTime = 3f;
            Debug.Log($"GrassSystem: Trying to grow grass...(waiTime: {waitTime})(extra in {spawn.location})");

            yield return new WaitForSeconds(waitTime);

            // End coroutine if spawn was removed
            if(!_instance._extraGrassGrowthCores.Contains(spawn.location))
            {
                Debug.Log($"GrassSystem: ExtraGrassSpawnArea ({spawn.location}) was removed, ending coroutine!");
                yield break;    // End coroutine
            } 

            // End coroutine if no spawn locations left
            if(spawn.extraSpawnLocations.Count == 0)
            {
                Debug.Log($"GrassSystem: ExtraGrassSpawnArea ({spawn.location}) is fully grown!");
                _instance._extraGrassGrowthCores.Remove(spawn.location);
                yield break;    // End coroutine
            } 

            // Choose random grass tile
            int i = Random.Range(0, (spawn.extraSpawnLocations.Count - 1));
            Vector2Int grass2D = spawn.extraSpawnLocations[i];
            
            if(HasGrass(grass2D)) spawn.extraSpawnLocations.RemoveAt(i);
            else
            {
                RecordNewGrassTile(grass2D);
                spawn.extraSpawnLocations.RemoveAt(i);
                
            }
        }
    }

    public static bool HasGrass(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, _instance._mainTilemap);

        foreach(var b in baseArray)
        {
            if(b!=_instance._grassTile) return false;
        }
        return true;
    }
    public static bool HasGrass(Vector3 location)
    {
        Vector3Int gridLocation = GridLayout.WorldToCell(location);

        return _instance._grassTilesSet.Contains(new Vector2Int(gridLocation.x, gridLocation.y));
    }
    public static bool HasGrass(Vector3Int gridLocation)
    {
        return _instance._grassTilesSet.Contains(new Vector2Int(gridLocation.x, gridLocation.y));
    }
    public static bool HasGrass(Vector2Int gridLocation)
    {
        return _instance._grassTilesSet.Contains(gridLocation);
    }
    public static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        if(tilemap == null) tilemap = _instance._mainTilemap;
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, z:0);
            array[counter++] = tilemap.GetTile(pos);
        }

        return array;
    }
}
