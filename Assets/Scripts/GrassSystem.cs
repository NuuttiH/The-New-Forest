using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private List<Vector2Int> _partialGrassTiles;
    private HashSet<Vector2Int> _partialGrassTilesSet;
    private HashSet<Vector2Int> _fullGrassTiles;
    private List<Vector2Int> _extraGrassSpawnLocations;
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
        _partialGrassTiles = new List<Vector2Int>();
        _partialGrassTilesSet = new HashSet<Vector2Int>();
        _fullGrassTiles = new HashSet<Vector2Int>();
        _extraGrassSpawnLocations = new List<Vector2Int>();
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
            Debug.Log($"GrassSystem: default initialization");

            // Read existing grass data from level tilemap
            foreach(var pos in _instance._mainTilemap.cellBounds.allPositionsWithin)
            {
                if(_instance._mainTilemap.HasTile(pos))
                {
                    Vector2Int pos2D = new Vector2Int(pos.x, pos.y);
                    _partialGrassTiles.Add(pos2D);
                    _partialGrassTilesSet.Add(pos2D);
                    //Debug.Log($"GrassSystem: Detected grass tile in position: {pos2D.x}, {pos2D.y}");
                }
            }

            // Check if grass tiles are fully grown
            foreach(Vector2Int tilePosition in _partialGrassTiles)
            {
                Vector2Int pos = new Vector2Int(tilePosition.x - 1, tilePosition.y + 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x, tilePosition.y + 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x + 1, tilePosition.y + 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;

                pos = new Vector2Int(tilePosition.x - 1, tilePosition.y);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x, tilePosition.y);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x + 1, tilePosition.y);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;

                pos = new Vector2Int(tilePosition.x - 1, tilePosition.y - 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x, tilePosition.y - 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;
                pos = new Vector2Int(tilePosition.x + 1, tilePosition.y - 1);
                if(!_fullGrassTiles.Contains(pos) && !_partialGrassTilesSet.Contains(pos)) continue;

                // If no continue triggered, all surroundings tiles also have grass
                _fullGrassTiles.Add(tilePosition);
                //Debug.Log($"GrassSystem: Detected fully grown position: {tilePosition.x}, {tilePosition.y}");
            }
            foreach(Vector2Int tilePosition in _fullGrassTiles)
            {
                _partialGrassTiles.Remove(tilePosition);
                _partialGrassTilesSet.Remove(tilePosition);
            }

            StartCoroutine(GrowGrass());
        }
    }

    public static void LoadGrassTiles(  List<Vector2Int> partialGrassTiles, 
                                        HashSet<Vector2Int> fullGrassTiles,
                                        List<Vector2Int> extraGrassSpawnLocations)
    {
        _instance._initialized = true;
        Debug.Log($"GrassSystem: Initialization from save data of {partialGrassTiles.Count} partial and {fullGrassTiles.Count} full grass tiles");
        
        _instance._partialGrassTiles = partialGrassTiles;
        foreach(Vector2Int partialGrass2D in partialGrassTiles)
        {
            _instance._partialGrassTilesSet.Add(partialGrass2D);
            Vector3Int grass3D = new Vector3Int(partialGrass2D.x, partialGrass2D.y, 0);
            _instance._mainTilemap.SetTile(grass3D, _instance._grassTile);
        }
        _instance._fullGrassTiles = fullGrassTiles;
        foreach(Vector2Int fullGrass2D in fullGrassTiles)
        {
            Vector3Int grass3D = new Vector3Int(fullGrass2D.x, fullGrass2D.y, 0);
            _instance._mainTilemap.SetTile(grass3D, _instance._grassTile);
        }
        _instance._extraGrassSpawnLocations = extraGrassSpawnLocations;

        _instance.StartCoroutine(_instance.GrowGrass());
    }
    public static List<Vector2Int> GetPartialGrassTiles()
    {
        return _instance._partialGrassTiles;
    }
    public static HashSet<Vector2Int> GetFullGrassTiles()
    {
        return _instance._fullGrassTiles;
    }
    public static List<Vector2Int> GetExtraGrassSpawns()
    {
        return _instance._extraGrassSpawnLocations;
    }

    public static void RecordNewGrassTile(Vector2Int grass2D)
    {
        Debug.Log("GrassSystem: Recording tile: " + grass2D.x + ", " + grass2D.y);
        _instance._partialGrassTiles.Add(grass2D);
        _instance._partialGrassTilesSet.Add(grass2D);
        Vector3Int grass3D = new Vector3Int(grass2D.x, grass2D.y, 0);
        _instance._mainTilemap.SetTile(grass3D, _instance._grassTile);
        _instance._extraGrassSpawnLocations.Remove(grass2D);
        MissionManager.onIncrementMission(MissionGoal.NewGrass, 1);
    }
    public static void RecordGrassSpawnLocation(Vector2Int grass2D)
    {
        if(!_instance._fullGrassTiles.Contains(grass2D) 
        && !_instance._partialGrassTiles.Contains(grass2D))
        {
            _instance._extraGrassSpawnLocations.Add(grass2D);
        }   
    }

    IEnumerator GrowGrass()
    {
        // Spawn new grass tiles adjacent to existing ones
        while(true)
        {
            Debug.Log("GrassSystem: Trying to grow grass...");
            // Wait for a time depending on growth speed
            float waitTime = _instance._baseGrassGrowthWaitTime * (1f / GameManager.GetGrowthMultiplier());
            if(_instance._reducedWait || waitTime < 1f)
            {
                waitTime = 1f;
                _instance._reducedWait = false;
            }

            yield return new WaitForSeconds(waitTime);

            // Randomize (50/50) growth on either extra spawn location or existing grass tile
            if(_instance._extraGrassSpawnLocations.Count > 0 && Random.Range(0, 1) == 0)
            {
                // Choose random extra tile
                Vector2Int grass2D = _instance._extraGrassSpawnLocations[
                    Random.Range(0, (_instance._extraGrassSpawnLocations.Count - 1))];
                RecordNewGrassTile(grass2D);
            }
            else
            {
                // Choose random grass tile
                Vector2Int grass2D = _instance._partialGrassTiles[
                    Random.Range(0, (_instance._partialGrassTiles.Count - 1))];
                
                Vector2Int[] tiles = GetAdjacentTiles(grass2D);

                int i = Random.Range(0, tiles.Length - 1);
                int ii = i+1;
                if(ii == tiles.Length) ii = 0;

                // Search for adjacent tile without grass
                if(_instance._fullGrassTiles.Contains(tiles[i]))
                {
                    _reducedWait = true;
                }
                else
                {
                    if(!_instance._partialGrassTilesSet.Contains(tiles[i]))
                    {
                        RecordNewGrassTile(tiles[i]);
                    }
                    else while(ii != i)
                    {
                        if(!_instance._partialGrassTilesSet.Contains(tiles[ii]))
                        {
                            RecordNewGrassTile(tiles[ii]);
                        }
                        else
                        {
                            ii++;
                            if(ii > 7) ii = 0;
                        }
                    }

                    if(i == ii)
                    {
                        // All adjacent tiles were grown
                        _instance._partialGrassTiles.Remove(grass2D);
                        _instance._partialGrassTilesSet.Remove(grass2D);
                        _instance._fullGrassTiles.Add(grass2D);
                        _reducedWait = true;
                        Debug.Log($"GrassSystem: Chosen tile ({grass2D.x}, {grass2D.y}) fully grown!");
                    }
                }
            }
        }
    }
    public static Vector2Int[] GetAdjacentTiles(Vector2Int grass2D)
    {
        Vector2Int[] tiles = new Vector2Int[8];
        
        tiles[0] = new Vector2Int(grass2D.x, grass2D.y + 1);
        tiles[1] = new Vector2Int(grass2D.x - 1, grass2D.y);
        tiles[2] = new Vector2Int(grass2D.x + 1, grass2D.y);
        tiles[3] = new Vector2Int(grass2D.x, grass2D.y - 1);

        return tiles;
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
    public static void AddGrassSpawnLocationArea(Vector3Int start, Vector3Int size)
    {
        Debug.Log("GrassSystem: AddGrassSpawnLocationArea(" + start + ", " + size);
        
        for(int x = start.x; x < (start.x + size.x); x++)
        {
            for(int y = start.y; y < (start.y + size.y); y++)
            {
                RecordGrassSpawnLocation(new Vector2Int(x, y));
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

        return _instance._fullGrassTiles.Contains(new Vector2Int(gridLocation.x, gridLocation.y));
    }
    public static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
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
