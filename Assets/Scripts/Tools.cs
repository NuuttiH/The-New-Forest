using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static float PlayAudio(GameObject parent, AudioEvent audio, bool dontDestroyOnLoad = false)
    {
        var audioPlayer = new GameObject("AudioPlayer", typeof (AudioSource)).GetComponent<AudioSource>();
        if(parent == null) parent = Camera.main.gameObject;
        audioPlayer.transform.position = parent.transform.position;
        audio.Play(audioPlayer);
        
        float duration = audioPlayer.clip.length*audioPlayer.pitch;
        if(dontDestroyOnLoad) GameObject.DontDestroyOnLoad(audioPlayer.gameObject);
        UnityEngine.Object.Destroy(audioPlayer.gameObject, duration);
        
        //Debug.Log($"Tools.PlayAudio({parent.name}, {audio.name}, {dontDestroyOnLoad})");
        return duration;
    }

    // Returns coordinates of specified corner of a game object
    public static Vector2 GetCorner(GameObject gameObject, Corner corner)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        float halfWidth = rect.rect.width / 2;
        float halfHeight = rect.rect.height / 2;
        Vector2 cornerPosition = gameObject.transform.position; 

        switch(corner)
        {
            case Corner.TopLeft:
                cornerPosition.x -= halfWidth;
                cornerPosition.y += halfHeight;
                break;
            case Corner.TopRight:
                cornerPosition.x += halfWidth;
                cornerPosition.y += halfHeight;
                break;
            case Corner.BottomRight:
                cornerPosition.x += halfWidth;
                cornerPosition.y -= halfHeight;
                break;
            case Corner.BottomLeft:
                cornerPosition.x -= halfWidth;
                cornerPosition.y -= halfHeight;
                break;
        }
        return cornerPosition;
    }

    // UI functions

    // TODO Improve panel position 
    public static void AdjustPanelPlacementInCanvasToMousePos(Canvas canvas, GameObject panel)
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, Input.mousePosition,
            canvas.worldCamera,
            out mousePos);

        // Determine screen corner to direct the panel towards
        Corner screenCorner;
        Corner panelCorner;
        if(mousePos.x < 0)
        {
            if(mousePos.y < 0)
            {
                Debug.Log("bottomleft");
                screenCorner = Corner.BottomLeft;
                panelCorner = Corner.TopRight;
            }
            else
            {
                Debug.Log("topleft");
                screenCorner = Corner.TopLeft;
                panelCorner = Corner.BottomRight;
            }
        }
        else
        {
            if(mousePos.y < 0)
            {
                Debug.Log("bottomright");
                screenCorner = Corner.BottomRight;
                panelCorner = Corner.TopLeft;
            }
            else
            {
                Debug.Log("topright");
                screenCorner = Corner.TopRight;
                panelCorner = Corner.BottomLeft;
            }
        }

        panel.transform.position = canvas.transform.TransformPoint(mousePos);
        Vector2 newPos = GetCorner(panel, panelCorner);

        Vector2 newPosFinal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, newPos,
            canvas.worldCamera,
            out newPosFinal);

        panel.transform.position = canvas.transform.TransformPoint(newPosFinal);
    }
}
