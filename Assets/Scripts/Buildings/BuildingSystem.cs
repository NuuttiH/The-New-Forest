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
    [SerializeField] private TileBase _whiteTile;
    [SerializeField] private TilemapRenderer _tileMapRenderer;
    [SerializeField] private Sprite _areaInUseSprite;


    public GameObject testPrefab;
    public LayerMask groundLayer;

    private PlaceableObject _objectToPlace;
    private bool _placementBlock = false;
    
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
            InitializedWithObject(testPrefab);
        }

        if(!_objectToPlace || _placementBlock) return;

        if(_objectToPlace.Placeable && Input.GetMouseButtonUp(0))
        {
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
            if(b==_whiteTile) return false;
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
            tileArray[index] = _instance._whiteTile;
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

    public static Sprite GetAreaInUseSprite()
    {
        return _instance._areaInUseSprite;
    }
}
