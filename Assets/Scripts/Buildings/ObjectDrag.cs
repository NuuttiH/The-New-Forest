using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3Int _cellPos;
    private Vector3 _offset;
    private PlaceableObject _objectScript;

    void Awake()
    {
        _cellPos = new Vector3Int();
        transform.position = BuildingSystem.GetMouseWorldPosition();
        _objectScript = this.gameObject.GetComponent<PlaceableObject>();
    }

    void Update()
    {
        if(!_objectScript.Initialized) return;

        Vector3 pos = BuildingSystem.GetMouseWorldPosition();
        if(pos == Vector3.zero) // Invalid position
        {
            return;
        }
        
        Vector3Int newCellPos = BuildingSystem.GridLayout.WorldToCell(pos);
        if(newCellPos != _cellPos)
        {
            transform.position = BuildingSystem.SnapCoordinateToGrid(pos);
            BuildingSystem.SetDragOverlay(_objectScript.GetStartPosition(), _objectScript.Size, _objectScript.RequireGrass);
            _cellPos = newCellPos;
        }
    }
}
