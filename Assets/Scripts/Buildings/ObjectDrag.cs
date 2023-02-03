using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;

    void Awake()
    {
        offset = transform.position = BuildingSystem.GetMouseWorldPosition();
    }

    void Update()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition();// + offset;
        transform.position = BuildingSystem.SnapCoordinateToGrid(pos);
    }
}
