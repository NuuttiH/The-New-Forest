using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Corner { TopLeft, TopRight, BottomRight, BottomLeft }

public class OpenPopUpOnClick : MonoBehaviour
{
    [SerializeField] private GameObject _UIPrefab;
    [SerializeField] private ObjectInfo _objectInfo;
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
            _popUpMenu.Init(this.gameObject, _objectInfo);

            Tools.AdjustPanelPlacementInCanvasToMousePos(
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
}
