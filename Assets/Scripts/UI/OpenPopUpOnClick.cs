using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Corner { TopLeft, TopRight, BottomRight, BottomLeft }

public class OpenPopUpOnClick : MonoBehaviour
{
    [SerializeField] private GameObject _UIPrefab;
    private PopUpMenu _popUpMenu;
    private bool _inUse;


    void Start()
    {
        _popUpMenu = null;
        _inUse = false;
    }
    public void Init()
    {
        // Init is called from the objects' other scripts
        StartCoroutine(EnableUsage());
    }
    IEnumerator EnableUsage()
    {
        yield return new WaitForSeconds(0.5f);
        _inUse = true;
    }

    void OnMouseDown()
    {
        if(_inUse && _popUpMenu == null && !EventSystem.current.IsPointerOverGameObject())
        {
            GameObject obj = Instantiate(_UIPrefab);
            _popUpMenu = obj.GetComponent<PopUpMenu>();
            _popUpMenu.Init(this.gameObject);

            AdjustPanelPlacementInCanvasToMousePos(
                obj.GetComponent<Canvas>(), obj.transform.GetChild(0).gameObject);
        }
    }

    void OnDestroy()
    {
        CloseOldPopUp();
    }

    public void CloseOldPopUp()
    {
        if(_popUpMenu != null)
        {
            Destroy(_popUpMenu.gameObject);
            _popUpMenu = null;
        }
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

    // TODO Improve panel position 
    public void AdjustPanelPlacementInCanvasToMousePos(Canvas canvas, GameObject panel)
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
