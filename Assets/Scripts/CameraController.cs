using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 50f;
    public float panBorderThickness = 15f;
    public Vector2 panLimitX;
    public Vector2 panLimitZ;

    public float scrollSpeed = 25f;
    public float minY = 35f;
    public float maxY = 75f;


    void Update()
    {
        Vector3 pos = transform.position;
        
        if (Input.GetKey("up") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += cameraSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("down") || Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= cameraSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("right") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += cameraSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey("left") || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= cameraSpeed * Time.unscaledDeltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.unscaledDeltaTime;

        pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);

        transform.position = pos;
    }
}
