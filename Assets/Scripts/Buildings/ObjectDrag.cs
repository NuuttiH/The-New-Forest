using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;
    private PlaceableObject objectScript;

    void Awake()
    {
        offset = transform.position = BuildingSystem.GetMouseWorldPosition();
        objectScript = this.gameObject.GetComponent<PlaceableObject>();
        objectScript.SetAreaSprite(true);
    }

    void Update()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition();// + offset;
        transform.position = BuildingSystem.SnapCoordinateToGrid(pos);
    }

    void OnDestroy()
    {
        objectScript.SetAreaSprite(false);
    }
}
