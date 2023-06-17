using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    private static BuildingSystem _instance;

    [SerializeField] private  GridLayout _gridLayout;
    public static GridLayout GridLayout { get { return _instance._gridLayout; } }
    private Grid _grid;
    [SerializeField] private Tilemap _mainTilemap;
    [SerializeField] private TileBase _occupiedTile;
    [SerializeField] private TileBase _overlayTile;
    [SerializeField] private TileBase _overlapTile;
    [SerializeField] private TilemapRenderer _tileMapRenderer;
    private int _maxLength;


    public GameObject testPrefab;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private GameObject _noGrassTree;
    [SerializeField] private GameObject _grassTree;
    //[SerializeField] private float _baseTreeGrowthWaitTime = 25f;
    [SerializeField] private float _naturalTreeGrowthModifier = 2.5f;

    private PlaceableObject _objectToPlace;
    private bool _placementBlock = false;
    private Vector3Int _previousOverlayStart;
    private TileBase[] _previousOverlayTiles;
    private Vector3Int _previousSize;
    private int _treeCount;

    private static Vector3 HIDDEN_POSITION = new Vector3(0f, -100f, 0f);
    
    private void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
        }
        _grid = GridLayout.gameObject.GetComponent<Grid>();
        _treeCount = 0;

        StartCoroutine(NaturalTreeGrowth());
    }
    private void Start()
    {
        _maxLength = GameManager.MapSize;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            ResetDragOverlay();
            InitializedWithObject(testPrefab);
        }

        if(!_objectToPlace || _placementBlock) return;

        if(_objectToPlace.Placeable && Input.GetMouseButtonUp(0))
        {
            ResetDragOverlay();
            if(CanBePlaced(_objectToPlace, true))
            {
                Vector3Int start = GridLayout.WorldToCell(_objectToPlace.GetStartPosition());
                _objectToPlace.Place(start);
                _objectToPlace = null;
            }
            else
            {
                Destroy(_objectToPlace.gameObject);
                _objectToPlace = null;
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            ResetDragOverlay();
            Destroy(_objectToPlace.gameObject);
            _objectToPlace = null;
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction *150, Color.green, 10f);
        if(Physics.Raycast(ray, out RaycastHit hit, 500f, _instance._groundLayer))
        {
            //Debug.Log(hit.collider.gameObject.name);
            return hit.point;
        }
        else{
            //Debug.Log("No raycast hit");
            return Vector3.zero;
        } 
    }

    public static Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = GridLayout.WorldToCell(position);
        position = _instance._grid.GetCellCenterWorld(cellPos);
        return position;
    }
    public static Vector3 SnapCoordinateToGrid(Vector3Int cellPos)
    {
        return _instance._grid.GetCellCenterWorld(cellPos);
    }

    public static void InitializedWithObject(GameObject prefab)
    {
        if(_instance._objectToPlace != null)
        {
            Destroy(_instance._objectToPlace.gameObject);
        }

        Vector3 position = SnapCoordinateToGrid(Vector3.zero);
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        _instance._objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();
    }

    public static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        if(tilemap == null) tilemap = _instance._mainTilemap;
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, z:0);

            //if(IsInBounds(pos)) 
            array[counter++] = tilemap.GetTile(pos);
            //else array[counter++] = _instance._occupiedTile;
        }

        return array;
    }
    public static bool IsInBounds(Vector3Int pos)
    {
        int minLength = -1 * _instance._maxLength;
        if(pos.x > _instance._maxLength || pos.x < minLength || pos.y > _instance._maxLength || pos.y < minLength)
            return false;
        return true;
    }

    private bool CanBePlaced(PlaceableObject placeableObject, bool giveErrorMessage = false)
    {
        BoundsInt area = new BoundsInt();
        area.position = GridLayout.WorldToCell(placeableObject.GetStartPosition());
        area.size = placeableObject.Size;

        TileBase[] baseArray = GetTilesBlock(area, _mainTilemap);

        int index = 0;
        Vector3Int[] positions = new Vector3Int[baseArray.Length];
        foreach (Vector3Int point in area.allPositionsWithin) positions[index++] = point;

        for(index = 0; index < baseArray.Length; index++)
        {
            if(baseArray[index] == _occupiedTile 
            || baseArray[index] == _overlapTile
            || IsInBounds(positions[index]) == false)
            {
                if(giveErrorMessage) 
                    MessageLog.NewMessage(new MessageData(
                        $"Can't place '{placeableObject.Name}', location is not available", MessageType.Error));
                return false;
            }   
        }
        if(placeableObject.RequireGrass)
        {
            bool val = GrassSystem.HasGrass(area);
            if(giveErrorMessage && !val) 
                MessageLog.NewMessage(new MessageData(
                    $"Can't place '{placeableObject.Name}', it requires grass", MessageType.Error));
            return val;
        }
        else return true;
    }

    public static void TakeArea(Vector3Int start, Vector3Int size)
    {
        Debug.Log("BuildingSystem: TakeArea(" + start + ", " + size);
        BoundsInt area = new BoundsInt();
        area.SetMinMax(start, start + size);
        Debug.Log("BuildingSystem: area set to (" + area.min + ", " + area.max);
        TileBase[] tileArray = new TileBase[size.x * size.y * size.z];
        for (int index = 0; index < tileArray.Length; index++)
        {
            tileArray[index] = _instance._occupiedTile;
        }
        _instance._mainTilemap.SetTilesBlock(area, tileArray);
    }

    public static void ReleaseArea(Vector3Int start, Vector3Int size)
    {
        Debug.Log("BuildingSystem: ReleaseArea(" + start + ", " + size);
        BoundsInt area = new BoundsInt();
        area.SetMinMax(start, start + size);
        Debug.Log("BuildingSystem: area set to (" + area.min + ", " + area.max);
        TileBase[] tileArray = new TileBase[size.x * size.y * size.z];
        _instance._mainTilemap.SetTilesBlock(area, tileArray);
    }

    public static void ShowGrid(bool visible)
    {
        _instance._tileMapRenderer.enabled = visible;
    }

    public static void BlockPlacement(float releaseTime = 0.01f)
    {
        _instance._placementBlock = true;
        _instance.StartCoroutine(_instance.UnblockPlacement(releaseTime));
    }
    IEnumerator UnblockPlacement(float releaseTime)
    {
        yield return new WaitForSecondsRealtime(releaseTime);
        _instance._placementBlock = false;
    }

    public static bool IsPlacingBuilding()
    {
        return _instance._objectToPlace != null;
    }

    public static void StopBuilding()
    {
        Destroy(_instance._objectToPlace.gameObject);
        _instance._objectToPlace = null;
    }

    public static void SetDragOverlay(Vector3 start, Vector3Int size, bool requireGrass)
    {
        Vector3Int cellPos = GridLayout.WorldToCell(start);
        //Debug.Log("BuildingSystem: SetDragOverlay(" + cellPos + ", " + size);

        // Undo previous changes 
        if(_instance._previousOverlayTiles != null)
        {
            BoundsInt previousArea = new BoundsInt();
            previousArea.SetMinMax( _instance._previousOverlayStart, 
                                    _instance._previousOverlayStart + _instance._previousSize);
            _instance._mainTilemap.SetTilesBlock(previousArea, _instance._previousOverlayTiles);
        }

        // Make new changes
        int index = 0;
        BoundsInt area = new BoundsInt();
        area.SetMinMax(cellPos, cellPos + size);

        Vector3Int[] positions = new Vector3Int[size.x * size.y * size.z];
        foreach (Vector3Int point in area.allPositionsWithin) positions[index++] = point;

        TileBase[] originalArray = GetTilesBlock(area, _instance._mainTilemap);
        TileBase[] tileArray = new TileBase[size.x * size.y * size.z];

        TileBase[] grassTileArray = null;
        if(requireGrass) grassTileArray = GrassSystem.GetTilesBlock(area, null);

        for(index = 0; index < tileArray.Length; index++)
        {
            if(originalArray[index] == null 
            && (!requireGrass || grassTileArray[index] != null)
            && IsInBounds(positions[index])) 
            {
                // empty, (doesn't require grass or has grass), not out of bounds
                tileArray[index] = _instance._overlayTile;
            }
            else
            {
                tileArray[index] = _instance._overlapTile;
            }
        }
        _instance._previousOverlayStart = cellPos;
        _instance._previousOverlayTiles = originalArray;
        _instance._previousSize = size;
        _instance._mainTilemap.SetTilesBlock(area, tileArray);
    }
    public static void ResetDragOverlay()
    {
        // Undo previous changes 
        if(_instance._previousOverlayTiles != null)
        {
            BoundsInt previousArea = new BoundsInt();
            previousArea.SetMinMax( _instance._previousOverlayStart, 
                                    _instance._previousOverlayStart + _instance._previousSize);
            //TileBase[] tiles = CheckAreaForInBound(previousArea, _instance._previousOverlayTiles);
            _instance._mainTilemap.SetTilesBlock(previousArea, _instance._previousOverlayTiles);

            _instance._previousOverlayTiles = null;
        }
    }

    
    IEnumerator NaturalTreeGrowth()
    {
        yield return new WaitForSeconds(3f);
        // Try spawning new trees in random locations
        bool reducedWait = false;
        
        while(true)
        {
            // Wait for a time depending on growth speed
            float waitTime = 35f * GameManager.GetGrowthMultiplier();
            if(waitTime < 18f) waitTime = 18f;
            if(reducedWait)
            {
                waitTime = 5f;
                reducedWait = false;
            }
            Debug.Log($"BuildingSystem: Trying to grow a tree...(waiTime: {waitTime})");

            yield return new WaitForSeconds(waitTime);

            
            // Choose random tile
            Vector3Int tile = new Vector3Int(Random.Range(0, _maxLength), Random.Range(0, _maxLength), 1);

            GameObject potentialBuilding = null;
            if(GrassSystem.HasGrass(tile))
            {
                if(GameManager.GetFlag(Flag.GrassBuildings))
                {
                    potentialBuilding = Instantiate(_instance._grassTree, HIDDEN_POSITION, Quaternion.identity);
                }
            }
            else
            {
                potentialBuilding = Instantiate(_instance._noGrassTree, HIDDEN_POSITION, Quaternion.identity);
            }

            if(potentialBuilding == null)
            {
                reducedWait = true;
            }
            else
            {
                int oldLayer = potentialBuilding.layer;
                potentialBuilding.layer = 10; // "Hide" layer
                PlaceableObject objectToPlace = potentialBuilding.GetComponent<PlaceableObject>();
                objectToPlace.ModifyGrowthTime(_naturalTreeGrowthModifier);

                yield return new WaitForSeconds(1.5f);

                potentialBuilding.transform.position = _instance._grid.GetCellCenterWorld(tile);
                if(CanBePlaced(objectToPlace))
                {
                    Vector3Int start = GridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    potentialBuilding.layer = oldLayer;
                    objectToPlace.Place(start, true);
                    Debug.Log($"BuildingSystem: Tree is naturally growing in {tile}");
                }
                else
                {
                    Destroy(potentialBuilding);
                    Debug.Log($"BuildingSystem: Tree could not grow in {tile}");
                }
            }
        }
    }

    public static void UpdateTreeCount(int val)
    {
        _instance._treeCount += val;
        MissionManager.onIncrementMission(MissionGoal.TreeCount, _instance._treeCount);
    }
    public static int GetTreeCount()
    {
        return _instance._treeCount;
    }
}
