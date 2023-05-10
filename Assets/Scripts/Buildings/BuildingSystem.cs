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


    public GameObject testPrefab;
    public LayerMask groundLayer;

    private PlaceableObject _objectToPlace;
    private bool _placementBlock = false;
    private Vector3Int _previousOverlayStart;
    private TileBase[] _previousOverlayTiles;
    private Vector3Int _previousSize;
    
    private void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
      }
      _grid = GridLayout.gameObject.GetComponent<Grid>();
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
            if(CanBePlaced(_objectToPlace))
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
        if(Physics.Raycast(ray, out RaycastHit hit, 500f, _instance.groundLayer))
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
            array[counter++] = tilemap.GetTile(pos);
        }

        return array;
    }

    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = GridLayout.WorldToCell(_objectToPlace.GetStartPosition());
        area.size = placeableObject.Size;

        TileBase[] baseArray = GetTilesBlock(area, _mainTilemap);

        foreach(var b in baseArray)
        {
            if(b == _occupiedTile || b == _overlapTile) return false;
        }
        if(placeableObject.RequireGrass) return GrassSystem.HasGrass(area);
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
        BoundsInt area = new BoundsInt();
        area.SetMinMax(cellPos, cellPos + size);
        TileBase[] originalArray = GetTilesBlock(area, _instance._mainTilemap);
        TileBase[] tileArray = new TileBase[size.x * size.y * size.z];
        TileBase[] grassTileArray = null;
        if(requireGrass) grassTileArray = GrassSystem.GetTilesBlock(area, null);
        for(int index = 0; index < tileArray.Length; index++)
        {
            if(originalArray[index] == null && (!requireGrass || grassTileArray[index] != null)) 
            {
                // empty and (doesn't require grass or has grass)
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
            _instance._mainTilemap.SetTilesBlock(previousArea, _instance._previousOverlayTiles);

            _instance._previousOverlayTiles = null;
        }
    }
}
